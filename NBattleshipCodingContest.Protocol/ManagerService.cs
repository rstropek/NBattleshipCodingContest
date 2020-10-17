namespace NBattleshipCodingContest.Protocol
{
    using Grpc.Core;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class ManagerService : Manager.ManagerBase
    {
        private static bool BattleRunnerConnected;
        private static readonly object BattleRunnerConnectedLockObject = new object();
        private readonly ILogger<ManagerService> logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly IPlayerHostConnection connection;

        public ManagerService(ILogger<ManagerService> logger, IHostApplicationLifetime appLifetime, IPlayerHostConnection connection)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.connection = connection;
        }

        public override async Task Connect(IAsyncStreamReader<PlayerResponse> requestStream, IServerStreamWriter<GameRequest> responseStream, ServerCallContext context)
        {
            // Make sure that only one battle host is running
            lock (BattleRunnerConnectedLockObject)
            {
                if (BattleRunnerConnected)
                {
                    logger.LogWarning("Another battle runner tried to connect, declining.");
                    context.Status = new Grpc.Core.Status(StatusCode.ResourceExhausted,
                        "A battle runner is already connected. Currently, multiple battle runners are not supported.");
                    return;
                }

                BattleRunnerConnected = true;
                connection.Connect(responseStream);
            }

            // Cancellation token source used to cancel the gRPC call
            var cts = new CancellationTokenSource();
            using var registration = appLifetime.ApplicationStopping.Register(() =>
            {
                // If manager shuts down, we cancel the gRPC call
                cts.Cancel();

                // Give it a second to process the cancellation
                Task.Delay(100).Wait();
            });

            try
            {
                logger.LogInformation("New battle runner connected.");
                await foreach (var item in requestStream.ReadAllAsync(cts.Token))
                {
                    await connection.Handle(item);
                }

                logger.LogInformation("Battle runner exited");
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Manager is shutting down");
            }
            finally
            {
                lock (BattleRunnerConnectedLockObject)
                {
                    BattleRunnerConnected = false;
                    connection.Disconnect();
                }
            }
        }
    }
}
