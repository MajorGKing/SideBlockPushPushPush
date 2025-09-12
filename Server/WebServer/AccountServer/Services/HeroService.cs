using GameDB;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using static AccountServer.Define;

namespace AccountServer.Services
{
    public class HeroService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;
        CurrencyService _currency;

        public HeroService(GameDbContext context, JwtTokenService jwt, CurrencyService currency)
        {
            _dbContext = context;
            _jwt = jwt;
            _currency = currency;
        }

        private async Task<PlayerDb> GetPlayerDbFromAccountDbId(int accountDbId)
        {
            // Player + Heroes 로드
            var player = await _dbContext.Players
                .Include(p => p.Heroes)
                .FirstOrDefaultAsync(p => p.PlayerDbId == accountDbId);

            //if (player == null)
            //{
            //    throw new InvalidOperationException($"Player {accountDbId} not found.");
            //}

            return player;
        }


        public async Task<bool> CreateHero(string jwt, int templateId, bool isSelected = false)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);

            //var token = _jwt.DecipherJwtAccessToken(jwt);
            //var subClaim = token.Claims.FirstOrDefault(c => c.Type == "sub");

            //if (subClaim == null)
            //{
            //    throw new UnauthorizedAccessException("JWT 토큰에 'sub' 클레임이 존재하지 않습니다.");
            //}

            //if (!int.TryParse(subClaim.Value, out int accountDbId))
            //{
            //    throw new FormatException("'sub' 클레임 값이 정수로 변환되지 않았습니다.");
            //}


            // Player 존재 여부 확인
            var player = await GetPlayerDbFromAccountDbId(accountDbId);

            //var player = await _dbContext.Players
            //    .Include(p => p.Heroes)
            //    .FirstOrDefaultAsync(p => p.PlayerDbId == accountDbId);

            if (player == null)
            {
                throw new InvalidOperationException($"Player {accountDbId} not found.");
            }

            if (player.Heroes.Any(h => h.TemplateId == templateId))
            {
                throw new InvalidOperationException($"Player {accountDbId} already owns hero {templateId}.");
            }

            if (!DataManager.HeroDataDic.TryGetValue(templateId, out var heroData))
            {
                throw new ArgumentException($"Invalid hero templateId: {templateId}");
            }

            var hero = new HeroSaveDataDb
            {
                TemplateId = templateId,
                SkillTemplateId = DataManager.HeroDataDic[templateId].SKillIds,
                IsSelected = isSelected,
                NowExp = 0,
                MaxExp = DataManager.HeroDataDic[templateId].LevelUpCurrency1Count,
                PlayerDbId = accountDbId,
            };

            player.Heroes.Add(hero);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<HeroListRes> GetHeroListAsync(HeroListReq request)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await GetPlayerDbFromAccountDbId(accountDbId);
            
            if (player == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Player {accountDbId} not found."
                };
            }

            var heroes = player.Heroes.Select(h => new HeroDTO
            {
                HeroSaveDataDbId = h.HeroSaveDataDbId,
                TemplateId = h.TemplateId,
                SkillTemplateIds = h.SkillTemplateId, // NotMapped 속성 덕분에 List<int>로 변환됨
                IsSelected = h.IsSelected,
                NowExp = h.NowExp,
                MaxExp = h.MaxExp
            }).ToList();

            return new HeroListRes
            {
                Success = true,
                Heroes = heroes
            };
        }

        public async Task<HeroListRes> ChangeSelectedHeroAsync(HeroNowChangeReq request)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Player {accountDbId} not found."
                };
            }

            bool found = false;
            foreach (var hero in player.Heroes)
            {
                if (hero.TemplateId == request.TemplateId)
                {
                    hero.IsSelected = true;
                    found = true;
                    Console.WriteLine($"Hero Selected : {hero.TemplateId}");
                }
                else
                {
                    hero.IsSelected = false;
                }
            }

            if (!found)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Hero with TemplateId {request.TemplateId} not found."
                };
            }

            await _dbContext.SaveChangesAsync();

            // 여기서 새로 DTO로 변환하지 않고, 기존 메서드 재사용
            return await GetHeroListAsync(new HeroListReq { Jwt = request.Jwt });
        }

        public async Task<HeroListRes> LevelUpHeroAsync(HeroLevelUpReq request)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Player {accountDbId} not found."
                };
            }

            // 1. Find Hero Data
            var hero = player.Heroes.FirstOrDefault(h => h.TemplateId == request.TemplateId);
            if (hero == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = "No hero is currently selected."
                };
            }

            if (!DataManager.HeroDataDic.TryGetValue(request.TemplateId, out var heroData))
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Hero template {request.TemplateId} not found in DataManager."
                };
            }

            // 2. Check if next level exists
            if (heroData.NextLevelId == 0)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = "This hero cannot level up further."
                };
            }

            // 3. Check currency availability
            foreach (var currency in heroData.LevelUpCurrencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                if (currency.count > await _currency.GetCurrency(accountDbId, currency.currencyType).ConfigureAwait(false))
                {
                    return new HeroListRes
                    {
                        Success = false,
                        Message = "Not enough currency for level up."
                    };
                }
            }

            // 4. Deduct currency
            foreach (var currency in heroData.LevelUpCurrencies)
            {
                // Define.ECurrencyType은 CurrencyType보다 1 큰 인덱스라고 가정
                CurrencyType type = (CurrencyType)((int)currency.currencyType - 1);

                await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq { jwt = request.Jwt, CurrencyType = type, Amount = -currency.count });
            }

            // 5. Level up hero
            hero.TemplateId = heroData.NextLevelId;

            if (!DataManager.HeroDataDic.TryGetValue(hero.TemplateId, out var nextHeroData))
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Next hero template {hero.TemplateId} not found."
                };
            }

            // Update MaxExp for new level
            hero.MaxExp = nextHeroData.LevelUpCurrency1Count;
            hero.NowExp = 0; // reset exp

            // Sync skills
            var orgSkillIds = hero.SkillTemplateId
                .Select(id => DataManager.HeroSkillDataDic[id].OriginalLevelId)
                .ToList();

            // becuse SkillTemplateId is [NotMapped]
            var skills = hero.SkillTemplateId.ToList();

            foreach (var skillId in nextHeroData.SKillIds)
            {
                var originalId = DataManager.HeroSkillDataDic[skillId].OriginalLevelId;
                if (!orgSkillIds.Contains(originalId))
                {
                    skills.Add(skillId);
                }
            }

            hero.SkillTemplateId = skills;

            // 6. Save changes
            await _dbContext.SaveChangesAsync();

            // 7. Return updated hero list
            return await GetHeroListAsync(new HeroListReq { Jwt = request.Jwt });
        }

        public async Task<HeroListRes> HeroSkillUpAsync(HeroSkillLevelUpReq request)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Player {accountDbId} not found."
                };
            }

            // 1. Find the hero
            var hero = player.Heroes.FirstOrDefault(h => h.TemplateId == request.HeroTemplateId);
            if (hero == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Hero {request.HeroTemplateId} not found."
                };
            }

            // 2. Validate skill exists
            if (!DataManager.HeroSkillDataDic.TryGetValue(request.HeroSkillTemplateId, out var skillData))
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Skill {request.HeroSkillTemplateId} not found in DataManager."
                };
            }

            // 3. Check if skill can level up
            if (skillData.NextLevelId == 0)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = "This skill cannot level up further."
                };
            }

            var skillList = hero.SkillTemplateId.ToList();
            var skillIndex = skillList.IndexOf(request.HeroSkillTemplateId);

            if (skillIndex == -1)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Hero does not have skill {request.HeroSkillTemplateId}."
                };
            }

            // 4. Check currency
            foreach (var currency in skillData.LevelUpCurrencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                var balance = await _currency.GetCurrency(accountDbId, currency.currencyType);
                if (currency.count > balance)
                {
                    return new HeroListRes
                    {
                        Success = false,
                        Message = "Not enough currency for skill upgrade."
                    };
                }
            }

            // 5. Deduct currency
            foreach (var currency in skillData.LevelUpCurrencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                await _currency.UpdatePlayerCurrencyAsync(new CurrencyAddReq
                {
                    jwt = request.Jwt,
                    CurrencyType = (CurrencyType)((int)currency.currencyType - 1),
                    Amount = -currency.count
                });
            }

            // 6. Upgrade skill
            skillList[skillIndex] = skillData.NextLevelId;
            hero.SkillTemplateId = skillList;

            // 7. Save changes
            await _dbContext.SaveChangesAsync();

            // 8. Return updated hero list
            return await GetHeroListAsync(new HeroListReq { Jwt = request.Jwt });
        }
    }
}
