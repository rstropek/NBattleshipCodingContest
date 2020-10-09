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
            board[2] = SquareContent.SunkenShip;
            board[3] = SquareContent.HitShip;
            var result = board.ToShortString();
            Assert.Equal("S XH", result[0..4]);
            Assert.Empty(result[4..].Where(c => c != 'W'));
        }

        [Theory]
        [InlineData(SquareContent.Water, ShipFindingResult.CompleteShip)]
        [InlineData(SquareContent.Unknown, ShipFindingResult.PartialShip)]
        public void FindShip_Horizontal(SquareContent initialContent, ShipFindingResult result)
        {
            for (var col = 0; col < 7; col++)
            {
                for (var row = 0; row < 10; row++)
                {
                    var board = new BoardContent(initialContent);
                    var locations = new List<BoardIndex>
                    {
                        new BoardIndex(col, row),
                        new BoardIndex(col + 1, row),
                        new BoardIndex(col + 2, row),
                    };
                    locations.ForEach(l => board[l] = SquareContent.Ship);
                    locations.ForEach(l =>
                    {
                        Assert.Equal(result, board.TryFindShip(l, out var ship));
                        Assert.Equal(3, ship.Length);
                    });
                }
            }
        }

        [Theory]
        [InlineData(SquareContent.Water, ShipFindingResult.CompleteShip)]
        [InlineData(SquareContent.Unknown, ShipFindingResult.PartialShip)]
        public void FindShip_Complete_Vertical(SquareContent initialContent, ShipFindingResult result)
        {
            for (var col = 0; col < 10; col++)
            {
                for (var row = 0; row < 7; row++)
                {
                    var board = new BoardContent(initialContent);
                    var locations = new List<BoardIndex>
                    {
                        new BoardIndex(col, row),
                        new BoardIndex(col, row + 1),
                        new BoardIndex(col, row + 2),
                    };
                    locations.ForEach(l => board[l] = SquareContent.Ship);
                    locations.ForEach(l =>
                    {
                        Assert.Equal(result, board.TryFindShip(l, out var ship));
                        Assert.Equal(3, ship.Length);
                    });
                }
            }
        }

        [Fact]
        public void FindShip_Complete_Simple()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = board[1] = board[2] = SquareContent.Ship;
            Assert.Equal(ShipFindingResult.CompleteShip, board.TryFindShip(new BoardIndex(0), out BoardIndexRange ship));
            Assert.Equal(3, ship.Length);
        }

        [Fact]
        public void FindShip_Incomplete_Simple()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = board[1] = SquareContent.Ship;
            board[2] = SquareContent.Unknown;
            Assert.Equal(ShipFindingResult.PartialShip, board.TryFindShip(new BoardIndex(0), out BoardIndexRange ship));
            Assert.Equal(2, ship.Length);
        }
    }
}
