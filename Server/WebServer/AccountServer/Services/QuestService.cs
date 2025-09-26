using AccountServer.Data;
using GameDB;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace AccountServer.Services
{
    public class QuestService
    {
        private readonly GameDbContext _dbContext;
        private readonly JwtTokenService _jwt;
        private readonly PlayerService _player;

        public QuestService(GameDbContext context, JwtTokenService jwt, PlayerService player)
        {
            _dbContext = context;
            _jwt = jwt;
            _player = player;
        }

        public async Task<bool> MissionCreateAsync(string jwt, int templateId)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            // Pass _dbContext to ensure same context
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

        public async Task MissionEventAsncHandle(string jwt, Define.EBroadcastEventType eventType, int value, bool commitChanges = true)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            // Pass _dbContext so the loaded player is tracked by the same context
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

        public async Task<GetMissionListRes> MissionListGetAsync(GetMissionListReq request)
        {
            var response = new GetMissionListRes();
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            // Pass _dbContext so the loaded player is tracked
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
    }
}
