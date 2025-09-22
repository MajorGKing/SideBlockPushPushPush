using AccountServer.Data;
using GameDB;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace AccountServer.Services
{
    public class StageService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;
        PlayerService _player;


        public StageService(GameDbContext context, JwtTokenService jwt, PlayerService player)
        {
            _dbContext = context;
            _jwt = jwt;
            _player = player;
        }

        public async Task<bool> StageCreate(string jwt, int templateId)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
            {
                throw new InvalidOperationException($"Player {accountDbId} not found.");
            }

            if (player.Stages.Any(s => s.TemplateId == templateId))
            {
                throw new InvalidOperationException($"Player {accountDbId} already owns stage {templateId}.");
            }

            if (!DataManager.StageDataDic.TryGetValue(templateId, out var stageData))
            {
                throw new ArgumentException($"Invalid stage templateId: {templateId}");
            }

            var stage = new StageClearDb
            {
                TemplateId = templateId,
                PlayerDbId = accountDbId,
                isEnable = true,
                isClear = false,
            };

            player.Stages.Add(stage);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<StageClearListRes> StageListGetAsync(StageClearListReq request)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
            {
                return new StageClearListRes
                {
                    Success = false,
                    Message = $"Player {accountDbId} not found."
                };
            }

            var stages = player.Stages.Select(s => new StageClearDTO
            {
                TemplateId = s.TemplateId,
                IsEnable = s.isEnable,
                IsClear = s.isClear,
            }).ToList();

            return new StageClearListRes
            {
                Success = true,
                Stages = stages,
            };
        }

        public async Task<StageStartDataRes> StageDataGetAsync(StageStartDataReq request)
        {
            var response = new StageStartDataRes();

            // Step 1: Validate player
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
            if (player == null)
            {
                response.Success = false;
                response.Message = "Invalid player.";
                return response;
            }

            // Step 2: Load hero from DB
            var heroDb = await _dbContext.Heroes
                .FirstOrDefaultAsync(h => h.PlayerDbId == player.PlayerDbId && h.IsSelected);

            if (heroDb == null)
            {
                response.Success = false;
                response.Message = "No hero selected.";
                return response;
            }

            var heroData = DataManager.HeroDataDic[heroDb.TemplateId];
            var heroSkillIds = heroDb.SkillTemplateId; // already parsed from SkillTemplateIdString

            var heroSnapshot = new HeroSnapshot
            {
                TemplateId = heroDb.TemplateId,
                Level = heroData.Level, 
                Attack = heroData.Attack,
                MagicAttack = heroData.MagicAttack,
                Skills = heroSkillIds.Select(skillId =>
                {
                    var skillData = DataManager.HeroSkillDataDic[skillId];
                    var effectData = DataManager.EffectDataDic[skillData.EffectDataId];

                    return new SkillSnapshot
                    {
                        TemplateId = skillId,
                        SkillLevel = skillData.SkillLevel,
                        SkillType = skillData.SkillType,
                        AnimSpeed = skillData.AnimSpeed,
                        UseSkillTargetType = skillData.UseSkillTargetType,
                        GatherTargetCounts = skillData.GatherTargetCounts,
                        GatherTargetType = skillData.GatherTargetType,
                        TargetFriendType = skillData.TargetFriendType,
                        IconImageKeys = skillData.IconImageKeys, // hero uses multiple icons
                        Effect = new EffectSnapshot
                        {
                            TemplateId = effectData.TemplateId,
                            EffectType = effectData.EffectType,
                            DurationPolicy = effectData.DurationPolicy,
                            Duration = effectData.Duration,
                            DamageValue = effectData.DamageValue,
                            StatType = effectData.StatType,
                            AddValue = effectData.AddValue,
                            LifeStealValue = effectData.LifeStealValue,
                            StunValue = effectData.StunValue
                        }
                    };
                }).ToList()
            };

            // Step 3 & 4: Load buddies and stage data in parallel
            var buddiesTask = _dbContext.Buddies
                .Where(b => b.PlayerDbId == player.PlayerDbId && b.SelectedNumber >= 0 && b.SelectedNumber < 4)
                .ToListAsync();

            var stageDataTask = Task.FromResult(DataManager.StageDataDic[player.CurrentStage]);

            await Task.WhenAll(buddiesTask, stageDataTask);

            var buddiesDb = buddiesTask.Result;
            var stageData = stageDataTask.Result;

            // Build buddy snapshots
            var buddySnapshots = buddiesDb.Select(buddyDb =>
            {
                var buddyData = DataManager.BuddyDataDic[buddyDb.TemplateId];
                var buddySkillIds = buddyDb.SkillTemplateId;

                return new BuddySnapshot
                {
                    TemplateId = buddyDb.TemplateId,
                    Level = buddyData.Level,
                    Attack = buddyData.Attack,
                    MagicAttack = buddyData.MagicAttack,
                    Reload = buddyData.Reload,
                    Skills = buddySkillIds.Select(skillId =>
                    {
                        var skillData = DataManager.BuddySkillDataDic[skillId];
                        var effectData = DataManager.EffectDataDic[skillData.EffectDataId];

                        return new SkillSnapshot
                        {
                            TemplateId = skillId,
                            SkillLevel = skillData.SkillLevel,
                            SkillType = skillData.SkillType,
                            Cooltime = skillData.Cooltime,
                            AnimSpeed = skillData.AnimSpeed,
                            UseSkillTargetType = skillData.UseSkillTargetType,
                            GatherTargetCounts = skillData.GatherTargetCounts,
                            GatherTargetType = skillData.GatherTargetType,
                            TargetFriendType = skillData.TargetFriendType,
                            IconImageKey = skillData.IconImageKey,
                            Effect = new EffectSnapshot
                            {
                                TemplateId = effectData.TemplateId,
                                EffectType = effectData.EffectType,
                                DurationPolicy = effectData.DurationPolicy,
                                Duration = effectData.Duration,
                                DamageValue = effectData.DamageValue,
                                StatType = effectData.StatType,
                                AddValue = effectData.AddValue,
                                LifeStealValue = effectData.LifeStealValue,
                                StunValue = effectData.StunValue
                            }
                        };
                    }).ToList()
                };
            }).ToList();

            // Build monster waves
            var firstWave = BuildWave(stageData.FirstWaveMonsterList, stageData.FirstWaveMonsterLevelList);
            var secondWave = BuildWave(stageData.SecondWaveMonsterList, stageData.SecondWaveMonsterLevelList);
            var bossWave = BuildWave(stageData.BossWaveMonsterList, stageData.BossWaveMonsterLevelList);

            // Step 5: Build response
            response.Success = true;
            response.Message = "Stage data loaded.";
            response.Hero = heroSnapshot;
            response.Buddies = buddySnapshots;
            response.FirstWave = firstWave;
            response.SecondWave = secondWave;
            response.BossWave = bossWave;

            return response;
        }

        private List<MonsterSnapshot> BuildWave(List<int> ids, List<int> levels)
        {
            var list = new List<MonsterSnapshot>();
            for (int i = 0; i < ids.Count; i++)
            {
                var monsterData = DataManager.MonsterDataDic[ids[i]];
                var progressionData = DataManager.ProgressionTypeDataDic[monsterData.ProgressionTypeId];
                var level = levels[i];

                list.Add(new MonsterSnapshot
                {
                    TemplateId = monsterData.TemplateId,
                    Level = level,
                    MaxHp = monsterData.MaxHp + ((level - 1) * progressionData.MaxHp),
                    NormalDefence = monsterData.NormalDefence + ((level - 1) * progressionData.NormalDefence),
                    MagicDefence = monsterData.MagicDefence + ((level - 1) * progressionData.MagicDefence),
                });
            }
            return list;
        }
    }
}
