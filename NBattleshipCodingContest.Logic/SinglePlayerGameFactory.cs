namespace NBattleshipCodingContest.Logic
{
    using System;

    /// <inheritdoc />
    public class SinglePlayerGameFactory : ISinglePlayerGameFactory
    {
        private readonly IBoardFiller filler;

        /// <inheritdoc />
        public SinglePlayerGameFactory(IBoardFiller filler) => this.filler = filler;

        /// <inheritdoc />
        public SinglePlayerGame Create(Guid gameId, int playerIndex)
        {
            var board = new BattleshipBoard();
            filler.Fill(BattleshipBoard.Ships, board);
            return new SinglePlayerGame(gameId, playerIndex, board, new BoardContent(SquareContent.Unknown));
        }
    }
}
