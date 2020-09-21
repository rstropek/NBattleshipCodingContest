namespace NBattleshipCodingContest.Logic.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class BattleshipBoardTests
    {
        [Fact] 
        public void PlaceShip_Horizontal()
        {
            var board = new BattleshipBoard();
            board.PlaceShip(new BoardIndex(0, 0), 2, Direction.Horizontal);
            Assert.Equal(SquareContent.Ship, board[new BoardIndex(0, 0)]);
            Assert.Equal(SquareContent.Ship, board[new BoardIndex(1, 0)]);
            Assert.Equal(2, board.Count(s => s == SquareContent.Ship));
        }

        [Fact]
        public void PlaceShip_Vertical()
        {
            var board = new BattleshipBoard();
            board.PlaceShip(new BoardIndex(0, 0), 2, Direction.Vertical);
            Assert.Equal(SquareContent.Ship, board[new BoardIndex(0, 0)]);
            Assert.Equal(SquareContent.Ship, board[new BoardIndex(0, 1)]);
            Assert.Equal(2, board.Count(s => s == SquareContent.Ship));
        }

        [Fact]
        public void Initialize()
        {
            var board = new BattleshipBoard();
            board.Initialize(new RandomBoardFiller());
            Assert.Equal(5 + 4 + 3 + 3 + 2, board.Count(s => s != SquareContent.Water));
        }

        [Fact]
        public void Count()
        {
            var board = new BattleshipBoard();
            Assert.Equal(100, board.Count);
        }

        [Fact]
        public void ReadOnlyList_Failure()
        {
            var board = new BattleshipBoard();
            Assert.Throws<ArgumentOutOfRangeException>(() => board[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => board[10 * 10]);
        }

        [Fact]
        public void ReadOnlyList()
        {
            var board = new BattleshipBoard();
            board.PlaceShip(new BoardIndex(0, 0), 1, Direction.Horizontal);
            Assert.Equal(SquareContent.Ship, board[0]);
            Assert.Equal(SquareContent.Water, board[99]);
        }
    }
}
