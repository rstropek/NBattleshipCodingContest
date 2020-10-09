namespace NBattleshipCodingContest.Players
{
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements Rainer's battleship player
    /// </summary>
    public class ScatterPirate : PlayerBase
    {
        private static readonly Dictionary<Guid, int> indexes = new Dictionary<Guid, int>();

        private static bool DoesItMakeSenseToShootHere(IReadOnlyBoard board, BoardIndex ix)
        {
            // Alvin Optimization ;-)
            //var isHitAbove = ix.Row > 0 && board[new BoardIndex(ix.Column, ix.Row - 1)] is SquareContent.HitShip or SquareContent.SunkenShip;
            //if (isHitAbove)

            if (ix.Row > 0)
            {
                var isHitAboveLeft = ix.Column > 0 && board[new BoardIndex(ix.Column - 1, ix.Row - 1)] is SquareContent.HitShip or SquareContent.SunkenShip;
                var isHitAboveRight = ix.Column < 9 && board[new BoardIndex(ix.Column + 1, ix.Row - 1)] is SquareContent.HitShip or SquareContent.SunkenShip;
                if (isHitAboveLeft || isHitAboveRight)
                {
                    return false;
                }
            }

            return board[ix] == SquareContent.Unknown;
        }

        /// <inheritdoc />
        public override async Task GetShot(Guid gameId, string __, IReadOnlyBoard board, Shoot shoot)
        {
            if (!indexes.TryGetValue(gameId, out int ix))
            {
                ix = -1;
            }

            if (ix == -1)
            {
                await shoot(new BoardIndex(0));
                indexes[gameId] = 0;
                return;
            }

            var lastShot = new BoardIndex(ix);
            var lastResult = board[lastShot];
            if (lastResult is SquareContent.HitShip)
            {
                if (lastShot.Row > 0 && board[lastShot.PreviousRow()] == SquareContent.Unknown
                    && DoesItMakeSenseToShootHere(board, lastShot.PreviousRow()))
                {
                    await shoot(lastShot.PreviousRow());
                    return;
                }

                if (lastShot.Column > 0 && board[lastShot.PreviousColumn()] == SquareContent.Unknown
                    && DoesItMakeSenseToShootHere(board, lastShot.PreviousColumn()))
                {
                    await shoot(lastShot.PreviousColumn());
                    return;
                }

                if (lastShot.Column < 9 && board[lastShot.NextColumn()] == SquareContent.Unknown
                    && DoesItMakeSenseToShootHere(board, lastShot.NextColumn()))
                {
                    await shoot(lastShot.NextColumn());
                    return;
                }

                if (lastShot.Row < 9 && board[lastShot.NextRow()] == SquareContent.Unknown
                    && DoesItMakeSenseToShootHere(board, lastShot.NextRow()))
                {
                    await shoot(lastShot.NextRow());
                    return;
                }
            }

            var row = lastShot.Row;
            var column = lastShot.Column;
            do
            {
                column += 2;
                if (column > 9)
                {
                    if (++row > 9) row = 0;
                    column = (row % 2 == 0) ? 0 : 1;
                }
            }
            while (!DoesItMakeSenseToShootHere(board, new BoardIndex(column, row)));

            await shoot(new BoardIndex(column, row));
            
            indexes[gameId] = new BoardIndex(column, row);
        }
    }
}
