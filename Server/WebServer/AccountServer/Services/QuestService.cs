using AccountServer.Data;
using GameDB;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using DbCurrencyType = GameDB.CurrencyType;
using CurrencyType = AccountServer.Data.CurrencyType;

namespace AccountServer.Services
{
    public class QuestService
    {
        private readonly GameDbContext _dbContext;
        private readonly JwtTokenService _jwt;
        private readonly PlayerService _player;
        IServiceProvider _serviceProvider;

        public QuestService(GameDbContext context, JwtTokenService jwt, PlayerService player, IServiceProvider serviceProvider)
        {
            _dbContext = context;
            _jwt = jwt;
            _player = player;
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> MissionCreateAsync(string jwt, int templateId)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
            if (player == null) return false;

            if (player.Missions.Any(m => m.TemplateId == templateId)) return false;
            if (!DataManager.MissionDataDic.TryGetValue(templateId, out var missionData)) return false;

            var mission = new MissionSaveDataDb
            {
                TemplateId = templateId,
                StackedPoint = 0,
                GetRewardCount = 0,
                MissionState = EMissionState.Progress,
                PlayerDbId = player.PlayerDbId,
            };

            player.Missions.Add(mission);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<GetMissionListRes> MissionListGetAsync(GetMissionListReq request)
        {
            var response = new GetMissionListRes();
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
            {
                response.Success = false;
                response.Message = "Invalid player.";
                return response;
            }

            response.Missions = player.Missions.Select(m => new MissionDTO
            {
                TemplateId = m.TemplateId,
                StackedPoint = m.StackedPoint,
                MissionState = m.MissionState,
                GetRewardCount = m.GetRewardCount,
            }).ToList();

            response.Success = true;
            response.Message = "Mission list retrieved.";
            return response;
        }

        public async Task MissionEventAsncHandle(string jwt, Define.EBroadcastEventType eventType, int value, bool commitChanges = true)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
            if (player == null) return;

            bool saveNeeded = false;

            foreach (var mission in player.Missions)
            {
                if (mission.MissionState != EMissionState.Progress)
                    continue;

                if (!DataManager.MissionDataDic.TryGetValue(mission.TemplateId, out var missionData))
                    continue;

                if (MissionGoalEventMap.TryGetValue(missionData.MissionGoal, out var allowedEvents) && allowedEvents.Contains(eventType))
                {
                    mission.StackedPoint += value;

                    if (mission.StackedPoint >= missionData.MissionCount)
                    {
                        mission.StackedPoint = missionData.MissionCount;
                        mission.MissionState = EMissionState.Rewardable;
                    }

                    saveNeeded = true;

                    _dbContext.Entry(mission).State = EntityState.Modified;
                }
            }

            // Save only if needed and if commitChanges is true
            if (commitChanges && saveNeeded)
                await _dbContext.SaveChangesAsync();

        }

        public async Task<GetMissionListRes> GetNormalMissionReward(GetNormalMissionRewardReq request)
        {
            // step0. Create transaction
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var response = new GetMissionListRes();

            try
            {
                // step1. Extract accountDbId from JWT
                var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);

                // step2. Validate player
                if (player == null)
                {
                    response.Success = false;
                    response.Message = "Invalid player.";
                    return response;
                }

                // step3. Find mission by TemplateId
                var mission = player.Missions.FirstOrDefault(m => m.TemplateId == request.TemplatedId);
                if (mission == null)
                {
                    response.Success = false;
                    response.Message = "Mission not found.";
                    return response;
                }

                // step4. Check mission state
                if (mission.MissionState != EMissionState.Rewardable)
                {
                    response.Success = false;
                    response.Message = "Mission not rewardable.";
                    return response;
                }

                // step5. Calculate reward points
                int point = DataManager.MissionDataDic[mission.TemplateId].Point;

                // step6. Add points to daily mission
                var dayMission = player.Missions.FirstOrDefault(m =>
                    DataManager.MissionDataDic[m.TemplateId].MissionType == Define.EMissionType.Day);

                if (dayMission != null)
                {
                    dayMission.StackedPoint += point;
                    int maxDay = DataManager.MissionDataDic[dayMission.TemplateId].MaxPoint;
                    if (dayMission.StackedPoint > maxDay)
                        dayMission.StackedPoint = maxDay;
                }

                // step7. Add points to weekly mission
                var weekMission = player.Missions.FirstOrDefault(m =>
                    DataManager.MissionDataDic[m.TemplateId].MissionType == Define.EMissionType.Week);

                if (weekMission != null)
                {
                    weekMission.StackedPoint += point;
                    int maxWeek = DataManager.MissionDataDic[weekMission.TemplateId].MaxPoint;
                    if (weekMission.StackedPoint > maxWeek)
                        weekMission.StackedPoint = maxWeek;
                }

                // step8. Update mission state to Finished
                mission.MissionState = EMissionState.Finish;

                // step9. Save changes
                await _dbContext.SaveChangesAsync();

                // step10. Commit transaction
                await transaction.CommitAsync();

                // step11. Reuse MissionListGetAsync to return updated mission list
                return await MissionListGetAsync(new GetMissionListReq { Jwt = request.Jwt });
            }
            catch (Exception ex)
            {
                // stepX. Rollback if error
                await transaction.RollbackAsync();

                response.Success = false;
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }

