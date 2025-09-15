using AccountServer.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        PlayerService _player;
        CurrencyService _currency;
        HeroService _hero;
        BuddyService _buddy;

        public GameController(PlayerService player, CurrencyService currency, HeroService heroService, BuddyService buddyService)
        {
            _player = player;
            _currency = currency;
            _hero = heroService;
            _buddy = buddyService;
        }

        [HttpPost]
        [Route("player")]
        public async Task<PlayerPacketRes> PlayerData([FromBody] PlayerPacketReq req)
        {
            return await _player.LoadOrCreatePlayerAsync(req);
        }

        [HttpPost]
        [Route("currency")]
        public async Task<CurrencyAllRes> CurrencyData([FromBody] CurrencyAllReq req)
        {
            return await _currency.GetPlayerCurrenciesAsync(req.jwt);
        }

        [HttpPost]
        [Route("currency/add")]
        public async Task<CurrencyAllRes> CurrencyAdd([FromBody] CurrencyAddReq req)
        {
            return await _currency.UpdatePlayerCurrencyAsync(req);
        }

        [HttpPost]
        [Route("hero")]
        public async Task<HeroListRes> HeroData([FromBody] HeroListReq req)
        {
            return await _hero.GetHeroListAsync(req);
        }

        [HttpPost]
        [Route("hero/nowHeroChange")]
        public async Task<HeroListRes> HeroSelectedChange([FromBody] HeroNowChangeReq req)
        {
            return await _hero.ChangeSelectedHeroAsync(req);
        }

        [HttpPost]
        [Route("hero/levelUp")]
        public async Task<HeroListRes> HeroLevelUp([FromBody] HeroLevelUpReq req)
        {
            return await _hero.LevelUpHeroAsync(req);
        }

        [HttpPost("hero/skillLevelUp")]
        public async Task<ActionResult<HeroListRes>> HeroSkillUp([FromBody] HeroSkillLevelUpReq req)
        {
            var result = await _hero.HeroSkillUpAsync(req);
            return Ok(result);
        }

        [HttpPost]
        [Route("buddy")]
        public async Task<BuddyListRes> BuddyData([FromBody] BuddyListReq req)
        {
            return await _buddy.GetBuddyListAsync(req);
        }

        [HttpPost]
        [Route("buddy/selectedRemove")]
        public async Task<BuddyListRes> BuddySelectedRemove([FromBody] BuddySelectedRemoveReq req)
        {
            return await _buddy.RemoveSelectedBuddyListAsync(req);
        }

        [HttpPost]
        [Route("buddy/selectedAdd")]
        public async Task<BuddyListRes> BuddySelectedAdd([FromBody] BuddySelectedAddReq req)
        {
            return await _buddy.AddSelectedBuddyListAsync(req);
        }

        [HttpPost]
        [Route("buddy/levelUp")]
        public async Task<BuddyListRes> BuddyLevelUp([FromBody] BuddyLevelUpReq req)
        {
            return await _buddy.LevelUpBuddyAsync(req);
        }
    }
}
