namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a board on which ships can be placed.
    /// </summary>
    public interface IFillableBoard
    {
        /// <summary>
        /// Tries to place a ship on a given location.
        /// </summary>
        /// <param name="ix">Coordinates where the ship should be placed</param>
        /// <param name="shipLength">Length of ship (max. 10)</param>
        /// <param name="direction">Direction of the ship</param>
        /// <returns>
        /// <c>true</c> if the ship could be placed, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown in case of invalid arguments</exception>
        bool TryPlaceShip(BoardIndex ix, int shipLength, Direction direction);
    }

    /// <summary>
    /// Represents an algorithm for placing ships on a board.
    /// </summary>
    public interface IBoardFiller
    {
        /// <summary>
        /// Fills board with given ships.
        /// </summary>
        /// <param name="shipLengths">Array of lengths of ships</param>
        /// <param name="board">Board to fill</param>
        /// <exception cref="BoardTooOccupiedException">Thrown if no position for all ships could have been found</exception>
        void Fill(IEnumerable<int> shipLengths, IFillableBoard board);
    }

    /// <summary>
    /// Places ships randomly on board.
    /// </summary>
    public class RandomBoardFiller : IBoardFiller
    {
        /// <inheritdoc/>
        public void Fill(IEnumerable<int> shipLengths, IFillableBoard board)
        {
            foreach (var shipLength in shipLengths)
            {
                PlaceShip(shipLength, board);
            }
        }

        private static void PlaceShip(int shipLength, IFillableBoard board)
        {
            // We try to randomly place ship 1000 times. If we cannot find a place,
            // we assume that there is not enough space for this ship on the board.
            var rand = new Random();
            for (var attemptsLeft = 1000; attemptsLeft > 0; attemptsLeft--)
            {
                var direction = rand.Next(2) == 0 ? Direction.Horizontal : Direction.Vertical;
                if (board.TryPlaceShip(new BoardIndex(
                    rand.Next(10 - (direction == Direction.Horizontal ? shipLength : 0)), 
                    rand.Next(10 - (direction == Direction.Vertical ? shipLength : 0))), 
                    shipLength, direction))
                {
                    // We found a spot
                    return;
                }
            }

            throw new BoardTooOccupiedException();
        }
    }
}
