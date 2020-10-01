namespace NBattleshipCodingContest.ConsoleGame
{
    using NBattleshipCodingContest.Logic;
    using NBattleshipCodingContest.Players;
    using Serilog;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    internal class ConsoleGameMain
    {
        [SuppressMessage("Microsoft.Performance", "CA1822", Scope = "method", Justification = "Static method could not act as context for logging")]
        public async Task StartConsoleGame(ConsoleGameOptions options)
        {
            var logger = Log.Logger.ForContext<ConsoleGameMain>();

            if (options.Player1Index < 0 || options.Player1Index >= PlayerList.Players.Length)
            {
                logger.Error("Player 1 index is invalid. Must be between 1 and {NumberOfPlayers}", PlayerList.Players.Length - 1);
            }

            if (options.Player2Index < 0 || options.Player2Index >= PlayerList.Players.Length)
            {
                logger.Error("Player 2 index is invalid. Must be between 1 and {NumberOfPlayers}", PlayerList.Players.Length - 1);
            }

            var gf = new GameFactory(new RandomBoardFiller());
            var game = gf.Create(options.Player1Index, options.Player2Index);
            var p1 = PlayerList.Players[options.Player1Index].Create();
            var p2 = PlayerList.Players[options.Player2Index].Create();
            while (game.GetWinner(BattleshipBoard.Ships) == Winner.NoWinner)
            {
                await p1.GetShot(game.GameId, PlayerList.Players[options.Player2Index].Name, game.ShootingBoards[0],
                    location => Task.FromResult(game.Shoot(1, location)));
                await p2.GetShot(game.GameId, PlayerList.Players[options.Player1Index].Name, game.ShootingBoards[1],
                    location => Task.FromResult(game.Shoot(2, location)));
            }

            Console.WriteLine(game.GetWinner(BattleshipBoard.Ships) switch
            {
                Winner.Draw => "We have a draw!",
                Winner.Player1 => "Player 1 is the winner",
                Winner.Player2 => "Player 2 is the winner",
                _ => "No winner? this should never happen"
            });

            Console.WriteLine("Shooting board of player 1");
            Console.WriteLine(game.ShootingBoards[0]);

            Console.WriteLine("Shooting board of player 2");
            Console.WriteLine(game.ShootingBoards[1]);
        }
    }
}
