using AccountServer.Data;
using GameDB;
using Server.Data;
using DbCurrencyType = GameDB.CurrencyType;
using CurrencyType = AccountServer.Data.CurrencyType;


namespace AccountServer.Services
{
    public class ShopService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;
        PlayerService _player;
        CurrencyService _currency;
        QuestService _quest;
        

        public ShopService(GameDbContext context, JwtTokenService jwt, PlayerService player, CurrencyService currency, QuestService quest)
        {
            _dbContext = context;
            _jwt = jwt;
            _player = player;
            _currency = currency;
            _quest = quest;
        }

        public async Task<ShopHeroGachaRes> HeroGachaDoAsync(ShopHeroGachaReq request)
        {
            var response = new ShopHeroGachaRes();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Validate JWT & load player
                var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Currency, true);
                if (player == null)
                {
                    response.Success = false;
                    response.Message = "Invalid player.";
                    return response;
                }

                // Step 2: Validate count
                if (request.Count != 1 && request.Count != 10)
                {
                    response.Success = false;
                    response.Message = "Invalid gacha count.";
                    return response;
                }

                // Step 3: Determine cost
                int needDia = request.Count == 1 ? 110 : 1000;
                if (player.Currency.Dia < needDia)
                {
                    response.Success = false;
                    response.Message = "Not enough diamonds.";
                    return response;
                }

