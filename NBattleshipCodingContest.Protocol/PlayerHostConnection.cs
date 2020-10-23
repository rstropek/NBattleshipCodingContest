namespace NBattleshipCodingContest.Protocol
{
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a connection of the manager to the battle host
    /// </summary>
    public class PlayerHostConnection : IPlayerHostConnection
    {
        private int shooter = -1;
        private BoardIndex? shotLocation;
        private readonly IGameFactory gameFactory;
        private readonly ILogger<PlayerHostConnection> logger;
        private TaskCompletionSource? shootCompletion;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerHostConnection"/> type.
        /// </summary>
        /// <param name="gameFactory">Factory used to create games</param>
        /// <param name="logger">Logger</param>
        public PlayerHostConnection(IGameFactory gameFactory, ILogger<PlayerHostConnection> logger)
        {
            this.gameFactory = gameFactory;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void Connect(IServerStreamWriter<GameRequest> stream)
        {
            if (State == PlayerHostConnectionState.Connected)
            {
                throw new InvalidOperationException("Already connected. This should never happen!");
            }

            GameRequestStream = stream;
        }

        public async Task HandleRequestsLoop(IAsyncStreamReader<PlayerResponse> requestStream, CancellationToken token)
        {
            await foreach (var item in requestStream.ReadAllAsync(token))
            {
                await Handle(item);
            }
        }

        /// <inheritdoc/>
        public Game? Game { get; private set; }

        /// <inheritdoc/>
        public PlayerHostConnectionState State
        {
            get
            {
                if (GameRequestStream == null)
                {
                    return PlayerHostConnectionState.Disconnected;
                }

                if (Game == null)
                {
                    return PlayerHostConnectionState.Connected;
                }

                if (shooter == -1)
                {
                    return PlayerHostConnectionState.GameRunning;
                }

                if (shotLocation == null)
                {
                    return PlayerHostConnectionState.WaitingForShot;
                }

                return PlayerHostConnectionState.WaitingForShotAck;
            }
        }

        /// <inheritdoc/>
        public bool CanStartGame => State is PlayerHostConnectionState.Connected or PlayerHostConnectionState.GameRunning;

        /// <inheritdoc/>
        public void StartGame(int player1Index, int player2Index)
        {
            if (!CanStartGame)
            {
                throw new InvalidOperationException("No battle host connected.");
            }

            Game = gameFactory.Create(player1Index, player2Index);
        }

        private void StartShootingProcess(int shooter) => this.shooter = shooter;

        private void EndShootingProcess()
        {
            shooter = -1;
            shotLocation = null;
        }

        /// <inheritdoc/>
        public Task Shoot(int shooter)
        {
            if (State != PlayerHostConnectionState.GameRunning || Game == null)
            {
                throw new InvalidOperationException("Wrong game state. Forgot to call StartGame?");
            }

            shootCompletion = new TaskCompletionSource();

            SendShotRequest(shooter)
                .ContinueWith(t =>
                {
                    // Error during shot
                    if (t.IsFaulted && t.Exception != null)
                    {
                        logger.LogError(t.Exception, "Error while talking to battle host");
                        shootCompletion.TrySetException(t.Exception);
                    }
                });

            return shootCompletion.Task;
        }

        private async Task SendShotRequest(int shooter)
        {
            if (Game == null)
            {
                throw new InvalidOperationException("No game. This should never happen!");
            }

            await Send(ProtocolTranslator.EncodeShotRequest(Game.GetShotRequest(shooter)));
            StartShootingProcess(shooter);
        }

        /// <inheritdoc/>
        public async Task Handle(PlayerResponse response)
        {
            if (Game == null || shooter == -1 || shootCompletion == null)
            {
                throw new InvalidOperationException("Invalid state. Should never happen!");
            }

            async Task ProcessShot(Shot shot)
            {
                var decodedResponse = ProtocolTranslator.DecodeShotResponse(shot);

                if (decodedResponse.GameId != Game.GameId)
                {
                    throw new InvalidOperationException("Received shot for invalid game. Should never happen!");
                }

                var content = Game.Shoot(shooter, decodedResponse.Index);
                shotLocation = decodedResponse.Index;
                await Send(ProtocolTranslator.EncodeShotResult(new(Game.GameId, content)));
            }

            void ProcessShotResultAck()
            {
                EndShootingProcess();
                shootCompletion.TrySetResult();
            }

            switch (response.PayloadCase)
            {
                case PlayerResponse.PayloadOneofCase.Shot:
                    await ProcessShot(response.Shot);
                    break;
                case PlayerResponse.PayloadOneofCase.ShotResultAck:
                    ProcessShotResultAck();
                    break;
                default:
                    logger.LogInformation("Received unknown payload type {PayloadCase}", response.PayloadCase);
                    break;
            }
        }

        private async Task Send(GameRequest request)
        {
            if (State == PlayerHostConnectionState.Disconnected || GameRequestStream == null)
            {
                throw new InvalidOperationException("No battle host connected.");
            }

            await GameRequestStream.WriteAsync(request);
        }


        /// <inheritdoc/>
        public void Disconnect() => GameRequestStream = null;

        private IServerStreamWriter<GameRequest>? GameRequestStream { get; set; }
    }
}
