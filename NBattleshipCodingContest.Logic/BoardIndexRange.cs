namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public readonly struct BoardIndexRange : IEquatable<BoardIndexRange>, IEnumerable, IEnumerable<BoardIndex>
    {
        private readonly BoardIndex from;
        private readonly BoardIndex to;

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

        public readonly bool Equals(BoardIndexRange other) => other.from == from && other.to == to;

        public readonly override bool Equals(object? obj) => obj != null && obj is BoardIndexRange r && Equals(r);

        public readonly override int GetHashCode() => HashCode.Combine(from, to);

        public static bool operator ==(BoardIndexRange left, BoardIndexRange right) => left.Equals(right);

        public static bool operator !=(BoardIndexRange left, BoardIndexRange right) => !(left == right);

        public readonly int Length => (from.Column == to.Column ? to.Row - from.Row : to.Column - from.Column) + 1;

        public readonly BoardIndex From => from;

        public readonly BoardIndex To => to;

        public readonly void Deconstruct(out BoardIndex from, out BoardIndex to) => (from, to) = (this.from, this.to);

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

        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