                // Deduct diamonds
                await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq
                {
                    jwt = request.Jwt,
                    CurrencyType = CurrencyType.Dia,
                    Amount = -needDia // 차감
                }, false);

                // Step 4: RNG + rewards
                List<HeroGachaReward> rewards = new List<HeroGachaReward>();
                Random random = new Random();
                int maxRoll = DataManager.HeroGachaDataDic.First().Value.Max;

                for (int i = 0; i < request.Count; i++)
                {
                    int roll = random.Next(maxRoll);

                    foreach (var heroGachaData in DataManager.HeroGachaDataDic.Values)
                    {
                        if (heroGachaData.Percent > roll)
                        {
                            var reward = new HeroGachaReward
                            {
                                Type = (CurrencyType)((int)heroGachaData.CurrencyType -1),
                                Count = heroGachaData.CurrencyCount
                            };
                            rewards.Add(reward);

                            // Update player currency
                            await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq
                            {
                                jwt = request.Jwt,
                                CurrencyType = (CurrencyType)((int)heroGachaData.CurrencyType - 1),
                                Amount = heroGachaData.CurrencyCount // 지급
                            }, false);

                            break;
                        }
                    }
                }

                // Step 5: Save log (1 entry per gacha session)
                for (int i = 0; i < request.Count; i++)
                {
                    //var singleReward = rewards.Skip(i).Take(1).ToList();
                    var singleReward = rewards.Skip(i).Take(1).ToList(); ;

                    var log = new HeroGachaLogDb
                    {
                        PlayerDbId = player.PlayerDbId,
                        Do = i + 1,             // 1 to Count
                        DoMax = request.Count,  // 1 or 10
                        GachaItemResult = (DbCurrencyType)singleReward[0].Type,
                        Count = singleReward[0].Count,
                        UnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };

                    _dbContext.HeroGachaLog.Add(log);
                }

                // Step 6 : EventCall
                await _quest.MissionEventAsncHandle(request.Jwt, Define.EBroadcastEventType.DoHeroGacha, request.Count, false);

                // Step 7: Save changes & commit
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Step 8: Build response
                response.Success = true;
                response.Message = "Gacha completed.";
                response.Rewards = rewards;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        public async Task<ShopBuddyGachaRes> BuddyGachaDoAsync(ShopBuddyGachaReq request)
        {
            var response = new ShopBuddyGachaRes();
            
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Validate JWT & load player
                var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Currency|PlayerIncludeType.Buddies);
                if (player == null)
                {
                    response.Success = false;
                    response.Message = "Invalid player.";
                    return response;
                }

                // Step 2: Validate count
                if (request.Count != 1 && request.Count != 10)
                {
                    response.Success = false;
                    response.Message = "Invalid gacha count.";
                    return response;
                }

                // Step 3: Determine cost
                int needDia = request.Count == 1 ? 110 : 1000;
                if (player.Currency.Dia < needDia)
                {
                    response.Success = false;
                    response.Message = "Not enough diamonds.";
                    return response;
                }

                // Deduct diamonds
                await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq
                {
                    jwt = request.Jwt,
                    CurrencyType = CurrencyType.Dia,
                    Amount = -needDia
                }, false);

                // Step 4: RNG + rewards
                List<BuddyGachaReward> rewards = new List<BuddyGachaReward>();
                Random random = new Random();

                for (int i = 0; i < request.Count; i++)
                {
                    // Determine rarity
                    int roll = random.Next(DataManager.BuddyGachaRarityDataDic.First().Value.Max);
                    Define.ERarityType rarity = Define.ERarityType.None;
                    foreach (var rarityData in DataManager.BuddyGachaRarityDataDic.Values)
                    {
                        if (rarityData.Percent > roll)
                        {
                            rarity = rarityData.RarityType;
                            break;
                        }
                    }

                    // Pick a random buddy by rarity
                    List<string> candidateList = rarity switch
                    {
                        Define.ERarityType.Common => DataManager.commonBuddies,
                        Define.ERarityType.Rare => DataManager.rareBuddies,
                        Define.ERarityType.Epic => DataManager.epicBuddies,
                        Define.ERarityType.Unique => DataManager.uniqueBuddies,
                        Define.ERarityType.Legend => DataManager.legendBuddies,
                        _ => new List<string>()
                    };

                    int randomIndex = random.Next(candidateList.Count);
                    string buddyName = candidateList[randomIndex];
                    int buddyTemplateId = DataManager.BuddyGachaDataDic[candidateList[randomIndex]].BuddyTemplateId;
                    var buddyData = DataManager.BuddyDataDic[buddyTemplateId];
                    var buddyGachaData = DataManager.BuddyGachaDataDic[candidateList[randomIndex]];

                    var gachaReward = new BuddyGachaReward
                    {
                        BuddyName = buddyName,
                    };

                    // Check if player already has this buddy
                    bool hasBuddy = player.Buddies.Any(b => b.TemplateId == buddyTemplateId);

                    if (!hasBuddy)
                    {
                        gachaReward.IsDuplicate = false;

                        // Add buddy to player
                        var newBuddy = new BuddySaveDataDb
                        {
                            TemplateId = buddyTemplateId,
                            SkillTemplateId = buddyData.SKillIds ?? new List<int>(),
                            SelectedNumber = -1,
                            PlayerDbId = accountDbId
                        };
                        player.Buddies.Add(newBuddy);
                    }
                    else
                    {
                        gachaReward.IsDuplicate = true;

                        // Add reward currency
                        await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq
                        {
                            jwt = request.Jwt,
                            CurrencyType = (CurrencyType)((int)buddyGachaData.CurrencyType - 1),
                            Amount = buddyGachaData.CurrencyCount
                        }, false);
                    }

                    rewards.Add(gachaReward);

                    // Step 5: Save log for each gacha
                    var log = new BuddyGachaLogDb
                    {
                        PlayerDbId = player.PlayerDbId,
                        Do = i + 1,
                        DoMax = request.Count,
                        BuddyTemplateId = buddyTemplateId,
                        BuddyGachaName = buddyGachaData.GachaItem,
                        Rarity = (Rarity)((int)buddyGachaData.Rarity - 1),
                        IsDuplicate = gachaReward.IsDuplicate,
                        DuplicateRewardType = gachaReward.IsDuplicate ? ((DbCurrencyType)(int)buddyGachaData.CurrencyType - 1) : null,
                        DuplicateRewardCount = gachaReward.IsDuplicate ? buddyGachaData.CurrencyCount : 0,
                        UnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };
                    _dbContext.BuddyGachaLog.Add(log);
                }

                // Step 6: EventCall
                await _quest.MissionEventAsncHandle(request.Jwt, Define.EBroadcastEventType.DoBuddyGacha, request.Count, false);

                // Step 7: Save changes & commit
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Step 8: Build response
                response.Success = true;
                response.Message = "Buddy gacha completed.";
                response.Rewards = rewards;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        public async Task<ShopCurrencyGachaRes> CurrencyGachaDoAsync(ShopCurrencyGachaReq request)
        {
            var response = new ShopCurrencyGachaRes();

            // Step 1: Validate player (via JWT)
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId, PlayerIncludeType.Currency);
            if (player == null)
            {
                response.Success = false;
                response.Message = "Invalid player.";
                return response;
            }

            // Step 2: Determine cost (based on count)
            int needGold = request.Count switch
            {
                1 => 100,
                10 => 1000,
                100 => 10000,
                _ => 0
            };

            if (needGold == 0)
            {
                response.Success = false;
                response.Message = "Invalid gacha count.";
                return response;
            }

            // Step 3: Check if player has enough gold
            if (player.Currency.Gold < needGold)
            {
                response.Success = false;
                response.Message = "Not enough gold.";
                return response;
            }

            // Step 4: Start DB transaction
            var rewards = new List<CurrencyGachaReward>();
            var random = new Random();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Step 5: Deduct gold
                await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq
                {
                    jwt = request.Jwt,
                    CurrencyType = CurrencyType.Gold,
                    Amount = -needGold
                }, false);

                // Step 6: Perform gacha draws
                int totalMax = DataManager.CurrencyGachaDataDic.Values.Max(x => x.Max);
                for (int i = 0; i < request.Count; i++)
                {
                    int randomNumber = random.Next(totalMax);

                    foreach (var gachaData in DataManager.CurrencyGachaDataDic.Values)
                    {
                        if (gachaData.Percent > randomNumber)
                        {
                            // Reward data
                            var reward = new CurrencyGachaReward
                            {
                                Type = (CurrencyType)((int)gachaData.CurrencyType - 1),
                                Count = gachaData.CurrencyCount,
                            };
                            rewards.Add(reward);

                            // Step 7: Apply reward to player
                            await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq
                            {
                                jwt = request.Jwt,
                                CurrencyType = reward.Type,
                                Amount = reward.Count,
                            }, false);

                            // Step 8: Save log
                            var log = new CurrencyGachaLogDb
                            {
                                PlayerDbId = player.PlayerDbId,
                                Do = i + 1,
                                DoMax = request.Count,
                                GachaItemResult = (DbCurrencyType)reward.Type,
                                Count = reward.Count,
                                UnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                            };
                            _dbContext.CurrencyGachaLog.Add(log);

                            break;
                        }
                    }
                }

                // Step 9: EventCall
                await _quest.MissionEventAsncHandle(request.Jwt, Define.EBroadcastEventType.DoCurrencyGacha, request.Count, false);

                // Step 10: Commit DB changes
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Step 11: Build response
                response.Success = true;
                response.Message = "Currency gacha completed.";
                response.Rewards = rewards;
            }
            catch (Exception ex)
            {
                // Step 11: Rollback if error
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = $"Currency gacha failed: {ex.Message}";
            }

            return response;
        }
    }
}
