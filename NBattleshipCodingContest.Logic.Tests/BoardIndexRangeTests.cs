namespace NBattleshipCodingContest.Logic.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class BoardIndexRangeTests
    {
        [Fact]
        public void Construction_Horizontal()
        {
            var bir = new BoardIndexRange("A1", "C1");
            Assert.Equal(new BoardIndex(0, 0), bir.From);
            Assert.Equal(new BoardIndex(2, 0), bir.To);
        }

        [Fact]
        public void Construction_Vertical()
        {
            var bir = new BoardIndexRange("A1", "A3");
            Assert.Equal(new BoardIndex(0, 0), bir.From);
            Assert.Equal(new BoardIndex(0, 2), bir.To);
        }

        [Fact]
        public void Construction_Switch()
        {
            var bir = new BoardIndexRange("C1", "A1");
            Assert.Equal(new BoardIndex(0, 0), bir.From);
            Assert.Equal(new BoardIndex(2, 0), bir.To);
        }

        [Fact] public void Construction_Error() => Assert.Throws<ArgumentException>(() => new BoardIndexRange("A1", "B2"));

        [Fact] public void Equal() => Assert.True(new BoardIndexRange("A1", "C1").Equals(new BoardIndexRange("A1", "C1")));
        
        [Fact] public void Equal_Object() => Assert.True(new BoardIndexRange("A1", "C1").Equals((object)new BoardIndexRange("A1", "C1")));

        [Fact] public void Hashcode() => Assert.Equal(new BoardIndexRange("A1", "C1").GetHashCode(), new BoardIndexRange("A1", "C1").GetHashCode());

        [Fact] public void Equal_Operator() => Assert.True(new BoardIndexRange("A1", "C1") == new BoardIndexRange("A1", "C1"));
        
        [Fact] public void Not_Equal_Operator() => Assert.True(new BoardIndexRange("A1", "C1") != new BoardIndexRange("A1", "D1"));

        [Fact] public void Length_Horizontal() => Assert.Equal(3, new BoardIndexRange("A1", "A3").Length);

        [Fact] public void Length_Vertical() => Assert.Equal(3, new BoardIndexRange("A1", "C1").Length);

        [Fact]
        public void Deconstruct()
        {
            var bir = new BoardIndexRange("A1", "A3");
            var (from, to) = bir;
            Assert.Equal("A1", from);
            Assert.Equal("A3", to);
        }

        [Fact]
        public void Enumerator_Horizontal()
        {
            var bir = new BoardIndexRange("A1", "A3");
            var ixs = bir.ToArray();
            Assert.Equal("A1", ixs[0]);
            Assert.Equal("A2", ixs[1]);
            Assert.Equal("A3", ixs[2]);
        }

        [Fact]
        public void Enumerator_Vertical()
        {
            var bir = new BoardIndexRange("A1", "C1");
            var ixs = bir.ToArray();
            Assert.Equal("A1", ixs[0]);
            Assert.Equal("B1", ixs[1]);
            Assert.Equal("C1", ixs[2]);
        }

    }
}
