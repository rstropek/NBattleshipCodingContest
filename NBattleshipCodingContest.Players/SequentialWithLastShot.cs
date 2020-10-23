namespace NBattleshipCodingContest.Players
{
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a battleship player that shoots at one cell after the other
    /// </summary>
    public class SequentialWithLastShot : PlayerBase
    {
        /// <inheritdoc />
        public override async Task GetShot(Guid gameId, IReadOnlyBoard board, Shoot shoot)
        {
            BoardIndex ix;
            if (LastShot == null)
            {
                // Player did not shoot before
                ix = new BoardIndex();
            }
            else
            {
                // Player did shoot before. Take location of last shot and increment it by one.
                ix = (BoardIndex)LastShot;
                ix++;
            }

            // Shoot at current square
            await shoot(ix);
        }
    }
}
