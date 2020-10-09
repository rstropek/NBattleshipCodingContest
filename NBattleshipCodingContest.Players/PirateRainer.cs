namespace NBattleshipCodingContest.Players
{
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements Rainer's battleship player
    /// </summary>
    public class PirateRainer : PlayerBase
    {
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
        public override async Task GetShot(Guid _, string __, IReadOnlyBoard board, Shoot shoot)
        {
            var ix = new BoardIndex();

            // Find next unknown square
            while (!DoesItMakeSenseToShootHere(board, ix)) ix = ix.Next();

            // Shoot at first unknonwn square
            await shoot(ix);
        }
    }
}
