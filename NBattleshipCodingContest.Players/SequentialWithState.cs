namespace NBattleshipCodingContest.Players
{
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a battleship player that shoots at one cell after the other
    /// </summary>
    public class SequentialWithState : PlayerBase
    {
        // The system create a new instance of the player for each shot. Therefore,
        // we need to store state in a static dictionary. Players do not run in parallel.
        // Therefore, it is not necessary to synchronize access to the dictionary with locking.
        private static readonly Dictionary<Guid, BoardIndex> indexes = new Dictionary<Guid, BoardIndex>();

        /// <inheritdoc />
        public override async Task GetShot(Guid gameId, string __, IReadOnlyBoard board, Shoot shoot)
        {
            if (!indexes.TryGetValue(gameId, out BoardIndex ix))
            {
                ix = indexes[gameId] = new BoardIndex();
            }

            // Shoot at first current square
            await shoot(ix);
            indexes[gameId]++;
        }
    }
}
