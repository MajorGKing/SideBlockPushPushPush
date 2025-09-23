using AccountServer.Data;
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
        ShopService _shop;
        StageService _stage;

        public GameController(PlayerService player, CurrencyService currency, HeroService heroService, BuddyService buddyService, ShopService shop, StageService stage)
        {
            _player = player;
            _currency = currency;
            _hero = heroService;
            _buddy = buddyService;
            _shop = shop;
            _stage = stage;
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
            return await _hero.HeroListGetAsync(req);
        }

        [HttpPost]
        [Route("hero/nowHeroChange")]
        public async Task<HeroListRes> HeroSelectedChange([FromBody] HeroNowChangeReq req)
        {
            return await _hero.HeroSelectedChangeAsync(req);
        }

        [HttpPost]
        [Route("hero/levelUp")]
        public async Task<HeroListRes> HeroLevelUp([FromBody] HeroLevelUpReq req)
        {
            return await _hero.HeroLevelUpAsync(req);
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
            return await _buddy.BuddyListGetAsync(req);
        }

        [HttpPost]
        [Route("buddy/selectedRemove")]
        public async Task<BuddyListRes> BuddySelectedRemove([FromBody] BuddySelectedRemoveReq req)
        {
            return await _buddy.BuddySelectedListRemoveAsync(req);
        }

        [HttpPost]
        [Route("buddy/selectedAdd")]
        public async Task<BuddyListRes> BuddySelectedAdd([FromBody] BuddySelectedAddReq req)
        {
            return await _buddy.BuddySelectedListAddAsync(req);
        }

        [HttpPost]
        [Route("buddy/levelUp")]
        public async Task<BuddyListRes> BuddyLevelUp([FromBody] BuddyLevelUpReq req)
        {
            return await _buddy.BuddyLevelUpAsync(req);
        }

        [HttpPost]
        [Route("buddy/skillUp")]
        public async Task<BuddyListRes> BuddySkillUp([FromBody] BuddySkillLevelUpReq req)
        {
            return await _buddy.BuddySkillUpAsync(req);
        }

        [HttpPost]
        [Route("shop/heroGachaDo")]
        public async Task<ShopHeroGachaRes> HeroGachaDo([FromBody] ShopHeroGachaReq req)
        {
            return await _shop.HeroGachaDoAsync(req);
        }

        [HttpPost]
        [Route("shop/buddyGachaDo")]
        public async Task<ShopBuddyGachaRes> BuddyGachaDo([FromBody] ShopBuddyGachaReq req)
        {
            return await _shop.BuddyGachaDoAsync(req);
        }

        [HttpPost]
        [Route("shop/currencyGachaDo")]
        public async Task<ShopCurrencyGachaRes> CurrencyGachaDo([FromBody] ShopCurrencyGachaReq req)
        {
            return await _shop.CurrencyGachaDoAsync(req);
        }

        [HttpPost]
        [Route("stage/getClearStageList")]
        public async Task<StageClearListRes> StageClearListGet([FromBody]  StageClearListReq req)
        {
            return await _stage.StageListGetAsync(req);
        }

        [HttpPost]
        [Route("stage/setClearStageNext")]
        public async Task<SetNextStageRes> SetStageClearNext([FromBody] SetNextStageReq req)
        {
            return await _stage.SetNextStageAsync(req);
        }

        [HttpPost]
        [Route("stage/setClearStageBack")]
        public async Task<SetBackStageRes> SetStageClearBack([FromBody] SetBackStageReq req)
        {
            return await _stage.SetBackStageAsync(req);
        }

        [HttpPost]
        [Route("stage/setClearStageHardNormal")]
        public async Task<SetHardNormalStageRes> SetStageClearHardNormal([FromBody] SetHardNormalStageReq req)
        {
            return await _stage.SetHardNormalStageAsync(req);
        }

        [HttpPost]
        [Route("stage/getStageData")]
        public async Task<StageStartDataRes> GetStageData([FromBody] StageStartDataReq req)
        {
            return await _stage.StageDataGetAsync(req);
        }

        [HttpPost]
        [Route("stage/getStageReward")]
        public async Task<StageRewardRes> GetStageReward([FromBody] StageRewardReq req)
        {
            return await _stage.StageRewardGetAsync(req);
        }
    }
}