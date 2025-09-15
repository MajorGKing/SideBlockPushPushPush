using GameDB;
using Server.Data;

namespace AccountServer.Services
{
    public class BuddyService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;
        PlayerService _player;

        public BuddyService(GameDbContext dbContext, JwtTokenService jwt, PlayerService player)
        {
            _dbContext = dbContext;
            _jwt = jwt;
            _player = player;
        }

        public async Task<bool> CreateBuddy(string jwt, int templateId, int selectedNumber = -1)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(jwt);

            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
            {
                throw new InvalidOperationException($"Player {accountDbId} not found.");
            }

            if (player.Buddies.Any(b => b.TemplateId == templateId))
            {
                throw new InvalidOperationException($"Player {accountDbId} already owns buddy {templateId}.");
            }

            if (DataManager.BuddyDataDic.TryGetValue(templateId, out var buddyData) == false)
            {
                throw new ArgumentException($"Invalid buddy templateId: {templateId}");
            }

            // If the selectedNumber is a "slot" index, ensure it's not already used
            if (selectedNumber != -1 && player.Buddies.Any(b => b.SelectedNumber == selectedNumber))
                throw new InvalidOperationException($"Selected slot {selectedNumber} is already occupied for player {accountDbId}.");

            // Build entity (SkillTemplateId is a NotMapped property; setting it will populate SkillTemplateIdString)
            var buddy = new BuddySaveDataDb
            {
                TemplateId = templateId,
                SkillTemplateId = buddyData.SKillIds ?? new List<int>(),
                SelectedNumber = selectedNumber,
                PlayerDbId = accountDbId,
            };

