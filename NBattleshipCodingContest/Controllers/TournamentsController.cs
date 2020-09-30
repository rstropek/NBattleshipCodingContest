namespace NBattleshipCodingContest.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NBattleshipCodingContest.Logic;
    using NBattleshipCodingContest.Players;
    using NBattleshipCodingContest.Protocol;

    public record Players(int Player1Index, int Player2Index);

    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly IEnumerable<PlayerInfo> players;
        private readonly IBattleHostConnection battleHostConnection;
        private readonly ILogger<TournamentsController> logger;

        public TournamentsController(PlayerInfo[] players, IBattleHostConnection battleHostConnection,
            ILogger<TournamentsController> logger)
        {
            this.players = players;
            this.battleHostConnection = battleHostConnection;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Start([FromBody] Players playerIndexes)
        {
            if (players.Count() < 2)
            {
                return BadRequest(new ProblemDetails
                {
                    Type = "Configuration error",
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Too few players",
                    Detail = "There have to be at least two players in order to start a tournament"
                });
            }

            if (playerIndexes.Player1Index < 0 || playerIndexes.Player1Index >= players.Count() ||
                playerIndexes.Player2Index < 0 || playerIndexes.Player2Index >= players.Count())
            {
                return BadRequest(new ProblemDetails
                {
                    Type = "Invalid index",
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Index out of bounds",
                    Detail = "Player index is out of bounds"
                });
            }

            if (!battleHostConnection.CanStartGame)
            {
                return BadRequest(new ProblemDetails
                {
                    Type = "Battle Host Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Connection to battle host does currently not allow new games.",
                    Detail = "Did you forget to connect?"
                });
            }

            battleHostConnection.StartGame(playerIndexes.Player1Index, playerIndexes.Player2Index);
            while (battleHostConnection.Game != null && battleHostConnection.Game.GetWinner(BattleshipBoard.Ships) == Winner.NoWinner)
            {
                await battleHostConnection.Shoot(1);
                await battleHostConnection.Shoot(2);
            }

            return Ok(battleHostConnection.Game!.Log);
        }
    }
}
