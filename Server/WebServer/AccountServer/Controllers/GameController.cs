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

        public GameController(PlayerService player)
        {
            _player = player;
        }

        [HttpPost]
        [Route("player")]
        public async Task<PlayerPacketRes> PlayerData([FromBody] PlayerPacketReq req)
        {
            return await _player.LoadOrCreatePlayerAsync(req.jwt);
        }
    }
}
