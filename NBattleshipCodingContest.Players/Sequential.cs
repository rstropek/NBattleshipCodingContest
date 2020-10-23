namespace NBattleshipCodingContest.Players
{
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a battleship player that shoots at one cell after the other
    /// </summary>
    public class Sequential : PlayerBase
    {
        /// <inheritdoc />
        public override async Task GetShot(Guid _, IReadOnlyBoard board, Shoot shoot)
        {
            var ix = new BoardIndex();

            // Find next unknown square
            while (board[ix] != SquareContent.Unknown) ix = ix.Next();

            // Shoot at first unknonwn square
            await shoot(ix);
        }
    }
}
