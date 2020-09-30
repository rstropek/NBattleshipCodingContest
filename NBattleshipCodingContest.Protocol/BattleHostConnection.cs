namespace NBattleshipCodingContest.Protocol
{
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Threading.Tasks;

    public enum BattleHostConnectionState
    {
        Disconnected,
        Connected,
        GameRunning,
        WaitingForShot,
        WaitingForShotAck
    }

    /// <summary>
    /// Represents a connection of the manager to the battle host
    /// </summary>
    public interface IBattleHostConnection
    {
        /// <summary>
        /// Connect to battle host
        /// </summary>
        /// <param name="stream">Stream used to send game requests to battle host</param>
        /// <remarks>
        /// Switches <see cref="State"/> from <see cref="BattleHostConnectionState.Disconnected"/>
        /// to <see cref="BattleHostConnectionState.Connected"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Already connected</exception>
        void Connect(IServerStreamWriter<GameRequest> stream);

        /// <summary>
        /// Current state of the connection
        /// </summary>
        BattleHostConnectionState State { get; }

        /// <summary>
        /// Indicates whether the connection is in a state where you can start a new game.
        /// </summary>
        bool CanStartGame { get; }

        /// <summary>
        /// Start a new game
        /// </summary>
        /// <param name="player1Index">Index of player 1</param>
        /// <param name="player2Index">Index of player 2</param>
        /// <remarks>
        /// Switches <see cref="State"/> from <see cref="BattleHostConnectionState.Connected"/>
        /// to <see cref="BattleHostConnectionState.GameRunning"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Not in <see cref="BattleHostConnectionState.Connected"/> state</exception>
        void StartGame(int player1Index, int player2Index);

        /// <summary>
        /// Handles a player response sent from battle host to manager
        /// </summary>
        /// <param name="response">Player response to process</param>
        Task Handle(PlayerResponse response);

        /// <summary>
        /// Gets the current game
        /// </summary>
        /// <remarks>
        /// Is <c>null</c> if not in <see cref="BattleHostConnectionState.GameRunning"/>
        /// or <see cref="BattleHostConnectionState.WaitingForShot"/> state.
        /// </remarks>
        Game? Game { get; }

        /// <summary>
        /// Make a shoot
        /// </summary>
        /// <param name="shooter">Player who shoots (1 or 2)</param>
        /// <remarks>
        /// The returned <see cref="Task"/> completes when the player has sent back a shot
        /// (<seealso cref="Handle(PlayerResponse)"/>) and the shot has been processed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Not in <see cref="BattleHostConnectionState.GameRunning"/> state</exception>
        Task Shoot(int shooter);

        /// <summary>
        /// Disconnects from battle host
        /// </summary>
        /// <remarks>
        /// Switches <see cref="State"/> from <see cref="BattleHostConnectionState.Connected"/>
        /// to <see cref="BattleHostConnectionState.Disconnected"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Already connected</exception>
        void Disconnect();
    }

    /// <summary>
    /// Implements a connection of the manager to the battle host
    /// </summary>
    public class BattleHostConnection : IBattleHostConnection
    {
        private int shooter = -1;
        private BoardIndex? shotLocation;
        private readonly IGameFactory gameFactory;
        private readonly ILogger<BattleHostConnection> logger;
        private TaskCompletionSource? shootCompletion;

        /// <summary>
        /// Initializes a new instance of the <see cref="BattleHostConnection"/> type.
        /// </summary>
        /// <param name="gameFactory">Factory used to create games</param>
        /// <param name="logger">Logger</param>
        public BattleHostConnection(IGameFactory gameFactory, ILogger<BattleHostConnection> logger)
        {
            this.gameFactory = gameFactory;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void Connect(IServerStreamWriter<GameRequest> stream)
        {
            if (State == BattleHostConnectionState.Connected)
            {
                throw new InvalidOperationException("Already connected. This should never happen!");
            }

            GameRequestStream = stream;
        }

        /// <inheritdoc/>
        public Game? Game { get; private set; }

        /// <inheritdoc/>
        public BattleHostConnectionState State
        {
            get
            {
                if (GameRequestStream == null)
                {
                    return BattleHostConnectionState.Disconnected;
                }

                if (Game == null)
                {
                    return BattleHostConnectionState.Connected;
                }

                if (shooter == -1)
                {
                    return BattleHostConnectionState.GameRunning;
                }

                if (shotLocation == null)
                {
                    return BattleHostConnectionState.WaitingForShot;
                }

                return BattleHostConnectionState.WaitingForShotAck;
            }
        }

        /// <inheritdoc/>
        public bool CanStartGame => State is BattleHostConnectionState.Connected or BattleHostConnectionState.GameRunning;

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
            if (State != BattleHostConnectionState.GameRunning || Game == null)
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
            if (State == BattleHostConnectionState.Disconnected || GameRequestStream == null)
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
