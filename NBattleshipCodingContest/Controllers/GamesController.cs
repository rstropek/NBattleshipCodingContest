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

    public record GameResult(string Board1, string Board2, Winner Winner, IEnumerable<GameLogRecord> Log);

    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly IEnumerable<PlayerInfo> players;
        private readonly IPlayerHostConnection playerHostConnection;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger<GamesController> logger;
#pragma warning restore IDE0052 // Remove unread private members

        public GamesController(PlayerInfo[] players, IPlayerHostConnection playerHostConnection,
            ILogger<GamesController> logger)
        {
            this.players = players;
            this.playerHostConnection = playerHostConnection;
            this.logger = logger;
        }

        [HttpPost]
        [Route("start")]
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

            if (!playerHostConnection.CanStartGame)
            {
                return BadRequest(new ProblemDetails
                {
                    Type = "Battle Host Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Connection to battle host does currently not allow new games.",
                    Detail = "Did you forget to connect?"
                });
            }

            playerHostConnection.StartGame(playerIndexes.Player1Index, playerIndexes.Player2Index);
            while (playerHostConnection.Game != null && playerHostConnection.Game.GetWinner(BattleshipBoard.Ships) == Winner.NoWinner)
            {
                await playerHostConnection.Shoot(1);
                await playerHostConnection.Shoot(2);
            }

            var game = playerHostConnection.Game!;
            var result = new GameResult(
                game.Boards[0].ToShortString(),
                game.Boards[1].ToShortString(),
                game.GetWinner(BattleshipBoard.Ships),
                playerHostConnection.Game!.Log);
            return Ok(result);
        }
    }
}
