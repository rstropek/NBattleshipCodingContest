namespace NBattleshipCodingContest.Logic.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ReadOnlyBoardExtensionsTests
    {
        [Fact]
        public void ToShortString()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = SquareContent.Ship;
            board[1] = SquareContent.Unknown;
            var result = board.ToShortString();
            Assert.Equal("S ", result[0..2]);
            Assert.Empty(result[2..].Where(c => c != 'W'));
        }

        [Fact]
        public void FindShipHorizontal()
        {
            for (var col = 0; col < 7; col++)
            {
                for (var row = 0; row < 10; row++)
                {
                    var board = new BoardContent(SquareContent.Water);
                    var locations = new List<BoardIndex>
                    {
                        new BoardIndex(col, row),
                        new BoardIndex(col + 1, row),
                        new BoardIndex(col + 2, row),
                    };
                    locations.ForEach(l => board[l] = SquareContent.Ship);
                    var shipLengths = locations.Select(l => board.FindShipAtPosition(l));
                    Assert.Empty(shipLengths.Where(s => s.Length != 3));
                }
            }
        }

        [Fact]
        public void FindShipVertical()
        {
            for (var col = 0; col < 10; col++)
            {
                for (var row = 0; row < 7; row++)
                {
                    var board = new BoardContent(SquareContent.Water);
                    var locations = new List<BoardIndex>
                    {
                        new BoardIndex(col, row),
                        new BoardIndex(col, row + 1),
                        new BoardIndex(col, row + 2),
                    };
                    locations.ForEach(l => board[l] = SquareContent.Ship);
                    var shipLengths = locations.Select(l => board.FindShipAtPosition(l));
                    Assert.Empty(shipLengths.Where(s => s.Length != 3));
                }
            }
        }

        [Fact]
        public void FindShip_Simple()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = board[1] = board[2] = SquareContent.Ship;
            var ship = board.FindShipAtPosition(new BoardIndex(0));
            Assert.Equal(3, ship.Length);
        }
    }
}
