namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public record SinglePlayerGameLogRecord(BoardIndex Location, SquareContent ShotResult);

    /// <summary>
    /// Represents a single-player Battleship game.
    /// </summary>
    /// <remarks>
    /// In real life, two human players play the Battleship game against each other. However, in our context
    /// we want to evaluate computer players. We want to find out how many shots they need in order to sink
    /// all ships. Therefore, there is no need for a second player. Games are simply played until the computer
    /// player has sunk all ships. By counting the number of required shots, we can find the computer player
    /// with the best strategy.
    /// </remarks>
    public class SinglePlayerGame
    {
        private readonly List<SinglePlayerGameLogRecord> log = new();
        private readonly IReadOnlyBoard board;
        private readonly BoardContent shootingBoard;

        /// <summary>
        /// Initializes a new instance of the <see cref="SinglePlayerGame"/> type.
        /// </summary>
        /// <param name="gameId">ID of the game</param>
        /// <param name="playerIndex">Index of the computer player</param>
        /// <param name="board">Board with ships on it. The computer player has to sink these ships.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="board"/> contains <see cref="SquareContent.Unknown"/> squares.
        /// </exception>
        public SinglePlayerGame(Guid gameId, int playerIndex, IReadOnlyBoard board)
        {
            if (board.Any(s => s == SquareContent.Unknown))
            {
                throw new ArgumentException("Board must not contain unknown squares", nameof(board));
            }

            GameId = gameId;
            PlayerIndex = playerIndex;
            this.board = board;
            shootingBoard = new BoardContent(SquareContent.Unknown); ;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SinglePlayerGame"/> type.
        /// </summary>
        /// <param name="gameId">ID of the game</param>
        /// <param name="playerIndex">Index of the computer player</param>
        /// <param name="board">Board with ships on it</param>
        /// <param name="shootingBoard">Boards with placed shots.</param>
        /// <remarks>
        /// This constructor is for writing test cases only. It can be used to start with a pre-filled shooting board
        /// containing not just <see cref="SquareContent.Unknown"/> squares.
        /// </remarks>
        internal SinglePlayerGame(Guid gameId, int playerIndex, IReadOnlyBoard board, BoardContent shootingBoard)
            : this(gameId, playerIndex, board) => this.shootingBoard = shootingBoard;

        /// <summary>
        /// Gets the game ID
        /// </summary>
        public Guid GameId { get; }

        /// <summary>
        /// Gets the player index
        /// </summary>
        public int PlayerIndex { get; }

        /// <summary>
        /// Gets the board with ships on it
        /// </summary>
        /// <remarks>
        /// The computer player has to shink those ships. Does not contain <see cref="SquareContent.Unknown"/>
        /// squares.
        /// </remarks>
        public IReadOnlyBoard Board => board;

        /// <summary>
        /// Gets the board with all the shots of the computer player
        /// </summary>
        /// <remarks>
        /// Typically contains some <see cref="SquareContent.Unknown"/> squares.
        /// </remarks>
        public IReadOnlyBoard ShootingBoard => shootingBoard;

        /// <summary>
        /// Gets the history of shots
        /// </summary>
        public IEnumerable<SinglePlayerGameLogRecord> Log => log.ToArray();

        /// <summary>
        /// Gets the last shot (if there is one)
        /// </summary>
        /// <returns></returns>
        public BoardIndex? LastShot => log.LastOrDefault()?.Location;

        /// <summary>
        /// Given player shoots at a given index
        /// </summary>
        /// <param name="ix">Square on which the player shoots</param>
        /// <returns>Square content after the shot.</returns>
        /// <remarks>
        /// If the shot hits a <see cref="SquareContent.Ship"/> square, the square
        /// turns into a <see cref="SquareContent.HitShip"/>. If all squares of a ship
        /// have been hit, the entire ship turns into <see cref="SquareContent.SunkenShip"/>.
        /// </remarks>
        public SquareContent Shoot(BoardIndex ix)
        {
            var content = Board[ix];
            shootingBoard[ix] = content;
            if (content == SquareContent.Ship)
            {
                // We have a hit
                content = shootingBoard[ix] = SquareContent.HitShip;

                // Check whether the hit sank the ship
                var shipResult = Board.TryFindShip(ix, out var shipRange);
                if (shipResult == ShipFindingResult.CompleteShip
                    && shipRange.All(ix => ShootingBoard[ix] == SquareContent.HitShip))
                {
                    // The hit sank the ship -> change all ship quares to SunkenShip
                    content = SquareContent.SunkenShip;
                    foreach (var shipIx in shipRange)
                    {
                        shootingBoard[shipIx] = SquareContent.SunkenShip;
                    }
                }
            }

            log.Add(new(ix, content));
            return content;
        }

        /// <summary>
        /// Builds a <see cref="ShotRequest"/> from a shot.
        /// </summary>
        /// <returns>
        /// Created <see cref="ShotRequest"/>
        /// </returns>
        public ShotRequest GetShotRequest() => new(GameId, PlayerIndex, ShootingBoard, LastShot);
    }
}
