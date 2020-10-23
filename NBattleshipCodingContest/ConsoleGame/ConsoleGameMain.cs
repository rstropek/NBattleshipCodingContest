namespace NBattleshipCodingContest.ConsoleGame
{
    using NBattleshipCodingContest.Logic;
    using NBattleshipCodingContest.Players;
    using NBattleshipCodingContest.Protocol;
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
            while (game.GetWinner(BattleshipBoard.Ships) == Winner.NoWinner)
            {
                var p1 = PlayerList.Players[options.Player1Index].Create();
                var shotRequest = ProtocolTranslator.DecodeShotRequest(ProtocolTranslator.EncodeShotRequest(
                    new Logic.ShotRequest(game.GameId, options.Player1Index, 
                        game.ShootingBoards[0], game.GetLastShot(1))).ShotRequest);
                p1.LastShot = shotRequest.LastShot;
                await p1.GetShot(shotRequest.GameId, shotRequest.BoardShooterView,
                    location => Task.FromResult(game.Shoot(1, location)));

                var p2 = PlayerList.Players[options.Player2Index].Create();
                shotRequest = ProtocolTranslator.DecodeShotRequest(ProtocolTranslator.EncodeShotRequest(
                    new Logic.ShotRequest(game.GameId, options.Player2Index, 
                        game.ShootingBoards[1], game.GetLastShot(2))).ShotRequest);
                p2.LastShot = shotRequest.LastShot;
                await p2.GetShot(game.GameId, shotRequest.BoardShooterView,
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
