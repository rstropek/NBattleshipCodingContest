namespace NBattleshipCodingContest.Logic.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class SinglePlayerGameTests
    {
        [Fact]
        public void Board_with_Unknown()
        {
            var board = new BoardContent(SquareContent.Unknown);
            Assert.Throws<ArgumentException>(() => new SinglePlayerGame(Guid.Empty, 4711, board));
        }

        [Fact]
        public void Shoot_Into_Water()
        {
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            var game = new SinglePlayerGame(Guid.Empty, 4711, new BoardContent(SquareContent.Water), shooterBoard);

            Assert.Equal(SquareContent.Water, game.Shoot("A1"));
            Assert.Equal(SquareContent.Water, shooterBoard[new BoardIndex(0, 0)]);
        }

        [Fact]
        public void Shoot_Ship()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = board[1] = SquareContent.Ship;
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            var game = new SinglePlayerGame(Guid.Empty, 4711, board, shooterBoard);

            Assert.Equal(SquareContent.HitShip, game.Shoot("A1"));
            Assert.Equal(SquareContent.HitShip, shooterBoard[new BoardIndex(0, 0)]);
        }

        [Fact]
        public void Sink_Ship()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = board[1] = SquareContent.Ship;
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            var game = new SinglePlayerGame(Guid.Empty, 4711, board, shooterBoard);

            Assert.Equal(SquareContent.HitShip, game.Shoot("A1"));
            Assert.Equal(SquareContent.SunkenShip, game.Shoot("B1"));
            Assert.Equal(SquareContent.SunkenShip, shooterBoard[new BoardIndex(0)]);
            Assert.Equal(SquareContent.SunkenShip, shooterBoard[new BoardIndex(1)]);
        }

        [Fact]
        public void GetShotRequest()
        {
            var game = new SinglePlayerGame(Guid.Empty, 4711, new BoardContent(SquareContent.Water));
            var shotRequest = game.GetShotRequest();

            // Note value-based equality here. See also
            // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#value-based-equality

            Assert.Equal(new ShotRequest(Guid.Empty, 4711, game.ShootingBoard), shotRequest);
        }

        [Fact]
        public void Log()
        {
            var game = new SinglePlayerGame(Guid.Empty, 4711, new BoardContent(SquareContent.Water));
            game.Shoot("A1");
            game.Shoot("A2");

            Assert.Equal(2, game.Log.Count());
            Assert.Equal(new BoardIndex(), game.Log.First().Location);
        }

        [Fact]
        public void LastShot()
        {
            var game = new SinglePlayerGame(Guid.Empty, 4711, new BoardContent(SquareContent.Water));

            var request = game.GetShotRequest();
            Assert.Null(request.LastShot);
            game.Shoot("A1");
            Assert.Equal("A1", game.LastShot);

            request = game.GetShotRequest();
            Assert.NotNull(request.LastShot);
            Assert.Equal("A1", request.LastShot!);
            game.Shoot("B1");
            Assert.Equal("B1", game.LastShot);

            request = game.GetShotRequest();
            Assert.NotNull(request.LastShot);
            Assert.Equal("B1", request.LastShot!);
        }
    }
}
