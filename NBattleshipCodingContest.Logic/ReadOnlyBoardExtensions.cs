namespace NBattleshipCodingContest.Logic
{
    using System;

    public enum ShipFindingResult
    {
        NoShip,
        CompleteShip,
        PartialShip
    }

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
                        SquareContent.SunkenShip => 'X',
                        SquareContent.Unknown => ' ',
                        _ => throw new InvalidOperationException("Invalid square content, should never happen!")
                    };
                }
            });

        public static ShipFindingResult TryFindShip(this IReadOnlyBoard board, BoardIndex ix, out BoardIndexRange shipRange)
        {
            if (board[ix] is not SquareContent.HitShip and not SquareContent.Ship)
            {
                // No ship at specified index
                shipRange = new BoardIndexRange();
                return ShipFindingResult.NoShip;
            }

            (BoardIndex ix, bool complete) FindShipEdge(BoardIndex current, Direction direction, bool prev)
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

                var complete = board[current] is not SquareContent.Unknown;
                if (board[current] is not SquareContent.Water and not SquareContent.Unknown)
                {
                    return (current, complete);
                }

                if (!prev)
                {
                    return (direction == Direction.Horizontal ? current.PreviousColumn() : current.PreviousRow(), complete);
                }

                return (direction == Direction.Horizontal ? current.NextColumn() : current.NextRow(), complete);
            }

            bool TryDirection(Direction direction, out (BoardIndexRange range, ShipFindingResult complete) result)
            {
                var (beginningIx, beginningComplete) = FindShipEdge(ix, direction, true);
                var (endIx, endComplete) = FindShipEdge(ix, direction, false);
                result = (new BoardIndexRange(beginningIx, endIx), 
                    beginningComplete && endComplete ? ShipFindingResult.CompleteShip : ShipFindingResult.PartialShip);
                return result.range.Length > 1;
            }

            // Go left and find first water
            if (TryDirection(Direction.Horizontal, out var resultHorizontal))
            {
                shipRange = resultHorizontal.range;
                return resultHorizontal.complete;
            }

            _ = TryDirection(Direction.Vertical, out var resultVertical);
            shipRange = resultVertical.range;
            return resultVertical.complete;
        }
    }
}
