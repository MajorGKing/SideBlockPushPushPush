using GameDB;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace AccountServer.Services
{
    public class HeroService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;

        public HeroService(GameDbContext context, JwtTokenService jwt)
        {
            _dbContext = context;
            _jwt = jwt;
        }

        public async Task<bool> CreateHero(string jwt, int templateId, bool isSelected = false)
        {
            var token = _jwt.DecipherJwtAccessToken(jwt);
            var subClaim = token.Claims.FirstOrDefault(c => c.Type == "sub");

            if (subClaim == null)
            {
                throw new UnauthorizedAccessException("JWT 토큰에 'sub' 클레임이 존재하지 않습니다.");
            }

            if (!int.TryParse(subClaim.Value, out int accountDbId))
            {
                throw new FormatException("'sub' 클레임 값이 정수로 변환되지 않았습니다.");
            }


            // Player 존재 여부 확인
            var player = await _dbContext.Players
                .Include(p => p.Heroes)
                .FirstOrDefaultAsync(p => p.PlayerDbId == accountDbId);

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
            var token = _jwt.DecipherJwtAccessToken(request.jwt);
            var subClaim = token.Claims.FirstOrDefault(c => c.Type == "sub");

            if (subClaim == null)
            {
                throw new UnauthorizedAccessException("JWT 토큰에 'sub' 클레임이 존재하지 않습니다.");
            }

            if (!int.TryParse(subClaim.Value, out int accountDbId))
            {
                throw new FormatException("'sub' 클레임 값이 정수로 변환되지 않았습니다.");
            }

            // Player + Heroes 로드
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
    }
}
