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
                    BuddySaveDataDbId = b.BuddySaveDataDbId,
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
    }
}
