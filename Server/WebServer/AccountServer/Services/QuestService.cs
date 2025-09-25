using AccountServer.Data;
using GameDB;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace AccountServer.Services
{
    public class QuestService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;
        PlayerService _player;

        public QuestService(GameDbContext context, JwtTokenService jwt, PlayerService player)
        {
            _dbContext = context;
            _jwt = jwt;
            _player = player;
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
                MissionState = EMissionState.Progress,
                PlayerDbId = player.PlayerDbId,
            };

            player.Missions.Add(mission);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async void OnHandleBroadcastMissionEvent(string jwt, Define.EBroadcastEventType eventType, int value, bool commitChanges = true)
        {
            // 1. Get player
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
            if (player == null) return;

            bool saveNeeded = false;

            // 2. Process each mission
            foreach (var mission in player.Missions)
            {
                if (mission.MissionState != EMissionState.Progress)
                    continue;

                // Get mission template data
                if (!DataManager.MissionDataDic.TryGetValue(mission.TemplateId, out var missionData))
                    continue;

                // Check if this event is relevant to the mission goal
                if (MissionGoalEventMap.TryGetValue(missionData.MissionGoal, out var allowedEvents) && allowedEvents.Contains(eventType))
                {
                    mission.StackedPoint += value;

                    // Clamp to max
                    if (mission.StackedPoint >= missionData.MissionCount)
                    {
                        mission.StackedPoint = missionData.MissionCount;
                        mission.MissionState = EMissionState.Rewardable;
                    }

                    saveNeeded = true;
                }
            }

            if (commitChanges == true && saveNeeded == true)
                await _dbContext.SaveChangesAsync();
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
                MissionState = m.MissionState
            }).ToList();

            response.Success = true;
            response.Message = "Mission list retrieved.";
            return response;
        }

        // Map mission goals → supported event types
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
