namespace NBattleshipCodingContest.Protocol
{
    using Grpc.Core;
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public enum PlayerHostConnectionState
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
    public interface IPlayerHostConnection
    {
        /// <summary>
        /// Connect to battle host
        /// </summary>
        /// <param name="stream">Stream used to send game requests to battle host</param>
        /// <remarks>
        /// Switches <see cref="State"/> from <see cref="PlayerHostConnectionState.Disconnected"/>
        /// to <see cref="PlayerHostConnectionState.Connected"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Already connected</exception>
        void Connect(IServerStreamWriter<GameRequest> stream);

        /// <summary>
        /// Current state of the connection
        /// </summary>
        PlayerHostConnectionState State { get; }

        /// <summary>
        /// Indicates whether the connection is in a state where you can start a new game.
        /// </summary>
        bool CanStartGame { get; }

        Task HandleRequestsLoop(IAsyncStreamReader<PlayerResponse> requestStream, CancellationToken token);

        /// <summary>
        /// Start a new game
        /// </summary>
        /// <param name="player1Index">Index of player 1</param>
        /// <param name="player2Index">Index of player 2</param>
        /// <remarks>
        /// Switches <see cref="State"/> from <see cref="PlayerHostConnectionState.Connected"/>
        /// to <see cref="PlayerHostConnectionState.GameRunning"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Not in <see cref="PlayerHostConnectionState.Connected"/> state</exception>
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
        /// Is <c>null</c> if not in <see cref="PlayerHostConnectionState.GameRunning"/>
        /// or <see cref="PlayerHostConnectionState.WaitingForShot"/> state.
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
        /// <exception cref="InvalidOperationException">Not in <see cref="PlayerHostConnectionState.GameRunning"/> state</exception>
        Task Shoot(int shooter);

        /// <summary>
        /// Disconnects from battle host
        /// </summary>
        /// <remarks>
        /// Switches <see cref="State"/> from <see cref="PlayerHostConnectionState.Connected"/>
        /// to <see cref="PlayerHostConnectionState.Disconnected"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Already connected</exception>
        void Disconnect();
    }
}
