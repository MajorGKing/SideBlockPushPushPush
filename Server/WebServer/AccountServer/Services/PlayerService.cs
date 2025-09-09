using GameDB;
using Microsoft.EntityFrameworkCore;

namespace AccountServer.Services
{
    public class PlayerService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;

        public PlayerService(GameDbContext context, JwtTokenService jwt)
        {
            _dbContext = context;
            _jwt = jwt;
        }

        // '로드 또는 생성' 로직을 구현하는 핵심 메서드
        public async Task<PlayerPacketRes> LoadOrCreatePlayerAsync(string jwt)
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

            // 1. GameDB에서 PlayerDbId로 데이터를 찾습니다.
            PlayerDb? playerDb = await _dbContext.Players.FirstOrDefaultAsync(p => p.PlayerDbId == accountDbId);


            // 2. 데이터가 없으면 새로 생성합니다.
            if (playerDb == null)
            {
                playerDb = new PlayerDb()
                {
                    PlayerDbId = accountDbId, // AccountDbId와 동일한 값으로 설정
                    UserLevel = 1,
                    UserName = "player", // 초기 닉네임 설정
                    Stamina = 50,
                    BGMOn = true,
                    EffectSoundOn = true,
                    LastMissionTime = DateTime.Now,
                };

                _dbContext.Players.Add(playerDb);
                await _dbContext.SaveChangesAsync(); // 새 PlayerDb를 GameDB에 저장합니다.
            }

            // 4. PlayerDb 객체를 PlayerData DTO로 변환하여 반환합니다.
            var playerData = new PlayerData
            {
                PlayerDbId = playerDb.PlayerDbId,
                UserLevel = playerDb.UserLevel,
                UserName = playerDb.UserName,
                Stamina = playerDb.Stamina,
                BGMOn = playerDb.BGMOn,
                EffectSoundOn = playerDb.EffectSoundOn,
                LastMissionTime = playerDb.LastMissionTime,
            };

            PlayerPacketRes res = new PlayerPacketRes()
            {
                Success = true,
                Message = "",
                PlayerData = playerData,
            };

            return res;
        }
    }
}
