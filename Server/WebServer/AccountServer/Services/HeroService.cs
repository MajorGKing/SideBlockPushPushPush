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

            // Step 0: Begin transaction to ensure atomicity
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Step 1: Load player with heroes inside transaction
            var player = await _dbContext.Players
                .Include(p => p.Heroes)
                .FirstOrDefaultAsync(p => p.PlayerDbId == accountDbId);

            if (player == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Player {accountDbId} not found."
                };
            }

            // Step 2: Find the hero
            var hero = player.Heroes.FirstOrDefault(h => h.TemplateId == request.TemplateId);
            if (hero == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = "No hero is currently selected."
                };
            }

            // Step 3: Get hero data
            if (!DataManager.HeroDataDic.TryGetValue(request.TemplateId, out var heroData))
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Hero template {request.TemplateId} not found in DataManager."
                };
            }

            // Step 4: Check if next level exists
            if (heroData.NextLevelId == 0)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = "This hero cannot level up further."
                };
            }

            // Step 5: Load currency inside transaction
            var currencyDb = await _dbContext.Currencies.FirstOrDefaultAsync(c => c.PlayerDbId == accountDbId);
            if (currencyDb == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = "Currency data not found."
                };
            }

            // Step 6: Check if player has enough currency
            foreach (var currency in heroData.LevelUpCurrencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                int balance = currency.currencyType switch
                {
                    Define.ECurrencyType.Gold => currencyDb.Gold,
                    Define.ECurrencyType.Dia => currencyDb.Dia,
                    Define.ECurrencyType.BlueGem => currencyDb.BlueGem,
                    Define.ECurrencyType.GreenGem => currencyDb.GreenGem,
                    Define.ECurrencyType.YellowGem => currencyDb.YellowGem,
                    Define.ECurrencyType.StoneArmor => currencyDb.StoneArmor,
                    Define.ECurrencyType.StoneBelt => currencyDb.StoneBelt,
                    Define.ECurrencyType.StoneBoots => currencyDb.StoneBoots,
                    Define.ECurrencyType.StoneGloves => currencyDb.StoneGloves,
                    Define.ECurrencyType.StoneRing => currencyDb.StoneRing,
                    Define.ECurrencyType.StoneWeapon => currencyDb.StoneWeapon,
                    Define.ECurrencyType.Exp => currencyDb.Exp,
                    Define.ECurrencyType.ScrollArmor => currencyDb.ScrollArmor,
                    Define.ECurrencyType.ScrollBelt => currencyDb.ScrollBelt,
                    Define.ECurrencyType.ScrollBoots => currencyDb.ScrollBoots,
                    Define.ECurrencyType.ScrollGloves => currencyDb.ScrollGloves,
                    Define.ECurrencyType.ScrollRing => currencyDb.ScrollRing,
                    Define.ECurrencyType.ScrollWeapon => currencyDb.ScrollWeapon,
                    _ => 0
                };

                if (balance < currency.count)
                {
                    return new HeroListRes
                    {
                        Success = false,
                        Message = "Not enough currency for hero level up."
                    };
                }
            }

            // Step 7: Deduct currency
            foreach (var currency in heroData.LevelUpCurrencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                switch (currency.currencyType)
                {
                    case Define.ECurrencyType.Gold: currencyDb.Gold -= currency.count; break;
                    case Define.ECurrencyType.Dia: currencyDb.Dia -= currency.count; break;
                    case Define.ECurrencyType.BlueGem: currencyDb.BlueGem -= currency.count; break;
                    case Define.ECurrencyType.GreenGem: currencyDb.GreenGem -= currency.count; break;
                    case Define.ECurrencyType.YellowGem: currencyDb.YellowGem -= currency.count; break;
                    case Define.ECurrencyType.StoneArmor: currencyDb.StoneArmor -= currency.count; break;
                    case Define.ECurrencyType.StoneBelt: currencyDb.StoneBelt -= currency.count; break;
                    case Define.ECurrencyType.StoneBoots: currencyDb.StoneBoots -= currency.count; break;
                    case Define.ECurrencyType.StoneGloves: currencyDb.StoneGloves -= currency.count; break;
                    case Define.ECurrencyType.StoneRing: currencyDb.StoneRing -= currency.count; break;
                    case Define.ECurrencyType.StoneWeapon: currencyDb.StoneWeapon -= currency.count; break;
                    case Define.ECurrencyType.Exp: currencyDb.Exp -= currency.count; break;
                    case Define.ECurrencyType.ScrollArmor: currencyDb.ScrollArmor -= currency.count; break;
                    case Define.ECurrencyType.ScrollBelt: currencyDb.ScrollBelt -= currency.count; break;
                    case Define.ECurrencyType.ScrollBoots: currencyDb.ScrollBoots -= currency.count; break;
                    case Define.ECurrencyType.ScrollGloves: currencyDb.ScrollGloves -= currency.count; break;
                    case Define.ECurrencyType.ScrollRing: currencyDb.ScrollRing -= currency.count; break;
                    case Define.ECurrencyType.ScrollWeapon: currencyDb.ScrollWeapon -= currency.count; break;
                }
            }

            // Step 8: Level up hero
            hero.TemplateId = heroData.NextLevelId;

            if (!DataManager.HeroDataDic.TryGetValue(hero.TemplateId, out var nextHeroData))
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Next hero template {hero.TemplateId} not found."
                };
            }

            hero.MaxExp = nextHeroData.LevelUpCurrency1Count;
            hero.NowExp = 0;

            // Step 9: Sync skills
            var orgSkillIds = hero.SkillTemplateId
                .Select(id => DataManager.HeroSkillDataDic[id].OriginalLevelId)
                .ToList();

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

            // Step 10: Save changes and commit transaction
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Step 11: Return updated hero list
            return await GetHeroListAsync(new HeroListReq { Jwt = request.Jwt });
        }


        public async Task<HeroListRes> HeroSkillUpAsync(HeroSkillLevelUpReq request)
        {
            // Step 1: Get account ID from JWT
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);

            // Step 2: Begin a transaction to ensure atomicity
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Step 3: Reload player with heroes inside transaction
            var player = await _dbContext.Players
                .Include(p => p.Heroes)
                .FirstOrDefaultAsync(p => p.PlayerDbId == accountDbId);

            if (player == null) // Step 4: Check if player exists
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Player {accountDbId} not found."
                };
            }

            // Step 5: Find the hero
            var hero = player.Heroes.FirstOrDefault(h => h.TemplateId == request.HeroTemplateId);
            if (hero == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Hero {request.HeroTemplateId} not found."
                };
            }

            // Step 6: Validate skill exists
            if (!DataManager.HeroSkillDataDic.TryGetValue(request.HeroSkillTemplateId, out var skillData))
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = $"Skill {request.HeroSkillTemplateId} not found."
                };
            }

            // Step 7: Check if skill can level up
            if (skillData.NextLevelId == 0)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = "This skill cannot level up further."
                };
            }

            // Step 8: Prepare skill list
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

            // Step 9: Load currency inside transaction
            var currencyDb = await _dbContext.Currencies
                .FirstOrDefaultAsync(c => c.PlayerDbId == accountDbId);

            if (currencyDb == null)
            {
                return new HeroListRes
                {
                    Success = false,
                    Message = "Currency data not found."
                };
            }

            // Step 10: Check if player has enough currency
            foreach (var currency in skillData.LevelUpCurrencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                int balance = currency.currencyType switch
                {
                    Define.ECurrencyType.Gold => currencyDb.Gold,
                    Define.ECurrencyType.Dia => currencyDb.Dia,
                    Define.ECurrencyType.BlueGem => currencyDb.BlueGem,
                    Define.ECurrencyType.GreenGem => currencyDb.GreenGem,
                    Define.ECurrencyType.YellowGem => currencyDb.YellowGem,
                    Define.ECurrencyType.StoneArmor => currencyDb.StoneArmor,
                    Define.ECurrencyType.StoneBelt => currencyDb.StoneBelt,
                    Define.ECurrencyType.StoneBoots => currencyDb.StoneBoots,
                    Define.ECurrencyType.StoneGloves => currencyDb.StoneGloves,
                    Define.ECurrencyType.StoneRing => currencyDb.StoneRing,
                    Define.ECurrencyType.StoneWeapon => currencyDb.StoneWeapon,
                    Define.ECurrencyType.Exp => currencyDb.Exp,
                    Define.ECurrencyType.ScrollArmor => currencyDb.ScrollArmor,
                    Define.ECurrencyType.ScrollBelt => currencyDb.ScrollBelt,
                    Define.ECurrencyType.ScrollBoots => currencyDb.ScrollBoots,
                    Define.ECurrencyType.ScrollGloves => currencyDb.ScrollGloves,
                    Define.ECurrencyType.ScrollRing => currencyDb.ScrollRing,
                    Define.ECurrencyType.ScrollWeapon => currencyDb.ScrollWeapon,
                    _ => 0
                };

                if (balance < currency.count) // Not enough currency
                {
                    return new HeroListRes
                    {
                        Success = false,
                        Message = "Not enough currency for skill upgrade."
                    };
                }
            }

            // Step 11: Deduct currency
            foreach (var currency in skillData.LevelUpCurrencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                switch (currency.currencyType)
                {
                    case Define.ECurrencyType.Gold: currencyDb.Gold -= currency.count; break;
                    case Define.ECurrencyType.Dia: currencyDb.Dia -= currency.count; break;
                    case Define.ECurrencyType.BlueGem: currencyDb.BlueGem -= currency.count; break;
                    case Define.ECurrencyType.GreenGem: currencyDb.GreenGem -= currency.count; break;
                    case Define.ECurrencyType.YellowGem: currencyDb.YellowGem -= currency.count; break;
                    case Define.ECurrencyType.StoneArmor: currencyDb.StoneArmor -= currency.count; break;
                    case Define.ECurrencyType.StoneBelt: currencyDb.StoneBelt -= currency.count; break;
                    case Define.ECurrencyType.StoneBoots: currencyDb.StoneBoots -= currency.count; break;
                    case Define.ECurrencyType.StoneGloves: currencyDb.StoneGloves -= currency.count; break;
                    case Define.ECurrencyType.StoneRing: currencyDb.StoneRing -= currency.count; break;
                    case Define.ECurrencyType.StoneWeapon: currencyDb.StoneWeapon -= currency.count; break;
                    case Define.ECurrencyType.Exp: currencyDb.Exp -= currency.count; break;
                    case Define.ECurrencyType.ScrollArmor: currencyDb.ScrollArmor -= currency.count; break;
                    case Define.ECurrencyType.ScrollBelt: currencyDb.ScrollBelt -= currency.count; break;
                    case Define.ECurrencyType.ScrollBoots: currencyDb.ScrollBoots -= currency.count; break;
                    case Define.ECurrencyType.ScrollGloves: currencyDb.ScrollGloves -= currency.count; break;
                    case Define.ECurrencyType.ScrollRing: currencyDb.ScrollRing -= currency.count; break;
                    case Define.ECurrencyType.ScrollWeapon: currencyDb.ScrollWeapon -= currency.count; break;
                }
            }

            // Step 12: Upgrade skill
            skillList[skillIndex] = skillData.NextLevelId;
            hero.SkillTemplateId = skillList;

            // Step 13: Save changes and commit transaction
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Step 14: Return updated hero list
            return await GetHeroListAsync(new HeroListReq { Jwt = request.Jwt });
        }


    }
}
