namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Diagnostics;

    public static class ReadOnlyBoardExtensions
    {
        /// <summary>
        /// Convert a battleship board into a compact string
        /// </summary>
        /// <param name="board">Board to convert</param>
        /// <returns>
        /// String in which each letter represents one board sqare.
        /// </returns>
        public static string ToShortString(this IReadOnlyBoard board) =>
            string.Create(100, board, (buf, content) =>
            {
                for (var i = 0; i < 100; i++)
                {
                    buf[i] = content[new BoardIndex(i)] switch
                    {
                        SquareContent.Water => 'W',
                        SquareContent.Ship => 'S',
                        SquareContent.HitShip => 'H',
                        SquareContent.Unknown => ' ',
                        _ => throw new InvalidOperationException("Invalid square content, should never happen!")
                    };
                }
            });

        internal static BoardIndexRange FindShip(this IReadOnlyBoard board, BoardIndex ix)
        {
            Debug.Assert(board[ix] is SquareContent.HitShip or SquareContent.Ship);

            BoardIndex FindShipEdge(BoardIndex current, Direction direction, bool prev)
            {
                bool canMoveFurther;
                do
                {
                    BoardIndex next;
                    if (prev) canMoveFurther = current.TryPrevious(direction, out next);
                    else canMoveFurther = current.TryNext(direction, out next);

                    if (canMoveFurther)
                    {
                        current = next;
                    }
                }
                while (canMoveFurther && board[current] is not SquareContent.Water and not SquareContent.Unknown);
                return board[current] is not SquareContent.Water and not SquareContent.Unknown ? current : 
                    (prev ? (direction == Direction.Horizontal ? current.PreviousColumn() : current.PreviousRow())
                          : (direction == Direction.Horizontal ? current.NextColumn() : current.NextRow()));
            }

            // Go left and find first water
            var horizontal = new BoardIndexRange(
                FindShipEdge(ix, Direction.Horizontal, true),
                FindShipEdge(ix, Direction.Horizontal, false));
            if (horizontal.Length > 1)
            {
                return horizontal;
            }

            return new BoardIndexRange(
                FindShipEdge(ix, Direction.Vertical, true),
                FindShipEdge(ix, Direction.Vertical, false));
        }

    }
}
