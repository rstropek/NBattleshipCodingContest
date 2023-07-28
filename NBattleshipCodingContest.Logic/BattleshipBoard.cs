namespace NBattleshipCodingContest.Logic
{
    using System;

    /// <summary>
    /// Implements a board for a battleship game.
    /// </summary>
    /// <remarks>
    /// The rules of the classical Battleship game apply (see also https://en.wikipedia.org/wiki/Battleship_(game)).
    /// </remarks>
    public class BattleshipBoard : BoardContent, IFillableBoard
    {
        /// <summary>
        /// Get ships for classical battleship board
        /// </summary>
        public static readonly int[] Ships = new[] { 5, 4, 3, 3, 2 };

        internal void PlaceShip(BoardIndex ix, int shipLength, Direction direction)
        {
            for (var i = 0; i < shipLength; i++)
            {
                this[ix] = SquareContent.Ship;
                ix.TryNext(direction, out ix);
            }
        }

        /// <summary>
        /// Initializes the board by using the given filler.
        /// </summary>
        /// <param name="filler">Filler to use for filling the board</param>
        /// <remarks>
        /// The board (i.e., everything is set to water) is cleared before filling it.
        /// </remarks>
        public void Initialize(IBoardFiller filler)
        {
            Clear(SquareContent.Water);
            filler.Fill(Ships, this);
        }

        /// <inheritdoc/>
        public bool TryPlaceShip(BoardIndex ix, int shipLength, Direction direction)
        {
            if (!CanPlaceShip(ix, shipLength, direction, (c, r) => this[new BoardIndex(c, r)] == SquareContent.Water))
            {
                return false;
            }

            PlaceShip(ix, shipLength, direction);
            return true;
        }

        /// <summary>
        /// Checks if a given square on the battleship board is water.
        /// </summary>
        /// <param name="col">Zero-based column index</param>
        /// <param name="row">Zero-based row index</param>
        /// <returns>
        /// <c>true</c> if the given square is water, otherwise <c>false</c>.
        /// </returns>
        internal delegate bool IsWater(int col, int row);

        /// <summary>
        /// Finds out if a ship can be places at given coordinates.
        /// </summary>
        /// <param name="ix">Coordinates where the ship should be placed</param>
        /// <param name="shipLength">Length of the ship (max. 10)</param>
        /// <param name="direction">Direction of the ship</param>
        /// <param name="isWater">Callback to find out if a given square is water before placing the ship</param>
        /// <returns>
        /// <c>true</c> if the ship can be placed here, otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Each ship occupies a number of consecutive squares on the battleship board, arranged either horizontally 
        /// or vertically. The ships cannot overlap (i.e., only one ship can occupy any given square on the board).
        /// There has to be at least one square with water between each ship (i.e., ships must not be place
        /// directly adjacent to each other).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown in case of invalid argument</exception>
        internal static bool CanPlaceShip(BoardIndex ix, int shipLength, Direction direction, IsWater isWater)
        {
            #region Check input parameter
            // This is a public method, so we have to check that parameters
            // contain valid values.

            // Check if ship isn't too long
            if (shipLength > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(shipLength), "Maximum length of ship is 10");
            }

            if (!Enum.IsDefined(typeof(Direction), direction))
            {
                throw new ArgumentOutOfRangeException(nameof(direction), "Unknown direction");
            }
            #endregion

            // Note static local functions. They cannot unintentionally capture state. Read more at
            // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/local-functions

            // Check if ship is placed outside bounds of board
            static bool OutsideBounds(int start, int shipLength) => start + shipLength > 10;
            if (OutsideBounds(direction == Direction.Horizontal ? ix.Column : ix.Row, shipLength))
            {
                return false;
            }

            // Helper methods for index calculations
            // Note static local functions
            static int GetFirst(int index) => index == 0 ? index : index - 1;
            static int GetElementsToCheckAcross(int index) => (index == 0 || index == 10 - 1) ? 2 : 3;
            static int GetElementsToCheckAlongside(int index, int shipLength) =>
                shipLength + ((index == 0 || index + shipLength == 10) ? 1 : 2);

            var numberOfRowsToCheck = direction == Direction.Horizontal
                ? GetElementsToCheckAcross(ix.Row)
                : GetElementsToCheckAlongside(ix.Row, shipLength);
            var numberOfColsToCheck = direction == Direction.Horizontal
                ? GetElementsToCheckAlongside(ix.Column, shipLength)
                : GetElementsToCheckAcross(ix.Column);

            var firstCheckRow = GetFirst(ix.Row);
            var firstCheckCol = GetFirst(ix.Column);

            // Check if ships overlap
            for (var r = firstCheckRow; r < firstCheckRow + numberOfRowsToCheck; r++)
            {
                for (var c = firstCheckCol; c < firstCheckCol + numberOfColsToCheck; c++)
                {
                    if (!isWater(c, r))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
