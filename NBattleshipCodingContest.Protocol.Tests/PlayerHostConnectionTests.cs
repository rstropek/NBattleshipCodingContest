namespace NBattleshipCodingContest.Protocol.Tests
{
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class PlayerHostConnectionTests
    {
        [Fact]
        public void Connect()
        {
            var stream = Mock.Of<IServerStreamWriter<GameRequest>>();
            var logger = Mock.Of<ILogger<PlayerHostConnection>>();
            var factory = Mock.Of<IGameFactory>();

            var bhc = new PlayerHostConnection(factory, logger);

            Assert.Equal(PlayerHostConnectionState.Disconnected, bhc.State);
            bhc.Connect(stream);
            Assert.Equal(PlayerHostConnectionState.Connected, bhc.State);
        }

        [Fact]
        public void Disconnect()
        {
            var stream = Mock.Of<IServerStreamWriter<GameRequest>>();
            var logger = Mock.Of<ILogger<PlayerHostConnection>>();
            var factory = Mock.Of<IGameFactory>();

            var bhc = new PlayerHostConnection(factory, logger);

            Assert.Equal(PlayerHostConnectionState.Disconnected, bhc.State);
            bhc.Connect(stream);
            bhc.Disconnect();
            Assert.Equal(PlayerHostConnectionState.Disconnected, bhc.State);
        }

        [Fact]
        public void CanStartGame()
        {
            var stream = Mock.Of<IServerStreamWriter<GameRequest>>();
            var logger = Mock.Of<ILogger<PlayerHostConnection>>();
            var factory = Mock.Of<IGameFactory>();

            var bhc = new PlayerHostConnection(factory, logger);
            Assert.False(bhc.CanStartGame);
            bhc.Connect(stream);
            Assert.True(bhc.CanStartGame);
        }

        private static Game CreateGame() =>
           new Game(Guid.Empty, new[] { 47, 11 }, new[] { new BoardContent(SquareContent.Water), new BoardContent(SquareContent.Water) },
               new[] { new BoardContent(SquareContent.Unknown), new BoardContent(SquareContent.Unknown) });

        [Fact]
        public void StartGame()
        {
            var stream = Mock.Of<IServerStreamWriter<GameRequest>>();
            var logger = Mock.Of<ILogger<PlayerHostConnection>>();
            var factory = new Mock<IGameFactory>();
            factory.Setup(f => f.Create(It.IsAny<int>(), It.IsAny<int>())).Returns(CreateGame());

            var bhc = new PlayerHostConnection(factory.Object, logger);
            bhc.Connect(stream);
            bhc.StartGame(47, 11);
            factory.Verify(f => f.Create(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
            Assert.Equal(PlayerHostConnectionState.GameRunning, bhc.State);
        }

        [Fact]
        public async Task Shoot()
        {
            // This test verifies the entire shooting protocol from the 
            // PlayerHostConnection's view.

            var logger = Mock.Of<ILogger<PlayerHostConnection>>();

            var stream = new Mock<IServerStreamWriter<GameRequest>>();
            GameRequest? gameRequest = null;
            stream.Setup(s => s.WriteAsync(It.IsAny<GameRequest>()))
                .Callback<GameRequest>(gr => gameRequest = gr)
                .Returns(Task.FromResult(0));

            var factory = new Mock<IGameFactory>();
            factory.Setup(f => f.Create(It.IsAny<int>(), It.IsAny<int>())).Returns(CreateGame());

            // Create connection and start game
            var bhc = new PlayerHostConnection(factory.Object, logger);
            bhc.Connect(stream.Object);
            bhc.StartGame(47, 11);
            Assert.NotNull(bhc.Game);

            // Simulate shooting of player 1
            var shootTask = bhc.Shoot(1);
            Assert.False(shootTask.IsCompleted);
            Assert.NotNull(gameRequest);
            Assert.Equal(GameRequest.PayloadOneofCase.ShotRequest, gameRequest!.PayloadCase);
            Assert.NotNull(gameRequest.ShotRequest);
            Assert.Empty(gameRequest.ShotRequest.LastShot);
            Assert.Equal(SquareContent.Unknown, bhc.Game!.ShootingBoards[0][new BoardIndex(0, 0)]);

            // Simulate incoming shot result 
            await bhc.Handle(ProtocolTranslator.EncodeShotResponse(new(Guid.Empty, new BoardIndex(0, 0))));
            Assert.False(shootTask.IsCompleted);
            Assert.Equal(SquareContent.Water, bhc.Game!.ShootingBoards[0][new BoardIndex(0, 0)]);

            // Simulate incoming shot ack
            await bhc.Handle(ProtocolTranslator.EncodeShotResultAck());
            Assert.True(shootTask.IsCompleted);
        }
    }
}
