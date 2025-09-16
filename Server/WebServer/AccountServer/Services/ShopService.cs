using GameDB;
using Newtonsoft.Json;
using Server.Data;
using static AccountServer.Define;

namespace AccountServer.Services
{
    public class ShopService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;
        PlayerService _player;
        CurrencyService _currency;

        public ShopService(GameDbContext context, JwtTokenService jwt, PlayerService player, CurrencyService currency)
        {
            _dbContext = context;
            _jwt = jwt;
            _player = player;
            _currency = currency;
        }

        public async Task<ShopHeroGachaRes> HeroGachaDoAsync(ShopHeroGachaReq request)
        {
            var response = new ShopHeroGachaRes();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Validate JWT & load player
                var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
                var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
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
                    var singleReward = rewards.Skip(i).Take(1).ToList();

                    var log = new HeroGachaLogDb
                    {
                        PlayerDbId = player.PlayerDbId,
                        Do = i + 1,             // 1 to Count
                        DoMax = request.Count,  // 1 or 10
                        GachaItemResult = JsonConvert.SerializeObject(singleReward),
                        UnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };

                    _dbContext.HeroGachaLog.Add(log);
                }

                // Step 6: Save changes & commit
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Step 7: Build response
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
    }
}
