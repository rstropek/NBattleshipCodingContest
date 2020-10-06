namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public enum Winner
    {
        NoWinner = -1,
        Draw = 0,
        Player1 = 1,
        Player2 = 2
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public record GameLogRecord(int Shooter, BoardIndex Location, SquareContent ShotResult, string? ShooterName = null);
#pragma warning restore IDE0060 // Remove unused parameter

    // Note use of records here. Read more at
    // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#records

    // Note limitations regarding XML documentation of records.

    /// <summary>
    /// Represents the state of a game between two players
    /// </summary>
    public record Game(Guid GameId, int[] PlayerIndexes, IReadOnlyBoard[] Boards, BoardContent[] ShootingBoards)
    {
        private readonly IList<GameLogRecord> log = new List<GameLogRecord>();

        private static void EnsureValidShooter(int shootingPlayer)
        {
            if (shootingPlayer is < 1 or > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(shootingPlayer), "Player must be 1 or 2");
            }
        }

        private void EnsureValidBoards()
        {
            if (Boards.Length != 2 || ShootingBoards.Length != 2)
            {
                throw new InvalidOperationException("Board array(s) of wrong length");
            }
        }

        /// <summary>
        /// Gets the history of shots
        /// </summary>
        public IEnumerable<GameLogRecord> Log => log.ToArray();

        internal static bool IsShipSunken(IReadOnlyBoard board, BoardIndex ix)
        {
            Debug.Assert(board[ix] == SquareContent.HitShip);

            bool FindShipEdge(ref BoardIndex current, Direction direction, bool prev)
            {
                bool canMoveFurther;
                do
                {
                    if (board[current] == SquareContent.Ship)
                    {
                        // We found a square that is a ship that has not been hit -> ship cannot be sunken
                        return false;
                    }

                    BoardIndex next;
                    if (prev) canMoveFurther = current.TryPrevious(direction, out next);
                    else canMoveFurther = current.TryNext(direction, out next);

                    if (canMoveFurther)
                    {
                        current = next;
                    }
                }
                while (canMoveFurther && board[current] != SquareContent.Water);

                return true;
            }

            // Go left and find first water
            BoardIndex current = ix;
            if (!FindShipEdge(ref current, Direction.Horizontal, true)) return false;
            BoardIndex leftStart = board[current] != SquareContent.Water ? current : current.NextColumn();

            // Go right and first first water
            current = ix;
            if (!FindShipEdge(ref current, Direction.Horizontal, false)) return false;
            BoardIndex rightEnd = board[current] != SquareContent.Water ? current : current.PreviousColumn();

            // Check whether we have a horizontal ship
            if (leftStart.Column < rightEnd.Column)
            {
                // We have found a horizontal ship and all its squares are hit -> it is sunken
                return true;
            }

            // We have a vertical ship, go up
            current = ix;
            if (!FindShipEdge(ref current, Direction.Vertical, true)) return false;

            // Go down
            current = ix;
            if (!FindShipEdge(ref current, Direction.Vertical, false)) return false;

            // We have found a vertical ship and all its squares are hit -> it is sunken
            return true;
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

            var board = Boards[shootingPlayer % 2];
            var content = board[ix];
            if (content == SquareContent.Ship)
            {
                content = SquareContent.HitShip;
                //if (IsShipSunken(board, ix))
                //{
                //    content = SquareContent.SunkenShip;
                //}
            }

            ShootingBoards[shootingPlayer - 1][ix] = content;
            log.Add(new(PlayerIndexes[shootingPlayer - 1], ix, content));
            return content;
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

            return new(GameId, PlayerIndexes[shootingPlayer - 1],
                PlayerIndexes[shootingPlayer % 2], ShootingBoards[shootingPlayer - 1]);
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
            var lostStates = ShootingBoards.Select(b => b.HasLost(ships)).ToArray();

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
        private readonly IBoardFiller filler;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameFactory"/> type.
        /// </summary>
        /// <param name="filler">Filler used to fill the game board.</param>
        public GameFactory(IBoardFiller filler)
        {
            this.filler = filler;
        }

        /// <inheritdoc/>
        public Game Create(int player1Index, int player2Index)
        {
            var boards = new BattleshipBoard[2];
            for (var i = 0; i < 2; i++)
            {
                boards[i] = new BattleshipBoard();
                filler.Fill(BattleshipBoard.Ships, boards[i]);
            }

            return new Game(Guid.NewGuid(), new[] { player1Index, player2Index }, boards,
                new[] { new BoardContent(SquareContent.Unknown), new BoardContent(SquareContent.Unknown) });
        }
    }
}
