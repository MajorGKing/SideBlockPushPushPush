using AccountServer.Data;
using GameDB;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using DbCurrencyType = GameDB.CurrencyType;
using CurrencyType = AccountServer.Data.CurrencyType;
using Server.Quest;

namespace AccountServer.Services
{
    public class StageService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;
        PlayerService _player;
        CurrencyService _currency;


        public StageService(GameDbContext context, JwtTokenService jwt, PlayerService player, CurrencyService currency)
        {
            _dbContext = context;
            _jwt = jwt;
            _player = player;
            _currency = currency;
        }

        public async Task<bool> StageCreateAsync(string jwt, int templateId, bool commitChanges = true)
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
            if(commitChanges == true)
            {
                await _dbContext.SaveChangesAsync();
            }
            

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

            var buddiesDb = (await buddiesTask).OrderBy(b => b.SelectedNumber).ToList();
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

        public async Task<StageRewardRes> StageRewardGetAsync(StageRewardReq request)
        {
            var response = new StageRewardRes();

            // Step 1: Validate player
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
            if (player == null)
            {
                response.Success = false;
                response.Message = "Invalid player.";
                return response;
            }

            // Step 2: Load stage data
            if (!DataManager.StageDataDic.TryGetValue(player.CurrentStage, out var stageData))
            {
                response.Success = false;
                response.Message = $"Stage {player.CurrentStage} not found.";
                return response;
            }

            // Step 3: Start transaction
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // EventCall
                var totalKillMonsters = stageData.FirstWaveMonsterList.Count + stageData.SecondWaveMonsterList.Count + stageData.BossWaveMonsterList.Count;
                EventManager.BroadcastMissionEvent(request.Jwt, Define.EBroadcastEventType.KillMonster, totalKillMonsters, false);

                int enumCount = Enum.GetNames(typeof(Define.ECurrencyType)).Length;
                List<int> currencyCounts = new(new int[enumCount]);
                Random random = new();

                // Normal rewards
                for (int i = 0; i < stageData.RewardTimes; i++)
                {
                    int totalWeight = stageData.RewardPercent.Sum();
                    int rand = random.Next(0, totalWeight);
                    int cumulative = 0;

                    for (int j = 0; j < stageData.RewardPercent.Count; j++)
                    {
                        cumulative += stageData.RewardPercent[j];
                        if (rand < cumulative)
                        {
                            Define.ECurrencyType currencyType = stageData.RewardType[j];
                            int rewardCount = stageData.RewardCount[j];
                            currencyCounts[(int)currencyType] += rewardCount;
                            break;
                        }
                    }
                }

                // Add rolled rewards
                for (int i = 0; i < currencyCounts.Count; i++)
                {
                    if (currencyCounts[i] == 0) continue;

                    response.Rewards.Add(new RewardDTO
                    {
                        RewardType = (Define.ECurrencyType)i,
                        RewardAmount = currencyCounts[i],
                        IsFirst = false
                    });

                    await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq
                    {
                        jwt = request.Jwt,
                        CurrencyType = (CurrencyType)(i - 1), // CurrencyType not has none
                        Amount = currencyCounts[i],
                    }, false);
                }

                // First-clear rewards
                var playerCurrentStageDb = player.Stages.FirstOrDefault(s => s.TemplateId == player.CurrentStage);

                // First-Clear
                if (playerCurrentStageDb.isClear == false)
                {
                    for (int i = 0; i < stageData.RewardFirstType.Count; i++)
                    {
                        response.Rewards.Add(new RewardDTO
                        {
                            RewardType = stageData.RewardFirstType[i],
                            RewardAmount = stageData.RewardFirstCount[i],
                            IsFirst = true
                        });

                        await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq
                        {
                            jwt = request.Jwt,
                            CurrencyType = (CurrencyType)((int)stageData.RewardFirstType[i] - 1), // CurrencyType not has none
                            Amount = stageData.RewardFirstCount[i],
                        }, false);
                    }

                    // Game has next stage
                    if (stageData.NextStageId != 0)
                    {
                        await StageCreateAsync(request.Jwt, stageData.NextStageId);
                        player.CurrentStage = stageData.NextStageId;
                    }

                    playerCurrentStageDb.isClear = true;
                }

                // Save and commit
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Success = true;
                response.Message = "Rewards granted.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = $"Error granting rewards: {ex.Message}";
            }

            return response;
        }

        private async Task<TResponse> ChangeStageAsync<TResponse>(
        string jwt,
        Func<StageData, int> targetStageSelector,
        Func<StageData, string> noStageMessage,
        string lockedFallbackMessage = "This stage is locked.")
        where TResponse : new()
        {
            var response = new TResponse();

            // Step 1: Validate player
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
            if (player == null)
                return SetResponse(response, false, false, 0, "Invalid player.");

            // Step 2: Load current stage
            if (!DataManager.StageDataDic.TryGetValue(player.CurrentStage, out var stageData))
                return SetResponse(response, false, false, 0, $"Stage {player.CurrentStage} not found.");

            // Step 3: Select target stage id
            int targetStageId = targetStageSelector(stageData);
            if (!DataManager.StageDataDic.TryGetValue(targetStageId, out var targetStageData))
                return SetResponse(response, true, false, player.CurrentStage, noStageMessage(stageData));

            // Step 4: Check if player can move
            var targetStageDb = player.Stages.FirstOrDefault(s => s.TemplateId == targetStageData.TemplateId);
            if (targetStageDb == null || targetStageDb.isEnable == false)
            {
                string message = lockedFallbackMessage;
                if (DataManager.StageDataDic.TryGetValue(targetStageData.PreviewStageId, out var prevStage))
                    message = $"Need to Clear {prevStage.DifficultyLevel} {prevStage.WorldNumber}-{prevStage.StageNumber}";

                return SetResponse(response, true, false, player.CurrentStage, message);
            }

            // Step 5: Update current stage
            player.CurrentStage = targetStageData.TemplateId;
            await _dbContext.SaveChangesAsync();

            return SetResponse(response, true, true, targetStageData.TemplateId, "Stage changed.");
        }

        private TResponse SetResponse<TResponse>(TResponse response, bool success, bool canChange, int stageTemplateId, string message)
        {
            dynamic r = response!;
            r.Success = success;
            r.CanChange = canChange;
            r.StageTemplateId = stageTemplateId;
            r.Message = message;
            return response;
        }

        // -------------------------------
        // Public wrappers
        // -------------------------------

        public Task<SetNextStageRes> SetNextStageAsync(SetNextStageReq request)
        {
            return ChangeStageAsync<SetNextStageRes>(
                request.Jwt,
                stage => stage.NextStageId,
                _ => "Wait Update. There is no next stage"
            );
        }

        public Task<SetBackStageRes> SetBackStageAsync(SetBackStageReq request)
        {
            return ChangeStageAsync<SetBackStageRes>(
                request.Jwt,
                stage => stage.PreviewStageId,
                _ => "There is no previous stage."
            );
        }

        public Task<SetHardNormalStageRes> SetHardNormalStageAsync(SetHardNormalStageReq request)
        {
            return ChangeStageAsync<SetHardNormalStageRes>(
                request.Jwt,
                stage => stage.OtherStageId,
                _ => "No other difficulty available for this stage."
            );
        }
    }
}
