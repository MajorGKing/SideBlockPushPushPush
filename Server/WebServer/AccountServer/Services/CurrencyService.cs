using GameDB;
using Microsoft.EntityFrameworkCore;

namespace AccountServer.Services
{
    public class CurrencyService
    {

        GameDbContext _dbContext;
        JwtTokenService _jwt;

        public CurrencyService(GameDbContext context, JwtTokenService jwt)
        {
            _dbContext = context;
            _jwt = jwt;
        }

        /// <summary>
        /// 플레이어의 모든 화폐 정보를 가져옵니다.
        /// </summary>
        /// <param name="jwt">JWT 토큰 </param>
        /// <returns>CurrencyDb 객체 또는 null</returns>
        public async Task<CurrenyAllRes> GetPlayerCurrenciesAsync(string jwt)
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

            // playerId를 이용해 DB에서 화폐 정보를 찾습니다.
            var currencyDb = await _dbContext.Currencies.FirstOrDefaultAsync(c => c.PlayerDbId == accountDbId);

            if (currencyDb == null)
            {
                return new CurrenyAllRes { Success = false, currencyData = null};
            }

            // CurrencyDb 엔티티를 CurrencyData로 변환합니다.
            var currencyDto = new CurrencyData
            {
                PlayerDbId = currencyDb.PlayerDbId,
                Gold = currencyDb.Gold,
                Dia = currencyDb.Dia,
                BlueGem = currencyDb.BlueGem,
                GreenGem = currencyDb.GreenGem,
                YellowGem = currencyDb.YellowGem,
                StoneArmor = currencyDb.StoneArmor,
                StoneBelt = currencyDb.StoneBelt,
                StoneBoots = currencyDb.StoneBoots,
                StoneGloves = currencyDb.StoneGloves,
                StoneRing = currencyDb.StoneRing,
                StoneWeapon = currencyDb.StoneWeapon,
                Exp = currencyDb.Exp,
                ScrollArmor = currencyDb.ScrollArmor,
                ScrollBelt = currencyDb.ScrollBelt,
                ScrollBoots = currencyDb.ScrollBoots,
                ScrollGloves = currencyDb.ScrollGloves,
                ScrollRing = currencyDb.ScrollRing,
                ScrollWeapon = currencyDb.ScrollWeapon
            };

            return new CurrenyAllRes
            {
                Success = true,
                currencyData = currencyDto
            };
        }
    }
}