            player.Buddies.Add(buddy);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<BuddyListRes> GetBuddyListAsync(BuddyListReq request)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);

            // Load player with buddies
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
            {
                return new BuddyListRes
                {
                    Success = false,
                    Message = $"Player {accountDbId} not found."
                };
            }

            // Convert BuddySaveDataDb → BuddyDTO
            var buddyDtos = player.Buddies
                .Select(b => new BuddyDTO
                {
                    TemplateId = b.TemplateId,
                    SkillTemplateId = b.SkillTemplateId,
                    SelectedNumber = b.SelectedNumber
                })
                .ToList();

            return new BuddyListRes
            {
                Success = true,
                Message = "Buddy list retrieved successfully.",
                Buddies = buddyDtos,
            };
        }

        public async Task<BuddyListRes> RemoveSelectedBuddyListAsync(BuddySelectedRemoveReq request)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
                return new BuddyListRes { Success = false, Message = $"Player {accountDbId} not found." };

            var buddy = player.Buddies.FirstOrDefault(b => b.TemplateId == request.TemplateId);
            if (buddy == null)
                return new BuddyListRes { Success = false, Message = $"Buddy {request.TemplateId} not found." };

            // Remove from selection
            buddy.SelectedNumber = -1;

            // Reorder remaining selected buddies
            var selectedBuddies = player.Buddies
                .Where(b => b.SelectedNumber >= 0)
                .OrderBy(b => b.SelectedNumber)
                .ToList();

            for (int i = 0; i < selectedBuddies.Count; i++)
            {
                selectedBuddies[i].SelectedNumber = i;
            }

            await _dbContext.SaveChangesAsync();

            // Reuse GetBuddyListAsync to return fresh list
            return await GetBuddyListAsync(new BuddyListReq { Jwt = request.Jwt });
        }

        public async Task<BuddyListRes> AddSelectedBuddyListAsync(BuddySelectedAddReq request)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);

            if (player == null)
                return new BuddyListRes { Success = false, Message = $"Player {accountDbId} not found." };

            var buddy = player.Buddies.FirstOrDefault(b => b.TemplateId == request.TemplateId);
            if (buddy == null)
                return new BuddyListRes { Success = false, Message = $"Buddy {request.TemplateId} not found." };

            // If already selected → nothing to do
            if (buddy.SelectedNumber >= 0)
                return new BuddyListRes { Success = true, Buddies = null };

            // Find first empty slot
            var nextSlot = player.Buddies
                .Where(b => b.SelectedNumber >= 0)
                .Select(b => b.SelectedNumber)
                .DefaultIfEmpty(-1)
                .Max() + 1;

            if (nextSlot < 4)
            {
                buddy.SelectedNumber = nextSlot;
                await _dbContext.SaveChangesAsync();

                // Return updated buddy list only when change happens
                return await GetBuddyListAsync(new BuddyListReq { Jwt = request.Jwt });
            }

            // Slots full → nothing changes
            return new BuddyListRes { Success = true, Buddies = null };
        }

        public async Task<BuddyListRes> LevelUpBuddyAsync(BuddyLevelUpReq request)
        {
            var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);

            // Step 0: Begin transaction
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Step 1: Load player (Heroes, Buddies, Currency are included by service)
            var player = await _player.GetPlayerDbFromAccountDbId(accountDbId);
            if (player == null)
            {
                return new BuddyListRes
                {
                    Success = false,
                    Message = $"Player {accountDbId} not found."
                };
            }

            // Step 2: Find the buddy
            var buddy = player.Buddies.FirstOrDefault(b => b.TemplateId == request.TemplateId);
            if (buddy == null)
            {
                return new BuddyListRes
                {
                    Success = false,
                    Message = $"Buddy {request.TemplateId} not found."
                };
            }

            // Step 3: Load buddy data from DataManager
            if (!DataManager.BuddyDataDic.TryGetValue(request.TemplateId, out var buddyData))
            {
                return new BuddyListRes
                {
                    Success = false,
                    Message = $"Buddy template {request.TemplateId} not found in DataManager."
                };
            }

            // Step 4: Check if next level exists
            if (buddyData.NextLevelId == 0)
            {
                return new BuddyListRes
                {
                    Success = false,
                    Message = "This buddy cannot level up further."
                };
            }

            // Step 5: Check currency
            foreach (var currency in buddyData.LevelUpCurrencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                int balance = currency.currencyType switch
                {
                    Define.ECurrencyType.Gold => player.Currency.Gold,
                    Define.ECurrencyType.Dia => player.Currency.Dia,
                    Define.ECurrencyType.BlueGem => player.Currency.BlueGem,
                    Define.ECurrencyType.GreenGem => player.Currency.GreenGem,
                    Define.ECurrencyType.YellowGem => player.Currency.YellowGem,
                    Define.ECurrencyType.StoneArmor => player.Currency.StoneArmor,
                    Define.ECurrencyType.StoneBelt => player.Currency.StoneBelt,
                    Define.ECurrencyType.StoneBoots => player.Currency.StoneBoots,
                    Define.ECurrencyType.StoneGloves => player.Currency.StoneGloves,
                    Define.ECurrencyType.StoneRing => player.Currency.StoneRing,
                    Define.ECurrencyType.StoneWeapon => player.Currency.StoneWeapon,
                    Define.ECurrencyType.Exp => player.Currency.Exp,
                    Define.ECurrencyType.ScrollArmor => player.Currency.ScrollArmor,
                    Define.ECurrencyType.ScrollBelt => player.Currency.ScrollBelt,
                    Define.ECurrencyType.ScrollBoots => player.Currency.ScrollBoots,
                    Define.ECurrencyType.ScrollGloves => player.Currency.ScrollGloves,
                    Define.ECurrencyType.ScrollRing => player.Currency.ScrollRing,
                    Define.ECurrencyType.ScrollWeapon => player.Currency.ScrollWeapon,
                    _ => 0
                };

                if (balance < currency.count)
                {
                    return new BuddyListRes
                    {
                        Success = false,
                        Message = "Not enough currency for buddy level up."
                    };
                }
            }

            // Step 6: Deduct currency
            foreach (var currency in buddyData.LevelUpCurrencies)
            {
                if (currency.currencyType == Define.ECurrencyType.None)
                    continue;

                switch (currency.currencyType)
                {
                    case Define.ECurrencyType.Gold: player.Currency.Gold -= currency.count; break;
                    case Define.ECurrencyType.Dia: player.Currency.Dia -= currency.count; break;
                    case Define.ECurrencyType.BlueGem: player.Currency.BlueGem -= currency.count; break;
                    case Define.ECurrencyType.GreenGem: player.Currency.GreenGem -= currency.count; break;
                    case Define.ECurrencyType.YellowGem: player.Currency.YellowGem -= currency.count; break;
                    case Define.ECurrencyType.StoneArmor: player.Currency.StoneArmor -= currency.count; break;
                    case Define.ECurrencyType.StoneBelt: player.Currency.StoneBelt -= currency.count; break;
                    case Define.ECurrencyType.StoneBoots: player.Currency.StoneBoots -= currency.count; break;
                    case Define.ECurrencyType.StoneGloves: player.Currency.StoneGloves -= currency.count; break;
                    case Define.ECurrencyType.StoneRing: player.Currency.StoneRing -= currency.count; break;
                    case Define.ECurrencyType.StoneWeapon: player.Currency.StoneWeapon -= currency.count; break;
                    case Define.ECurrencyType.Exp: player.Currency.Exp -= currency.count; break;
                    case Define.ECurrencyType.ScrollArmor: player.Currency.ScrollArmor -= currency.count; break;
                    case Define.ECurrencyType.ScrollBelt: player.Currency.ScrollBelt -= currency.count; break;
                    case Define.ECurrencyType.ScrollBoots: player.Currency.ScrollBoots -= currency.count; break;
                    case Define.ECurrencyType.ScrollGloves: player.Currency.ScrollGloves -= currency.count; break;
                    case Define.ECurrencyType.ScrollRing: player.Currency.ScrollRing -= currency.count; break;
                    case Define.ECurrencyType.ScrollWeapon: player.Currency.ScrollWeapon -= currency.count; break;
                }
            }

            // Step 7: Level up buddy
            buddy.TemplateId = buddyData.NextLevelId;

            if (!DataManager.BuddyDataDic.TryGetValue(buddy.TemplateId, out var nextBuddyData))
            {
                return new BuddyListRes
                {
                    Success = false,
                    Message = $"Next buddy template {buddy.TemplateId} not found."
                };
            }

            // Step 8: Sync skills
            var orgSkillIds = buddy.SkillTemplateId
                .Select(id => DataManager.BuddySkillDataDic[id].OriginalLevelId)
                .ToList();

            var skills = buddy.SkillTemplateId.ToList();
            foreach (var skillId in nextBuddyData.SKillIds)
            {
                var originalId = DataManager.BuddySkillDataDic[skillId].OriginalLevelId;
                if (!orgSkillIds.Contains(originalId))
                {
                    skills.Add(skillId);
                }
            }
            buddy.SkillTemplateId = skills;

            // Step 9: Save changes and commit
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // Step 10: Return updated buddy list
            return await GetBuddyListAsync(new BuddyListReq { Jwt = request.Jwt });
        }
    }
}
