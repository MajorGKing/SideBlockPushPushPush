using AccountServer.Data;
using GameDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Data;

namespace AccountServer.Services
{
    public class PlayerService
    {
        GameDbContext _dbContext;
        JwtTokenService _jwt;
        //HeroService _hero;
        //BuddyService _buddy;
        IServiceProvider _serviceProvider;

        public PlayerService(GameDbContext context, JwtTokenService jwt, IServiceProvider serviceProvider)
        {
            _dbContext = context;
            _jwt = jwt;
            //_hero = heroService;
            //_buddy = buddyService;
            _serviceProvider = serviceProvider;
        }

        public async Task<PlayerDb> GetPlayerDbFromAccountDbId(int accountDbId)
        {
            // Player + Heroes + Buddy + Currency + Stage로드
            var player = await _dbContext.Players
                .Include(p => p.Heroes)
                .Include(p => p.Buddies)
                .Include(p => p.Currency)
                .Include(p => p.Stages)
                .FirstOrDefaultAsync(p => p.PlayerDbId == accountDbId);

            return player;
        }

        // '로드 또는 생성' 로직을 구현하는 핵심 메서드
        public async Task<PlayerPacketRes> LoadOrCreatePlayerAsync(PlayerPacketReq request)
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
                    CurrentStage = 1,

                    // 새로운 플레이어를 만들 때 CurrencyDb도 함께 추가합니다.
                    Currency = new CurrencyDb()
                    {
                        Gold = 1000000,
                        Dia = 1000000,
                        BlueGem = 50,
                        GreenGem = 50,
                        YellowGem = 50,
                        StoneArmor = 50,
                        StoneBelt = 50,
                        StoneBoots = 50,
                        StoneGloves = 50,
                        StoneRing = 50,
                        StoneWeapon = 50,
                        Exp = 50,
                        ScrollArmor = 50,
                        ScrollBelt = 50,
                        ScrollBoots = 50,
                        ScrollGloves = 50,
                        ScrollRing = 50,
                        ScrollWeapon = 50,
                    }
                };

                _dbContext.Players.Add(playerDb);
                await _dbContext.SaveChangesAsync(); // 새 PlayerDb를 GameDB에 저장합니다.

                // 지연 주입
                var heroService = _serviceProvider.GetRequiredService<HeroService>();
                var buddyService = _serviceProvider.GetRequiredService<BuddyService>();
                var stageService = _serviceProvider.GetRequiredService<StageService>();

                // 2. 기본 영웅 두 개 지급 (HeroService 호출)
                await heroService.HeroCreate(request.jwt, 100, true);   // 첫 번째 영웅
                await heroService.HeroCreate(request.jwt, 200, false);  // 두 번째 영웅

                // 3. 기본 버디 네 개 지급
                await buddyService.BuddyCreate(request.jwt, 300000100, 0);
                await buddyService.BuddyCreate(request.jwt, 100000100, 1);
                await buddyService.BuddyCreate(request.jwt, 100000300, 2);
                await buddyService.BuddyCreate(request.jwt, 100000500, 3);

                // 4. 기본 스테이지 설정
                await stageService.StageCreate(request.jwt, 1);

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
                CurrentStage = playerDb.CurrentStage,
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
