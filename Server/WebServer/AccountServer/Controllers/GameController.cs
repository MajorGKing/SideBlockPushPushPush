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
        HeroService _heroService;

        public GameController(PlayerService player, CurrencyService currency, HeroService heroService)
        {
            _player = player;
            _currency = currency;
            _heroService = heroService;
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
            return await _heroService.GetHeroListAsync(req);
        }
    }
}
