namespace NBattleshipCodingContest.Logic.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class GameTests
    {
        private static Game CreateGame() =>
            new Game(Guid.Empty, new[] { 47, 11 }, new[] { new BoardContent(SquareContent.Water), new BoardContent(SquareContent.Water) },
                new[] { new BoardContent(SquareContent.Unknown), new BoardContent(SquareContent.Unknown) });

        [Fact]
        public void Shoot_Into_Water()
        {
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            // Note with expression for record. See also
            // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#with-expressions

            var game = CreateGame() with { ShootingBoards = new[] { shooterBoard, new BoardContent(SquareContent.Unknown) } };

            Assert.Equal(SquareContent.Water, game.Shoot(1, "A1"));
            Assert.Equal(SquareContent.Water, shooterBoard[new BoardIndex(0, 0)]);
        }

        [Fact]
        public void Shoot_Ship()
        {
            var board = new BoardContent(SquareContent.Water);
            board[new BoardIndex(0, 0)] = SquareContent.Ship;
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            var game = CreateGame() with
            {
                Boards = new[] { board, new BoardContent(SquareContent.Water) },
                ShootingBoards = new[] { new BoardContent(SquareContent.Unknown), shooterBoard }
            };

            Assert.Equal(SquareContent.HitShip, game.Shoot(2, "A1"));
            Assert.Equal(SquareContent.HitShip, shooterBoard[new BoardIndex(0, 0)]);
        }

        [Fact]
        public void Shoot_Invalid()
        {
            var game = new Game(Guid.Empty, new[] { 0 }, new[] { new BoardContent(SquareContent.Water) },
                new[] { new BoardContent(SquareContent.Unknown) });

            Assert.Throws<ArgumentOutOfRangeException>(() => game.Shoot(0, new BoardIndex()));
            Assert.Throws<InvalidOperationException>(() => game.Shoot(1, new BoardIndex()));
        }

        [Fact]
        public void GetShotRequest()
        {
            var game = CreateGame();
            var shotRequest = game.GetShotRequest(1);

            // Note value-based equality here. See also
            // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#value-based-equality

            Assert.Equal(new ShotRequest(Guid.Empty, 47, 11, game.ShootingBoards[0]), shotRequest);
        }

        [Fact]
        public void GetWinner_Draw()
        {
            var shootingBoards = new[] { new BoardContent(SquareContent.Unknown), new BoardContent(SquareContent.Unknown) };
            var game = CreateGame() with { ShootingBoards = shootingBoards };
            shootingBoards[0][new BoardIndex(0, 0)] = shootingBoards[1][new BoardIndex(0, 0)] = SquareContent.HitShip;
            Assert.Equal(Winner.Draw, game.GetWinner(1));
        }

        [Fact]
        public void GetWinner_Player1()
        {
            var shooterBoard1 = new BoardContent(SquareContent.Unknown);
            var game = CreateGame() with { ShootingBoards = new[] { shooterBoard1, new BoardContent(SquareContent.Unknown) } };
            shooterBoard1[new BoardIndex(0, 0)] = SquareContent.HitShip;
            Assert.Equal(Winner.Player2, game.GetWinner(1));
        }

        [Fact]
        public void GetWinner_Player2()
        {
            var shooterBoard2 = new BoardContent(SquareContent.Unknown);
            var game = CreateGame() with { ShootingBoards = new[] { new BoardContent(SquareContent.Unknown), shooterBoard2 } };
            shooterBoard2[new BoardIndex(0, 0)] = SquareContent.HitShip;
            Assert.Equal(Winner.Player1, game.GetWinner(1));
        }

        [Fact]
        public void GetWinner_NoWinner()
        {
            Assert.Equal(Winner.NoWinner, CreateGame().GetWinner(1));
        }

        [Fact]
        public void Log()
        {
            var game = CreateGame();
            game.Shoot(1, "A1");
            game.Shoot(2, "A1");

            Assert.Equal(2, game.Log.Count());
            Assert.Equal(47, game.Log.First().Shooter);
            Assert.Equal(new BoardIndex(), game.Log.First().Location);
        }
    }
}
