namespace NBattleshipCodingContest.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using NBattleshipCodingContest.Logic;
    using NBattleshipCodingContest.Players;
    using NBattleshipCodingContest.Protocol;

    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly IEnumerable<PlayerInfo> players;
        private readonly IBattleHostConnection battleHostConnection;

        public TournamentsController(PlayerInfo[] players, IBattleHostConnection battleHostConnection)
        {
            this.players = players;
            this.battleHostConnection = battleHostConnection;
        }

        [HttpPost]
        public async Task<IActionResult> Start()
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

            var (x, y) = Console.GetCursorPosition();

            battleHostConnection.StartGame(0, 1);
            while (battleHostConnection.Game != null && battleHostConnection.Game.GetWinner(BattleshipBoard.Ships) == Winner.NoWinner)
            {
                await battleHostConnection.Shoot(1);
                await battleHostConnection.Shoot(2);

                Console.SetCursorPosition(x, y);
                Console.WriteLine(PlayerList.Players[battleHostConnection.Game.PlayerIndexes[0]].Name);
                Console.WriteLine(battleHostConnection.Game.ShootingBoards[0]);
                Console.WriteLine(PlayerList.Players[battleHostConnection.Game.PlayerIndexes[1]].Name);
                Console.WriteLine(battleHostConnection.Game.ShootingBoards[1]);
            }

            return Ok();
        }
    }
}
