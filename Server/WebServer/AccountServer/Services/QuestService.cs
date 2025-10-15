using AccountServer.Data;
using GameDB;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using DbCurrencyType = GameDB.CurrencyType;
using CurrencyType = AccountServer.Data.CurrencyType;
using DbMissionState = GameDB.EMissionState;


namespace AccountServer.Services
{
    public class QuestService
    {
        private readonly GameDbContext _dbContext;
        private readonly JwtTokenService _jwt;
        private readonly PlayerService _player;
        IServiceProvider _serviceProvider;
        private readonly ILogger<QuestService> _logger;

        public QuestService(GameDbContext context, JwtTokenService jwt, PlayerService player, IServiceProvider serviceProvider, ILogger<QuestService> logger)
        {
            _dbContext = context;
            _jwt = jwt;
            _player = player;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<bool> MissionCreateAsync(string jwt, int templateId)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Missions);
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
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Missions, true);

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
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Missions);
            if (player == null) return;

            bool saveNeeded = false;

            foreach (var mission in player.Missions)
            {
                if (mission.MissionState != DbMissionState.Progress)
                    continue;

                if (!DataManager.MissionDataDic.TryGetValue(mission.TemplateId, out var missionData))
                    continue;

                if (MissionGoalEventMap.TryGetValue(missionData.MissionGoal, out var allowedEvents) && allowedEvents.Contains(eventType))
                {
                    mission.StackedPoint += value;

                    if (mission.StackedPoint >= missionData.MissionCount)
                    {
                        mission.StackedPoint = missionData.MissionCount;
                        mission.MissionState = DbMissionState.Rewardable;
                    }

                    saveNeeded = true;

                    _dbContext.Entry(mission).State = EntityState.Modified;
                }
            }

            if (await AchievementEventAsyncHandle(jwt, eventType, value, commitChanges))
                saveNeeded = true;

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
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Missions);

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
                if (mission.MissionState != DbMissionState.Rewardable)
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
                mission.MissionState = DbMissionState.Finish;

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
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Missions);

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

        private int GetValueByMissionGoal(AchievementValueDb v, Define.EMissionGoal goal)
        {
            return goal switch
            {
                Define.EMissionGoal.MonsterKill => v.MonsterKill,
                Define.EMissionGoal.ConsumGold => v.ConsumGold,
                Define.EMissionGoal.StageClear => v.StageClear,
                Define.EMissionGoal.CurrencyGacha => v.CurrencyGacha,
                Define.EMissionGoal.BuddySkillUp => v.BuddySkillUp,
                Define.EMissionGoal.BuddyLevelUp => v.BuddyLevelUp,
                Define.EMissionGoal.HeroSkillUp => v.HeroSkillUp,
                Define.EMissionGoal.HeroLevelUp => v.HeroLevelUp,
                Define.EMissionGoal.HeroGacha => v.HeroGacha,
                Define.EMissionGoal.BuddyGacha => v.BuddyGacha,
                _ => 0,
            };
        }

        private bool CompareEventTypeAndMissionGoal(Define.EBroadcastEventType eventType, Define.EMissionGoal goal)
        {
            return (eventType, goal) switch
            {
                (Define.EBroadcastEventType.ChangeGold, Define.EMissionGoal.ConsumGold) => true,
                (Define.EBroadcastEventType.UseGold, Define.EMissionGoal.ConsumGold) => true,
                (Define.EBroadcastEventType.KillMonster, Define.EMissionGoal.MonsterKill) => true,
                (Define.EBroadcastEventType.StageClear, Define.EMissionGoal.StageClear) => true,
                (Define.EBroadcastEventType.StageClear, Define.EMissionGoal.StageClearAt) => true,
                (Define.EBroadcastEventType.BuddySkillUp, Define.EMissionGoal.BuddySkillUp) => true,
                (Define.EBroadcastEventType.BuddyLevelUp, Define.EMissionGoal.BuddyLevelUp) => true,
                (Define.EBroadcastEventType.HeroSkillUp, Define.EMissionGoal.HeroSkillUp) => true,
                (Define.EBroadcastEventType.HeroLevelUp, Define.EMissionGoal.HeroLevelUp) => true,
                (Define.EBroadcastEventType.DoCurrencyGacha, Define.EMissionGoal.CurrencyGacha) => true,
                (Define.EBroadcastEventType.DoHeroGacha, Define.EMissionGoal.HeroGacha) => true,
                (Define.EBroadcastEventType.DoBuddyGacha, Define.EMissionGoal.BuddyGacha) => true,
                _ => false
            };
        }

        private int GetValueByMissionGoalStageClearAt(List<StageClearDb> stages, int templateId)
        {
            if (stages == null || stages.Count == 0)
                return 0;

            var stage = stages.FirstOrDefault(s => s.TemplateId == templateId);
            if (stage != null && stage.isClear)
                return 1;

            return 0;
        }



        #endregion

        public async Task<bool> AddNewAchievementsAsync(string jwt)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Achievements|PlayerIncludeType.AchievementClearList, true);
            if (player == null) return false;

            // Step 1. Get all root achievements (TemplateId == OriginalAchievementId)
            var rootAchievements = DataManager.AchievementDataDic.Values
                .Where(a => a.TemplateId == a.OriginalAchievementId)
                .ToList();

            // Step 2. Filter out ones the player already has (either in progress or cleared)
            var playerSaveIds = player.Achievements.Select(a => a.TemplateId).ToHashSet();
            var playerClearIds = player.AchievementClearList.Select(c => c.TemplateId).ToHashSet();

            var missing = rootAchievements
                .Where(a => !playerSaveIds.Contains(a.TemplateId) && !playerClearIds.Contains(a.TemplateId))
                .ToList();

            // Step 3. Add missing ones
            if (missing.Count == 0)
                return true;

            foreach (var data in missing)
            {
                await AchievementCreateAsync(jwt, data.TemplateId);    
            }

            return true;
        }

        public async Task<bool> AchievementCreateAsync(string jwt, int templateId, bool commitChanges = true, bool autoInitializeProgress = false)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.AchievementValues);
            if (player == null) return false;

            if (player.Achievements.Any(m => m.TemplateId == templateId)) return false;
            if (!DataManager.AchievementDataDic.TryGetValue(templateId, out var data)) return false;

            var achievement = new AchievementSaveDataDb
            {
                TemplateId = templateId,
                StackedPoint = 0,
                MissionState = DbMissionState.Progress,
                PlayerDbId = player.PlayerDbId,
            };
            player.Achievements.Add(achievement);

            if (autoInitializeProgress && data.AchievementType == Define.EAchievementType.Normal)
            {
                var achievementValue = player.AchievementValues;
                int current = data.MissionGoal == Define.EMissionGoal.StageClearAt
                    ? GetValueByMissionGoalStageClearAt(player.Stages.ToList(), data.MissionCount)
                    : GetValueByMissionGoal(achievementValue, data.MissionGoal);

                achievement.StackedPoint = current;

                if (current >= data.MissionCount)
                    achievement.MissionState = DbMissionState.Rewardable;
            }

            if (commitChanges)
                await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AchievementRemoveAsync(string jwt, int templateId, bool commitChanges = true)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Achievements);
            if (player == null) return false;

            // Find the achievement in progress
            var achievement = player.Achievements.FirstOrDefault(a => a.TemplateId == templateId);
            if (achievement == null) return false;

            // Remove it
            player.Achievements.Remove(achievement);

            // Save changes
            if(commitChanges)
                await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AchievementClearCreateAsync(string jwt, int templateId, bool commitChanges = true)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.AchievementClearList);
            if (player == null) return false;

            if (player.AchievementClearList.Any(m => m.TemplateId == templateId)) return false;

            var achievementClear = new AchievementClearListDb
            {
                TemplateId = templateId,
                PlayerDbId = player.PlayerDbId,
            };

            player.AchievementClearList.Add(achievementClear);

            // Save changes
            if (commitChanges)
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
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Achievements, true);
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

        public async Task<bool> AchievementEventAsyncHandle(string jwt, Define.EBroadcastEventType eventType, int value, bool commitChanges = true)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Achievements|PlayerIncludeType.AchievementValues|PlayerIncludeType.Stages);
            if (player == null) return false;

            // Step 0: Get or create AchievementValue row from included navigation property
            var achievementValue = player.AchievementValues;

            if (achievementValue == null)
            {
                _logger.LogWarning($"AchievementValue missing for player {player.PlayerDbId}");
                return false;
            }

            bool hasProgress = false;


            // Step 1: Update the value based on event type
            switch (eventType)
            {
                case Define.EBroadcastEventType.KillMonster:
                    achievementValue.MonsterKill += value;
                    hasProgress = true;
                    break;
                case Define.EBroadcastEventType.UseGold:
                //case Define.EBroadcastEventType.ChangeGold:
                    achievementValue.ConsumGold += value;
                    hasProgress = true;
                    break;
                case Define.EBroadcastEventType.StageClear:
                    achievementValue.StageClear += value;
                    hasProgress = true;
                    break;
                case Define.EBroadcastEventType.DoCurrencyGacha:
                    achievementValue.CurrencyGacha += value;
                    hasProgress = true;
                    break;
                case Define.EBroadcastEventType.BuddySkillUp:
                    achievementValue.BuddySkillUp += value;
                    hasProgress = true;
                    break;
                case Define.EBroadcastEventType.BuddyLevelUp:
                    achievementValue.BuddyLevelUp += value;
                    hasProgress = true;
                    break;
                case Define.EBroadcastEventType.HeroSkillUp:
                    achievementValue.HeroSkillUp += value;
                    hasProgress = true;
                    break;
                case Define.EBroadcastEventType.HeroLevelUp:
                    achievementValue.HeroLevelUp += value;
                    hasProgress = true;
                    break;
                case Define.EBroadcastEventType.DoHeroGacha:
                    achievementValue.HeroGacha += value;
                    hasProgress = true;
                    break;
                case Define.EBroadcastEventType.DoBuddyGacha:
                    achievementValue.BuddyGacha += value;
                    hasProgress = true;
                    break;
                default:
                    break; // do nothing for None or unhandled events
            }

            // step2 change stage of Achievement
            if (player.Achievements == null)
            {
                _logger.LogWarning($"Player {player.PlayerDbId} has no Achievements loaded.");
                return false;
            }

            foreach (var save in player.Achievements)
            {
                if (!DataManager.AchievementDataDic.TryGetValue(save.TemplateId, out var data))
                    continue;

                // Only update if the event matches the mission goal
                if (CompareEventTypeAndMissionGoal(eventType, data.MissionGoal) == false)
                    continue;

                hasProgress = true;

                if (data.AchievementType == Define.EAchievementType.Normal)
                {
                    int current = data.MissionGoal == Define.EMissionGoal.StageClearAt
                        ? GetValueByMissionGoalStageClearAt(player.Stages.ToList(), data.MissionCount)
                        : GetValueByMissionGoal(achievementValue, data.MissionGoal);

                    save.StackedPoint = current;

                    if (save.MissionState == DbMissionState.Progress && current >= data.MissionCount)
                    {
                        save.MissionState = DbMissionState.Rewardable;
                    }
                }
            }

            // Commit changes if requested
            if (hasProgress && commitChanges)
                await _dbContext.SaveChangesAsync();

            return hasProgress;
        }

        public async Task<GetAchievementRewardRes> GetAchievementReward(GetAchievementRewardReq req)
        {
            var response = new GetAchievementRewardRes();

            // Step 0. Start transaction for safety
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Step 1. Get player info from JWT
                var accountDbId = _jwt.GetAccountDbIdInJwt(req.Jwt);
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Achievements);

                if (player == null)
                {
                    response.Success = false;
                    response.Message = "Invalid player.";
                    return response;
                }

                // Step 2. Find the achievement
                var achievement = player.Achievements
                    .FirstOrDefault(a => a.TemplateId == req.TemplatedId);

                if (achievement == null)
                {
                    response.Success = false;
                    response.Message = "Achievement not found.";
                    return response;
                }

                // Step 3. Check state
                if (achievement.MissionState != DbMissionState.Rewardable)
                {
                    response.Success = false;
                    response.Message = "Achievement is not rewardable.";
                    return response;
                }

                // Step 4. Get reward data from DataManager
                if (!DataManager.AchievementDataDic.TryGetValue(req.TemplatedId, out var achievementData))
                {
                    response.Success = false;
                    response.Message = "Achievement data not found.";
                    return response;
                }

                var rewards = new List<RewardDTO>();

                rewards.Add(new RewardDTO
                {
                    RewardType = achievementData.RewardType,
                    RewardAmount = achievementData.RewardCount,
                    IsFirst = false
                });

                if (rewards.Count == 0)
                {
                    response.Success = false;
                    response.Message = "No rewards configured.";
                    return response;
                }

                // Step 5. Give rewards using CurrencyService
                var currencyService = _serviceProvider.GetRequiredService<CurrencyService>();

                foreach (var reward in rewards)
                {
                    await currencyService.UpdatePlayerCurrencyAsync(
                        new CurrencyAddReq
                        {
                            jwt = req.Jwt,
                            CurrencyType = (CurrencyType)((int)reward.RewardType - 1),
                            Amount = reward.RewardAmount
                        },
                        false);
                }

                // Step 6. Move achievement to clear list
                await AchievementRemoveAsync(req.Jwt, req.TemplatedId, false);
                await AchievementClearCreateAsync(req.Jwt, req.TemplatedId, false);

                // If there is next achievement, add it.
                if (achievementData.NextAchievementId != 0)
                {
                    await AchievementCreateAsync(req.Jwt, achievementData.NextAchievementId, false, true);
                }

                // Step 7. Save and commit
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Success = true;
                response.Rewards = rewards;
                response.Message = "Achievement reward granted.";
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
    }
}
