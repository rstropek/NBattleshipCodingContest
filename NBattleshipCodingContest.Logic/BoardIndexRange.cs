namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a range (e.g. A1-A3) on a board
    /// </summary>
    /// <remarks>
    /// A range consists of a starting board index and and end board index. Column or row index have to match
    /// so that the range has a width of 1. <see cref="BoardIndexRange"/> is used in conjunction with placing
    /// ships as it can nicely represent a ship position on a battleship board.
    /// </remarks>
    public readonly struct BoardIndexRange : IEquatable<BoardIndexRange>, IEnumerable, IEnumerable<BoardIndex>
    {
        private readonly BoardIndex from;
        private readonly BoardIndex to;

        /// <summary>
        /// Initializes a new <see cref="BoardIndexRange"/> instance.
        /// </summary>
        /// <param name="from">Starting point</param>
        /// <param name="to">End point</param>
        /// <remarks>
        /// If <paramref name="from"/> is greater than <paramref name="to"/>, from and to are switched
        /// so that <see cref="Length"/> is always positive.
        /// </remarks>
        public BoardIndexRange(BoardIndex from, BoardIndex to)
        {
            if (from.Column == to.Column)
            {
                this.from = to.Row > from.Row ? from : to;
                this.to = to.Row > from.Row ? to : from;
            } else if (from.Row == to.Row)
            {
                this.from = to.Column > from.Column ? from : to;
                this.to = to.Column > from.Column ? to : from;
            } else
            {
                throw new ArgumentException("Row or column or from and to have to match (e.g. width = 1)");
            }
        }

        /// <inheritdoc/>
        public readonly bool Equals(BoardIndexRange other) => other.from == from && other.to == to;

        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj != null && obj is BoardIndexRange r && Equals(r);

        /// <inheritdoc/>
        public readonly override int GetHashCode() => HashCode.Combine(from, to);

        /// <inheritdoc/>
        public static bool operator ==(BoardIndexRange left, BoardIndexRange right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(BoardIndexRange left, BoardIndexRange right) => !(left == right);

        /// <summary>
        /// Gets the length of the range
        /// </summary>
        /// <remarks>
        /// Length of A1-A3 would be 3.
        /// </remarks>
        public readonly int Length => (from.Column == to.Column ? to.Row - from.Row : to.Column - from.Column) + 1;

        /// <summary>
        /// Gets the starting point of the range
        /// </summary>
        public readonly BoardIndex From => from;

        /// <summary>
        /// Gets the end point of the range
        /// </summary>
        public readonly BoardIndex To => to;

        /// <summary>
        /// Deconstructs the instance
        /// </summary>
        /// <param name="from">Starting point</param>
        /// <param name="to">End point</param>
        public readonly void Deconstruct(out BoardIndex from, out BoardIndex to) => (from, to) = (this.from, this.to);

        /// <inheritdoc/>
        public readonly IEnumerator<BoardIndex> GetEnumerator()
        {
            var current = from;
            while (true)
            {
                yield return current;
                if (current == to) break;

                current = from.Column == to.Column ? current.NextRow() : current.NextColumn();
            }
        }

        /// <inheritdoc/>
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
