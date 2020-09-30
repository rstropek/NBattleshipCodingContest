namespace NBattleshipCodingContest.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using NBattleshipCodingContest.Players;
    using System.Collections.Generic;
    using System.Linq;

    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> GetPlayers() => PlayerList.Players.Select(p => p.Name);
    }
}
