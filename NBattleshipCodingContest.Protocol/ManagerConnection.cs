namespace NBattleshipCodingContest.Protocol
{
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using NBattleshipCodingContest.Players;
    using System;
    using System.Threading.Tasks;

    public enum ManagerConnectionState
    {
        Disconnected,
        Connected
    }

    /// <summary>
    /// Represents a connection of the battle host to the manager
    /// </summary>
    public interface IManagerConnection
    {
        /// <summary>
        /// Current state of the connection
        /// </summary>
        public ManagerConnectionState State { get; }

        /// <summary>
        /// Connect to manager
        /// </summary>
        /// <param name="stream">Stream used to send player requests to manager</param>
        /// <remarks>
        /// Switches <see cref="State"/> from <see cref="ManagerConnectionState.Disconnected"/>
        /// to <see cref="ManagerConnectionState.Connected"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Already connected</exception>
        void Connect(IClientStreamWriter<PlayerResponse> stream);

        /// <summary>
        /// Handles game requests sent from manager to battle host
        /// </summary>
        /// <param name="request">Game request to process</param>
        /// <exception cref="InvalidOperationException">Not in <see cref="ManagerConnectionState.Connected"/> state</exception>
        void Handle(GameRequest request);
    }

    /// <summary>
    /// Implements a connection of the manager to the battle host
    /// </summary>
    public class ManagerConnection : IManagerConnection
    {
        private TaskCompletionSource<Logic.SquareContent>? shotCompletion;
        private readonly ILogger<ManagerConnection> logger;
        private readonly PlayerInfo[] players;

        public ManagerConnection(ILogger<ManagerConnection> logger, PlayerInfo[] players)
        {
            this.logger = logger;
            this.players = players;
        }

        private IClientStreamWriter<PlayerResponse>? PlayerResponseStream { get; set; }

        /// <inheritdoc/>
        public void Connect(IClientStreamWriter<PlayerResponse> stream)
        {
            if (State == ManagerConnectionState.Connected)
            {
                throw new InvalidOperationException("Already connected. This should never happen!");
            }

            PlayerResponseStream = stream;
        }

        /// <inheritdoc/>
        public ManagerConnectionState State
        {
            get
            {
                if (PlayerResponseStream == null)
                {
                    return ManagerConnectionState.Disconnected;
                }

                return ManagerConnectionState.Connected;
            }
        }

        /// <inheritdoc/>
        public void Handle(GameRequest request)
        {
            if (State != ManagerConnectionState.Connected)
            {
                throw new InvalidOperationException("Not connected");
            }

            switch (request.PayloadCase)
            {
                case GameRequest.PayloadOneofCase.ShotRequest:
                    GetShot(request.ShotRequest).ContinueWith(t =>
                    {
                        if (t.IsFaulted && t.Exception != null)
                        {
                            logger.LogError(t.Exception, "Error getting shot");
                        }
                    });
                    break;
                case GameRequest.PayloadOneofCase.ShotResult:
                    ProcessShotResult(request.ShotResult);
                    break;
                default:
                    logger.LogInformation("Received unknown payload type {PayloadCase}", request.PayloadCase);
                    break;
            }
        }

        private async Task GetShot(ShotRequest request)
        {
            if (State == ManagerConnectionState.Disconnected)
            {
                throw new InvalidOperationException("Not connected to battle manager.");
            }

            shotCompletion = new TaskCompletionSource<Logic.SquareContent>();

            var shotRequest = ProtocolTranslator.DecodeShotRequest(request);
            var shooter = players[shotRequest.Shooter].Create();
            await shooter.GetShot(shotRequest.GameId, players[shotRequest.Opponent].Name, shotRequest.BoardShooterView, loc =>
            {
                if (PlayerResponseStream == null)
                {
                    throw new InvalidOperationException("No response stream. Should never happen!");
                }

                Send(ProtocolTranslator.EncodeShotResponse(new(shotRequest.GameId, loc)))
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted && t.Exception != null)
                        {
                            logger.LogError(t.Exception, "Error while talking to battle manager");
                            shotCompletion.TrySetException(t.Exception);
                        }
                    });

                return shotCompletion.Task;
            });
            await Send(ProtocolTranslator.EncodeShotResultAck());
        }


        private async Task Send(PlayerResponse response)
        {
            if (State == ManagerConnectionState.Disconnected || PlayerResponseStream == null)
            {
                throw new InvalidOperationException("No manager connected.");
            }

            await PlayerResponseStream.WriteAsync(response);
        }

        private void ProcessShotResult(Protocol.ShotResult result)
        {
            if (shotCompletion == null)
            {
                throw new InvalidOperationException("Wrong internal state, should never happen!");
            }

            shotCompletion.TrySetResult((Logic.SquareContent)result.SquareContent);
        }
    }
}
