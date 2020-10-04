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
        public void GetWinner_Draw_Too_Many_moves()
        {
            var shootingBoards = new[] { new BoardContent(SquareContent.Unknown), new BoardContent(SquareContent.Unknown) };
            var game = CreateGame() with { ShootingBoards = shootingBoards };
            for (var i = 0; i < 200; i++)
            {
                game.Shoot(1, new BoardIndex(0));
            }

            Assert.Equal(Winner.Draw, game.GetWinner(1));
        }

        [Fact]
        public void GetWinner_Player1()
        {
            var shooterBoard1 = new BoardContent(SquareContent.Unknown);
            var game = CreateGame() with { ShootingBoards = new[] { shooterBoard1, new BoardContent(SquareContent.Unknown) } };
            shooterBoard1[new BoardIndex(0, 0)] = SquareContent.HitShip;
            Assert.Equal(Winner.Player1, game.GetWinner(1));
        }

        [Fact]
        public void GetWinner_Player2()
        {
            var shooterBoard2 = new BoardContent(SquareContent.Unknown);
            var game = CreateGame() with { ShootingBoards = new[] { new BoardContent(SquareContent.Unknown), shooterBoard2 } };
            shooterBoard2[new BoardIndex(0, 0)] = SquareContent.HitShip;
            Assert.Equal(Winner.Player2, game.GetWinner(1));
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

        [Theory]
        [InlineData("B2,C2,D2")]
        [InlineData("B1,C1,D1")]
        [InlineData("H1,I1,J1")]
        [InlineData("B2,B3,B4")]
        [InlineData("B1,B2,B3")]
        [InlineData("B7,B8,B9")]
        [InlineData("A1,B1")]
        [InlineData("I10,J10")]
        public void IsSunken(string shipLocation)
        {
            var board = new BoardContent(SquareContent.Water);
            var locations = shipLocation.Split(',').ToList();
            locations.ForEach(l => board[l] = SquareContent.HitShip);
            Assert.All(locations, l => Game.IsShipSunken(board, l));
        }

        [Theory]
        [InlineData("B2,C2,D2")]
        [InlineData("B1,C1,D1")]
        [InlineData("H1,I1,J1")]
        [InlineData("B2,B3,B4")]
        [InlineData("B1,B2,B3")]
        [InlineData("B8,B9,B10")]
        [InlineData("A1,B1")]
        [InlineData("I10,J10")]
        public void IsNotSunken(string shipLocation)
        {
            var board = new BoardContent(SquareContent.Water);
            var locations = shipLocation.Split(',');
            locations[..^1].ToList().ForEach(l => board[l] = SquareContent.HitShip);
            board[locations[^1]] = SquareContent.Ship;
            Assert.DoesNotContain(locations, l => Game.IsShipSunken(board, l));
        }
    }
}
