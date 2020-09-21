namespace NBattleshipCodingContest.Protocol.Tests
{
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NBattleshipCodingContest.Logic;
    using NBattleshipCodingContest.Players;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class ManagerConnectionTests
    {
        [Fact]
        public void Connect()
        {
            var stream = Mock.Of<IClientStreamWriter<PlayerResponse>>();
            var logger = Mock.Of<ILogger<ManagerConnection>>();

            var mc = new ManagerConnection(logger, Array.Empty<PlayerInfo>());

            Assert.Equal(ManagerConnectionState.Disconnected, mc.State);
            mc.Connect(stream);
            Assert.Equal(ManagerConnectionState.Connected, mc.State);
        }

        public class Dummy : PlayerBase
        {
            public SquareContent? Content { get; private set; }

            /// <inheritdoc />
            public override async Task GetShot(Guid _, string __, IReadOnlyBoard board, Shoot shoot) =>
                Content = await shoot(new BoardIndex());
        }

        [Fact]
        public void Handle()
        {
            var logger = Mock.Of<ILogger<ManagerConnection>>();

            var stream = new Mock<IClientStreamWriter<PlayerResponse>>();
            PlayerResponse? playerResponse = null;
            stream.Setup(s => s.WriteAsync(It.IsAny<PlayerResponse>()))
                .Callback<PlayerResponse>(pr => playerResponse = pr)
                .Returns(Task.FromResult(0));

            // Create connection and start game
            var player = new Dummy();
            var players = new[]
            {
                new PlayerInfo("Foo", () => player),
                new PlayerInfo("Bar", () => new Dummy())
            };
            var mc = new ManagerConnection(logger, players);
            mc.Connect(stream.Object);

            // Simulate incoming ShotRequest
            mc.Handle(ProtocolTranslator.EncodeShotRequest(new(Guid.Empty, 0, 1, new BoardContent(SquareContent.Unknown))));
            Assert.NotNull(playerResponse);
            Assert.Equal(PlayerResponse.PayloadOneofCase.Shot, playerResponse!.PayloadCase);

            // Simulate incoming ShotResult
            Assert.Null(player.Content);
            mc.Handle(ProtocolTranslator.EncodeShotResult(new(Guid.Empty, SquareContent.Water)));
            Assert.NotNull(player.Content);
            Assert.Equal(SquareContent.Water, player.Content);
        }
    }
}
