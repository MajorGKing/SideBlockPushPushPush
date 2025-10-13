using AccountServer.Data;
using GameDB;
using Microsoft.EntityFrameworkCore;
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
            // Player + Heroes + Buddy + Currency + Stage + Mission + Achievement로드
            var player = await _dbContext.Players
                .Include(p => p.Heroes)
                .Include(p => p.Buddies)
                .Include(p => p.Currency)
                .Include(p => p.Stages)
                .Include(p => p.Missions)
                .Include(p => p.Achievements)
                .Include(p => p.AchievementClearList)
                .Include(p => p.AchievementValues)
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
                    },

                    AchievementValues = new AchievementValueDb()
                };

                _dbContext.Players.Add(playerDb);
                await _dbContext.SaveChangesAsync(); // 새 PlayerDb를 GameDB에 저장합니다.

                // 지연 주입
                var heroService = _serviceProvider.GetRequiredService<HeroService>();
                var buddyService = _serviceProvider.GetRequiredService<BuddyService>();
                var stageService = _serviceProvider.GetRequiredService<StageService>();
                var questService = _serviceProvider.GetRequiredService<QuestService>();

                // 2. 기본 영웅 두 개 지급 (HeroService 호출)
                await heroService.HeroCreate(request.jwt, 100, true);   // 첫 번째 영웅
                await heroService.HeroCreate(request.jwt, 200, false);  // 두 번째 영웅

                // 3. 기본 버디 네 개 지급
                await buddyService.BuddyCreate(request.jwt, 300000100, 0);
                await buddyService.BuddyCreate(request.jwt, 100000100, 1);
                await buddyService.BuddyCreate(request.jwt, 100000300, 2);
                await buddyService.BuddyCreate(request.jwt, 100000500, 3);

                // 4. 기본 스테이지 설정
                await stageService.StageCreateAsync(request.jwt, 1);

                // 5. 기본 미션 설정
                foreach(var mission in DataManager.MissionDataDic.Values)
                {
                    await questService.MissionCreateAsync(request.jwt, mission.TemplateId);
                }

                // 6. 기본 업적 설정
                await questService.AddNewAchievementsAsync(request.jwt);
            }

            // 3. PlayerDb에 신규 업적 추가
            {
                var questService = _serviceProvider.GetRequiredService<QuestService>();
                await questService.AddNewAchievementsAsync(request.jwt);
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
                LastStaminaUpdateTime = playerDb.LastStaminaUpdateTime,
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

        public async Task<PlayerTimeCheckRes> PlayerTimeCheck(PlayerTimeCheckReq request)
        {
            var response = new PlayerTimeCheckRes();
            bool missionChanged = false;
            bool staminaChanged = false;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // step1. JWT에서 accountDbId 추출
                var accountDbId = _jwt.GetAccountDbIdInJwt(request.Jwt);
                var player = await GetPlayerDbFromAccountDbId(accountDbId);

                if (player == null)
                {
                    response.Success = false;
                    response.Message = "Invalid player.";
                    return response;
                }

                DateTime now = DateTime.Now;
                DateTime today9AM = now.Date.AddHours(9);
                DateTime thisMonday9AM = GetThisMondayAt9AM(now);

                // step2. 미션 리셋 체크 (Day/Week)
                bool dayResetNeeded = false;
                bool weekResetNeeded = false;

                // Day Reset
                if ((player.LastMissionTime.Date != now.Date && now >= today9AM) ||
                    (player.LastMissionTime.Date == now.Date && player.LastMissionTime < today9AM && now >= today9AM))
                {
                    dayResetNeeded = true;
                }

                // Week Reset
                if (player.LastMissionTime < thisMonday9AM && now >= thisMonday9AM)
                {
                    weekResetNeeded = true;
                }

                if (dayResetNeeded || weekResetNeeded)
                {
                    if (dayResetNeeded)
                    {
                        ResetNormalMissions(player);  // Normal Mission 초기화
                        ResetDayMissions(player);     // Day Mission 초기화
                    }

                    if (weekResetNeeded)
                        ResetWeekMissions(player);
                    
                    missionChanged = true;
                }
                

                // step3. 스태미너 회복 체크 (3분 단위)
                {
                    if (player.Stamina == Define.MAX_STAMINA)
                    {
                        player.LastStaminaUpdateTime = now;
                    }
                    else
                    {
                        int minutesPassed = (int)(now - player.LastStaminaUpdateTime).TotalMinutes;
                        if (minutesPassed >= 3)
                        {
                            int recoverAmount = minutesPassed / 3;
                            player.Stamina = Math.Min(Define.MAX_STAMINA, player.Stamina + recoverAmount);

                            // 마지막 회복 시점 갱신
                            if (player.Stamina < Define.MAX_STAMINA)
                                player.LastStaminaUpdateTime = player.LastStaminaUpdateTime.AddMinutes(recoverAmount * 3);

                            staminaChanged = true;
                        }
                    }
                }



                // step4. DB 저장
                //if (missionChanged || staminaChanged)
                player.LastMissionTime = now;

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                response.Success = true;

                // step5. MissionList / PlayerInfo는 변경된 경우만 반환
                if (missionChanged)
                {
                    var questService = _serviceProvider.GetRequiredService<QuestService>();
                    response.MissionList = await questService.MissionListGetAsync(new GetMissionListReq { Jwt = request.Jwt });
                }

                if (staminaChanged)
                {
                    response.PlayerInfo = await LoadOrCreatePlayerAsync(new PlayerPacketReq { jwt = request.Jwt });
                }

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
                return response;
            }
        }

        #region Helper
        // 이번 주 월요일 9시 계산
        private DateTime GetThisMondayAt9AM(DateTime currentTime)
        {
            int daysSinceMonday = (int)currentTime.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysSinceMonday < 0) daysSinceMonday += 7;
            DateTime thisMonday = currentTime.Date.AddDays(-daysSinceMonday);
            return thisMonday.AddHours(9);
        }

        // Normal Mission 초기화
        private void ResetNormalMissions(PlayerDb player)
        {
            var normalMissions = player.Missions
                .Where(m => DataManager.MissionDataDic[m.TemplateId].MissionType == Define.EMissionType.Normal);

            foreach (var m in normalMissions)
            {
                m.StackedPoint = 0;
                m.MissionState = EMissionState.Progress;
                m.GetRewardCount = 0;
            }
        }

        // Day 미션 초기화
        private void ResetDayMissions(PlayerDb player)
        {
            var dayMissions = player.Missions
                .Where(m => DataManager.MissionDataDic[m.TemplateId].MissionType == Define.EMissionType.Day);

            foreach (var m in dayMissions)
            {
                m.StackedPoint = 0;
                m.MissionState = EMissionState.Progress;
                m.GetRewardCount = 0;
            }
        }

        // Week 미션 초기화
        private void ResetWeekMissions(PlayerDb player)
        {
            var weekMissions = player.Missions
                .Where(m => DataManager.MissionDataDic[m.TemplateId].MissionType == Define.EMissionType.Week);

            foreach (var m in weekMissions)
            {
                m.StackedPoint = 0;
                m.MissionState = EMissionState.Progress;
                m.GetRewardCount = 0;
            }
        }
        #endregion

    }
}
