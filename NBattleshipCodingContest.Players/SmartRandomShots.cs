namespace NBattleshipCodingContest.Players
{
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a battleship player that randomly shoots at squares
    /// </summary>
    /// <remarks>
    /// This player is smarter than <see cref="RandomShots"/> because it does not
    /// shoot on squares that have already been shot at.
    /// </remarks>
    public class SmartRandomShots : PlayerBase
    {
        /// <inheritdoc />
        public override async Task GetShot(Guid _, IReadOnlyBoard board, Shoot shoot)
        {
            var rand = new Random();
            while (true)
            {
                var ix = new BoardIndex(rand.Next(10), rand.Next(10));
                if (board[ix] == SquareContent.Unknown)
                {
                    await shoot(ix);
                    break;
                }
            }
        }
    }
}
