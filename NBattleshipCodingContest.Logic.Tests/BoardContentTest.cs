namespace NBattleshipCodingContest.Logic.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class BoardContentTest
    {
        [Fact]
        public void Parameterless_Constructor()
        {
            var bc = new BoardContent();
            Assert.Empty(bc.Where(c => c != SquareContent.Water));
        }

        [Fact]
        public void Initializing_Constructor()
        {
            var bc = new BoardContent(SquareContent.Unknown);
            Assert.Empty(bc.Where(c => c != SquareContent.Unknown));
        }

        [Fact]
        public void From_Byte_Array()
        {
            var bc = new BoardContent(Enumerable.Range(0, 100).Select(n => (byte)(n % 4)));
            Assert.Equal(25, bc.Count(c => c == SquareContent.Ship));
        }

        [Fact]
        public void From_Byte_Array_Too_Short() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardContent(new byte[] { 0 }));

        [Fact]
        public void From_Byte_Array_Invalid() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardContent(new [] { (byte)(SquareContent.Unknown + 1) }));

        [Fact]
        public void Initializing_Constructor_Invalid() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardContent(SquareContent.Unknown + 1));

        [Fact]
        public void Content_To_String()
        {
            var bc = new BoardContent();
            bc.Clear(SquareContent.Unknown);
            bc[new BoardIndex(0, 0)] = SquareContent.Ship;
            bc[new BoardIndex(1, 0)] = SquareContent.HitShip;
            bc[new BoardIndex(2, 0)] = SquareContent.Water;

            var output = bc.ToString();
            Assert.Equal((1 + 10 + 9 + 1) * (1 + 10 * 3 + 1), output.Length);
            Assert.Equal(2, output.Count(c => c == '█'));
            Assert.Equal(2, output.Count(c => c == 'X'));
            Assert.Equal(2, output.Count(c => c == '~'));
            Assert.Equal(97 * 2, output.Count(c => c == ' '));
        }

        [Fact]
        public void HasLost()
        {
            var board = new BoardContent();
            board[0] = SquareContent.HitShip;
            Assert.True(board.HasLost(1));
        }
    }
}
