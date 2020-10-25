namespace NBattleshipCodingContest.Logic
{
    using System;

    /// <summary>
    /// Factory for creating <see cref="SinglePlayerGame"/> instances
    /// </summary>
    public interface ISinglePlayerGameFactory
    {
        /// <summary>
        /// Create a <see cref="SinglePlayerGame"/> instance
        /// </summary>
        /// <param name="playerIndex">Index of player</param>
        /// <returns>
        /// Created <see cref="SinglePlayerGame"/> instance.
        /// </returns>
        SinglePlayerGame Create(int playerIndex) => Create(Guid.NewGuid(), playerIndex);

        /// <summary>
        /// Create a <see cref="SinglePlayerGame"/> instance
        /// </summary>
        /// <param name="gameId">ID of the game</param>
        /// <param name="playerIndex">Index of player</param>
        /// <returns>
        /// Created <see cref="SinglePlayerGame"/> instance.
        /// </returns>
        SinglePlayerGame Create(Guid gameId, int playerIndex);
    }
}
