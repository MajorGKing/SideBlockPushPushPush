using AccountServer.Data;
using GameDB;
using Microsoft.EntityFrameworkCore;
using static AccountServer.Define;
using CurrencyType = AccountServer.Data.CurrencyType;
using DbCurrencyType = GameDB.CurrencyType;

namespace AccountServer.Services
{
    public class CurrencyService
    {

        GameDbContext _dbContext;
        JwtTokenService _jwt;
        QuestService _quest;

        public CurrencyService(GameDbContext context, JwtTokenService jwt, QuestService quest)
        {
            _dbContext = context;
            _jwt = jwt;
            _quest = quest;
        }

        /// <summary>
        /// 플레이어의 모든 화폐 정보를 가져옵니다.
        /// </summary>
        /// <param name="jwt">JWT 토큰 </param>
        /// <returns>CurrencyDb 객체 또는 null</returns>
        public async Task<CurrencyAllRes> GetPlayerCurrenciesAsync(string jwt)
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
                return new CurrencyAllRes { Success = false, currencyData = null};
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

            return new CurrencyAllRes
            {
                Success = true,
                currencyData = currencyDto
            };
        }

        public async Task<CurrencyAllRes> UpdatePlayerCurrencyAsync(CurrencyAddReq request, bool commitChanges = true)
        {
            var token = _jwt.DecipherJwtAccessToken(request.jwt);
            var subClaim = token.Claims.FirstOrDefault(c => c.Type == "sub");

            if (subClaim == null || !int.TryParse(subClaim.Value, out int playerDbId))
            {
                throw new UnauthorizedAccessException("JWT 토큰의 'sub' 클레임이 잘못되었습니다.");
            }

            var currencyDb = await _dbContext.Currencies.FirstOrDefaultAsync(c => c.PlayerDbId == playerDbId);

            if (currencyDb == null)
            {
                return new CurrencyAllRes { Success = false, currencyData = null };
            }

            // 증감 처리
            switch (request.CurrencyType)
            {
                case CurrencyType.Gold:
                    currencyDb.Gold += request.Amount;
                    if (request.Amount < 0)
                    {
                        await _quest.MissionEventAsncHandle(request.jwt, EBroadcastEventType.UseGold, -request.Amount, commitChanges);
                        await _quest.MissionEventAsncHandle(request.jwt, EBroadcastEventType.ChangeGold, -request.Amount, commitChanges);
                    }
                    else if (request.Amount > 0)
                    {
                        await _quest.MissionEventAsncHandle(request.jwt, EBroadcastEventType.GetGold, request.Amount, commitChanges);
                        await _quest.MissionEventAsncHandle(request.jwt, EBroadcastEventType.ChangeGold, request.Amount, commitChanges);
                    }
                    break;
                case CurrencyType.Dia:
                    currencyDb.Dia += request.Amount;
                    break;
                case CurrencyType.BlueGem:
                    currencyDb.BlueGem += request.Amount;
                    break;
                case CurrencyType.GreenGem:
                    currencyDb.GreenGem += request.Amount;
                    break;
                case CurrencyType.YellowGem:
                    currencyDb.YellowGem += request.Amount;
                    break;
                case CurrencyType.StoneArmor:
                    currencyDb.StoneArmor += request.Amount;
                    break;
                case CurrencyType.StoneBelt:
                    currencyDb.StoneBelt += request.Amount;
                    break;
                case CurrencyType.StoneBoots:
                    currencyDb.StoneBoots += request.Amount;
                    break;
                case CurrencyType.StoneGloves:
                    currencyDb.StoneGloves += request.Amount;
                    break;
                case CurrencyType.StoneRing:
                    currencyDb.StoneRing += request.Amount;
                    break;
                case CurrencyType.StoneWeapon:
                    currencyDb.StoneWeapon += request.Amount;
                    break;
                case CurrencyType.Exp:
                    currencyDb.Exp += request.Amount;
                    break;
                case CurrencyType.ScrollArmor:
                    currencyDb.ScrollArmor += request.Amount;
                    break;
                case CurrencyType.ScrollBelt:
                    currencyDb.ScrollBelt += request.Amount;
                    break;
                case CurrencyType.ScrollBoots:
                    currencyDb.ScrollBoots += request.Amount;
                    break;
                case CurrencyType.ScrollGloves:
                    currencyDb.ScrollGloves += request.Amount;
                    break;
                case CurrencyType.ScrollRing:
                    currencyDb.ScrollRing += request.Amount;
                    break;
                case CurrencyType.ScrollWeapon:
                    currencyDb.ScrollWeapon += request.Amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("지원하지 않는 CurrencyType입니다.");
            }

            if(commitChanges == true)
            {
                await _dbContext.SaveChangesAsync();
            }
            

            // 갱신된 데이터를 다시 DTO로 변환
            var updatedCurrency = new CurrencyData
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

            return new CurrencyAllRes
            {
                Success = true,
                currencyData = updatedCurrency
            };
        }

        public async Task<int> GetCurrency(int accountDbId, Define.ECurrencyType currencyType)
        {
            var currencyDb = await _dbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.PlayerDbId == accountDbId);

            if (currencyDb == null)
            {
                return 0;
            }

            // enum에 맞춰서 해당 컬럼 값을 반환
            switch (currencyType)
            {
                case Define.ECurrencyType.None: return 0;
                case Define.ECurrencyType.Gold: return currencyDb.Gold;
                case Define.ECurrencyType.Dia: return currencyDb.Dia;
                case Define.ECurrencyType.BlueGem: return currencyDb.BlueGem;
                case Define.ECurrencyType.GreenGem: return currencyDb.GreenGem;
                case Define.ECurrencyType.YellowGem: return currencyDb.YellowGem;
                case Define.ECurrencyType.StoneArmor: return currencyDb.StoneArmor;
                case Define.ECurrencyType.StoneBelt: return currencyDb.StoneBelt;
                case Define.ECurrencyType.StoneBoots: return currencyDb.StoneBoots;
                case Define.ECurrencyType.StoneGloves: return currencyDb.StoneGloves;
                case Define.ECurrencyType.StoneRing: return currencyDb.StoneRing;
                case Define.ECurrencyType.StoneWeapon: return currencyDb.StoneWeapon;
                case Define.ECurrencyType.Exp: return currencyDb.Exp;
                case Define.ECurrencyType.ScrollArmor: return currencyDb.ScrollArmor;
                case Define.ECurrencyType.ScrollBelt: return currencyDb.ScrollBelt;
                case Define.ECurrencyType.ScrollBoots: return currencyDb.ScrollBoots;
                case Define.ECurrencyType.ScrollGloves: return currencyDb.ScrollGloves;
                case Define.ECurrencyType.ScrollRing: return currencyDb.ScrollRing;
                case Define.ECurrencyType.ScrollWeapon: return currencyDb.ScrollWeapon;
                default: return 0;
            }
        }
    }

}
