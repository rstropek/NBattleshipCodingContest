namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public enum Winner
    {
        NoWinner = -1,
        Draw = 0,
        Player1 = 1,
        Player2 = 2
    }

    public record GameLogRecord(int Shooter, BoardIndex Location, SquareContent ShotResult, string? ShooterName = null);

    // Note use of records here. Read more at
    // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#records

    // Note limitations regarding XML documentation of records.

    /// <summary>
    /// Represents the state of a game between two players
    /// </summary>
    public class Game
    {
        private readonly SinglePlayerGame[] games;

        public Game(Guid gameId, SinglePlayerGame[] games)
        {
            if (games.Length != 2)
            {
                throw new ArgumentException("Has to be exactly two games", nameof(games));
            }

            GameId = gameId;
            this.games = games;
        }

        public Guid GameId { get; }

        public IReadOnlyList<IReadOnlyBoard> ShootingBoards => games.Select(g => g.ShootingBoard).ToArray();

        public IReadOnlyList<IReadOnlyBoard> Boards => games.Select(g => g.Board).ToArray();

        private static void EnsureValidShooter(int shootingPlayer)
        {
            if (shootingPlayer is < 1 or > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(shootingPlayer), "Player must be 1 or 2");
            }
        }

        private void EnsureValidBoards()
        {
            if (games.Length != 2)
            {
                throw new InvalidOperationException("Games array(s) of wrong length");
            }
        }

        /// <summary>
        /// Gets the history of shots
        /// </summary>
        public IEnumerable<GameLogRecord> Log
        {
            get
            {
                EnsureValidBoards();
                var game1Log = games[0].Log.ToArray();
                var game2Log = games[1].Log.ToArray();

                for (var i = 0; i < Math.Max(game1Log.Length, game2Log.Length); i++)
                {
                    if (i < game1Log.Length)
                    {
                        yield return new(games[0].PlayerIndex, game1Log[i].Location, game1Log[i].ShotResult);
                    }

                    if (i < game2Log.Length)
                    {
                        yield return new(games[1].PlayerIndex, game2Log[i].Location, game2Log[i].ShotResult);
                    }
                }
            }
        }

        /// <summary>
        /// Given player shoots at a given index
        /// </summary>
        /// <param name="shootingPlayer">Player who is shooting (1 or 2)</param>
        /// <param name="ix">Square on which the player shoots</param>
        /// <returns>Square content after the shot.</returns>
        /// <remarks>
        /// If the shot hits a <see cref="SquareContent.Ship"/> square, the square
        /// turns into a <see cref="SquareContent.HitShip"/>. 
        /// </remarks>
        public SquareContent Shoot(int shootingPlayer, BoardIndex ix)
        {
            EnsureValidShooter(shootingPlayer);
            EnsureValidBoards();

            return games[shootingPlayer - 1].Shoot(ix);
        }

        /// <summary>
        /// Builds a <see cref="ShotRequest"/> from a shot.
        /// </summary>
        /// <param name="shootingPlayer"></param>
        /// <returns></returns>
        public ShotRequest GetShotRequest(int shootingPlayer)
        {
            EnsureValidShooter(shootingPlayer);
            EnsureValidBoards();

            return new(GameId, games[shootingPlayer - 1].PlayerIndex, games[shootingPlayer - 1].ShootingBoard, GetLastShot(shootingPlayer));
        }

        public BoardIndex? GetLastShot(int player)
        {
            EnsureValidShooter(player);
            EnsureValidBoards();

            return games[player - 1].LastShot;
        }

        /// <summary>
        /// Determine winner
        /// </summary>
        /// <returns>
        /// Current winner of the game.
        /// </returns>
        public Winner GetWinner(params int[] ships)
        {
            EnsureValidBoards();
            var lostStates = games.Select(g => g.ShootingBoard.HasLost(ships)).ToArray();

            // A game is considered a draw if more than 200 moves were made
            if ((lostStates[0] && lostStates[1]) || Log.Count() >= 200)
            {
                return Winner.Draw;
            }

            if (lostStates[0])
            {
                return Winner.Player1;
            }

            if (lostStates[1])
            {
                return Winner.Player2;
            }

            return Winner.NoWinner;
        }
    }

    /// <summary>
    /// Factory for <see cref="Game"/> instances.
    /// </summary>
    public interface IGameFactory
    {
        /// <summary>
        /// Create a <see cref="Game"/> instance.
        /// </summary>
        /// <param name="player1Index">Index of player 1</param>
        /// <param name="player2Index">Index of player 2</param>
        /// <returns>
        /// New game.
        /// </returns>
        Game Create(int player1Index, int player2Index);
    }

    /// <summary>
    /// Factory for <see cref="Game"/> instances.
    /// </summary>
    /// <remarks>
    /// The reason for this factory is that it needs a <see cref="IBoardFiller"/>
    /// from dependency injection.
    /// </remarks>
    public class GameFactory : IGameFactory
    {
        private readonly ISinglePlayerGameFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameFactory"/> type.
        /// </summary>
        /// <param name="filler">Filler used to fill the game board.</param>
        public GameFactory(ISinglePlayerGameFactory factory)
        {
            this.factory = factory;
        }

        /// <inheritdoc/>
        public Game Create(int player1Index, int player2Index)
        {
            var gameId = Guid.NewGuid();
            return new Game(gameId, new[] { factory.Create(gameId, player1Index), factory.Create(gameId, player2Index) });
        }
    }
}
