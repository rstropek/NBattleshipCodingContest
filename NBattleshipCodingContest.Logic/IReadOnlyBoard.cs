namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Enables read-only access to battleship board content
    /// </summary>
    public interface IReadOnlyBoard : IEnumerable<SquareContent>, IReadOnlyList<SquareContent>
    {
        /// <summary>
        /// Gets or sets the content on a given board square.
        /// </summary>
        /// <param name="ix">Index of the square</param>
        /// <returns>Content of the given square</returns>
        SquareContent this[BoardIndex ix] { get; }

        /// <summary>
        /// Indicates if the player of this board has lost.
        /// </summary>
        /// <param name="ships">Ships that were placed on the board</param>
        /// <remarks>
        /// Lost means that all ship squares were hit by shots
        /// </remarks>
        bool HasLost(params int[] ships);

        string ToShortString() =>
            string.Create(100, this, (buf, content) =>
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
    }
}