        public async Task<GetMissionRewardRes> GetMissionReward(GetMissionRewardReq request)
        {
            var response = new GetMissionRewardRes();

            // step0. 트랜잭션 시작
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // step1. JWT에서 accountDbId 추출
                var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);

                if (player == null)
                {
                    response.Success = false;
                    response.Message = "Invalid player.";
                    return response;
                }

                // step2. 미션 찾기
                var mission = player.Missions
                    .FirstOrDefault(m => m.TemplateId == request.TemplatedId);

                if (mission == null)
                {
                    response.Success = false;
                    response.Message = "Mission not found.";
                    return response;
                }

                var missionData = DataManager.MissionDataDic[mission.TemplateId];

                // step3. 지급 가능한 보상 확인
                var rewards = new List<RewardDTO>();
                while (mission.GetRewardCount < missionData.RewardCurrencies.Count &&
                       mission.StackedPoint >= missionData.RewardCurrencies[mission.GetRewardCount].point)
                {
                    var rewardInfo = missionData.RewardCurrencies[mission.GetRewardCount];

                    rewards.Add(new RewardDTO
                    {
                        RewardType = rewardInfo.currencyType,
                        RewardAmount = rewardInfo.count,
                        IsFirst = false,
                    });

                    // 다음 보상 구간으로 증가
                    mission.GetRewardCount++;
                }

                if (rewards.Count == 0)
                {
                    response.Success = false;
                    response.Message = "No rewards available.";
                    return response;
                }

                // step4. 보상 지급 (CurrencyService 활용)
                var currencyService = _serviceProvider.GetRequiredService<CurrencyService>();

                foreach (var reward in rewards)
                {
                    await currencyService.UpdatePlayerCurrencyAsync(new CurrencyAddReq { jwt = request.Jwt, CurrencyType = (CurrencyType)((int)reward.RewardType - 1), Amount = reward.RewardAmount}, false);
                }

                // step5. DB 저장
                await _dbContext.SaveChangesAsync();

                // step6. 트랜잭션 커밋
                await transaction.CommitAsync();

                response.Success = true;
                response.Rewards = rewards;
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                response.Success = false;
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }

        #region Helper
        private static readonly Dictionary<Define.EMissionGoal, HashSet<Define.EBroadcastEventType>> MissionGoalEventMap =
            new()
            {
                { Define.EMissionGoal.MonsterKill, new() { Define.EBroadcastEventType.KillMonster } },
                { Define.EMissionGoal.ConsumGold, new() { Define.EBroadcastEventType.UseGold, Define.EBroadcastEventType.ChangeGold } },
                { Define.EMissionGoal.StageClear, new() { Define.EBroadcastEventType.StageClear } },
                { Define.EMissionGoal.CurrencyGacha, new() { Define.EBroadcastEventType.DoCurrencyGacha } },
                { Define.EMissionGoal.HeroGacha, new() { Define.EBroadcastEventType.DoHeroGacha } },
                { Define.EMissionGoal.BuddyGacha, new() { Define.EBroadcastEventType.DoBuddyGacha } },
                { Define.EMissionGoal.BuddySkillUp, new() { Define.EBroadcastEventType.BuddySkillUp } },
                { Define.EMissionGoal.BuddyLevelUp, new() { Define.EBroadcastEventType.BuddyLevelUp } },
                { Define.EMissionGoal.HeroSkillUp, new() { Define.EBroadcastEventType.HeroSkillUp } },
                { Define.EMissionGoal.HeroLevelUp, new() { Define.EBroadcastEventType.HeroLevelUp } },
            };

        #endregion

        public async Task<bool> AchievementCreateAsync(string jwt, int templateId)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
            if (player == null) return false;

            if (player.Achievements.Any(m => m.TemplateId == templateId)) return false;
            if (!DataManager.AchievementDataDic.TryGetValue(templateId, out var missionData)) return false;

            var achievement = new AchievementSaveDataDb
            {
                TemplateId = templateId,
                StackedPoint = 0,
                MissionState = EMissionState.Progress,
                IsCleared = false,
                PlayerDbId = player.PlayerDbId,
            };

            player.Achievements.Add(achievement);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AchievementClearCreateAsync(string jwt, int templateId)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
            if (player == null) return false;

            if (player.AchievementClearList.Any(m => m.TemplateId == templateId)) return false;

            var achievementClear = new AchievementClearListDb
            {
                TemplateId = templateId,
                PlayerDbId = player.PlayerDbId,
            };

            player.AchievementClearList.Add(achievementClear);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<AchievementListRes> AchievementListGetAsync(AchievementListReq request)
        {
            var response = new AchievementListRes();
            try
            {
                // 1. accountDbId 추출
                var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);

                // 2. Player 가져오기
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
                if (player == null)
                {
                    response.Success = false;
                    response.Message = "Invalid player.";
                    return response;
                }

                // 3. 업적 매핑
                var achievementList = player.Achievements
                    .Select(a => new AchievementDTO
                    {
                        TemplateId = a.TemplateId,
                        StackedPoint = a.StackedPoint,
                        MissionState = a.MissionState,
                        IsCleared = a.IsCleared
                    })
                    .ToList();

                // 4. 응답 구성
                response.Success = true;
                response.Achievements = achievementList;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }
    }
}
