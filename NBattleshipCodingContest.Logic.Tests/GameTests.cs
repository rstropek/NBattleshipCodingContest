namespace NBattleshipCodingContest.Logic.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class GameTests
    {
        private static Game CreateGame()
        {
            return new Game(Guid.Empty,
                new[] {
                    new SinglePlayerGame(Guid.Empty, 47, new BoardContent(SquareContent.Water)),
                    new SinglePlayerGame(Guid.Empty, 11, new BoardContent(SquareContent.Water))
                });
        }

        private static Game CreateGame(BoardContent[] shooterBoards)
        {
            return new Game(Guid.Empty,
                new[] {
                    new SinglePlayerGame(Guid.Empty, 47, new BoardContent(SquareContent.Water), shooterBoards[0]),
                    new SinglePlayerGame(Guid.Empty, 11, new BoardContent(SquareContent.Water), shooterBoards[1])
                });
        }

        private static Game CreateGame(IReadOnlyBoard[] boards, BoardContent[] shooterBoards)
        {
            return new Game(Guid.Empty,
                new[] {
                    new SinglePlayerGame(Guid.Empty, 47, boards[1], shooterBoards[0]),
                    new SinglePlayerGame(Guid.Empty, 11, boards[0], shooterBoards[1])
                });
        }

        [Fact]
        public void Shoot_Into_Water()
        {
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            // Note with expression for record. See also
            // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#with-expressions

            var game = CreateGame(new[] { shooterBoard, new BoardContent(SquareContent.Unknown) });

            Assert.Equal(SquareContent.Water, game.Shoot(1, "A1"));
            Assert.Equal(SquareContent.Water, shooterBoard[new BoardIndex(0, 0)]);
        }

        [Fact]
        public void Shoot_Ship()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = board[1] = SquareContent.Ship;
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            var game = CreateGame(
                new[] { board, new BoardContent(SquareContent.Water) },
                new[] { new BoardContent(SquareContent.Unknown), shooterBoard }
            );

            Assert.Equal(SquareContent.HitShip, game.Shoot(2, "A1"));
            Assert.Equal(SquareContent.HitShip, shooterBoard[new BoardIndex(0, 0)]);
        }

        [Fact]
        public void Sink_Ship()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = board[1] = SquareContent.Ship;
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            var game = CreateGame(
                new[] { board, new BoardContent(SquareContent.Water) },
                new[] { new BoardContent(SquareContent.Unknown), shooterBoard }
            );

            Assert.Equal(SquareContent.HitShip, game.Shoot(2, "A1"));
            Assert.Equal(SquareContent.SunkenShip, game.Shoot(2, "B1"));
            Assert.Equal(SquareContent.SunkenShip, shooterBoard[new BoardIndex(0)]);
            Assert.Equal(SquareContent.SunkenShip, shooterBoard[new BoardIndex(1)]);
        }

        [Fact]
        public void GetShotRequest()
        {
            var game = CreateGame();
            var shotRequest = game.GetShotRequest(1);

            // Note value-based equality here. See also
            // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#value-based-equality

            Assert.Equal(new ShotRequest(Guid.Empty, 47, game.ShootingBoards[0]), shotRequest);
        }

        [Fact]
        public void GetWinner_Draw()
        {
            var shootingBoards = new[] { new BoardContent(SquareContent.Unknown), new BoardContent(SquareContent.Unknown) };
            var game = CreateGame(shootingBoards);
            shootingBoards[0][new BoardIndex(0, 0)] = shootingBoards[1][new BoardIndex(0, 0)] = SquareContent.HitShip;
            Assert.Equal(Winner.Draw, game.GetWinner(1));
        }

        [Fact]
        public void GetWinner_Draw_Too_Many_moves()
        {
            var shootingBoards = new[] { new BoardContent(SquareContent.Unknown), new BoardContent(SquareContent.Unknown) };
            var game = CreateGame(shootingBoards);
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
            var game = CreateGame(new[] { shooterBoard1, new BoardContent(SquareContent.Unknown) });
            shooterBoard1[new BoardIndex(0, 0)] = SquareContent.HitShip;
            Assert.Equal(Winner.Player1, game.GetWinner(1));
        }

        [Fact]
        public void GetWinner_Player2()
        {
            var shooterBoard2 = new BoardContent(SquareContent.Unknown);
            var game = CreateGame(new[] { new BoardContent(SquareContent.Unknown), shooterBoard2 });
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

        [Fact]
        public void LastShot()
        {
            var game = CreateGame();

            var request = game.GetShotRequest(1);
            Assert.Null(request.LastShot);
            game.Shoot(1, "A1");
            Assert.Equal("A1", game.GetLastShot(1));

            request = game.GetShotRequest(2);
            Assert.Null(request.LastShot);
            game.Shoot(2, "A1");
            Assert.Equal("A1", game.GetLastShot(2));

            request = game.GetShotRequest(1);
            Assert.NotNull(request.LastShot);
            Assert.Equal("A1", request.LastShot!);
            game.Shoot(1, "B1");
            Assert.Equal("B1", game.GetLastShot(1));

            request = game.GetShotRequest(2);
            Assert.NotNull(request.LastShot);
            Assert.Equal("A1", request.LastShot!);
            game.Shoot(2, "B1");
            Assert.Equal("B1", game.GetLastShot(2));

            request = game.GetShotRequest(1);
            Assert.NotNull(request.LastShot);
            Assert.Equal("B1", request.LastShot!);
        }
    }
}
