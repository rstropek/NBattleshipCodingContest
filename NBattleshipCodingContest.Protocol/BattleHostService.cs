namespace NBattleshipCodingContest.Protocol
{
    using Grpc.Core;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System.Threading;
    using System.Threading.Tasks;
    using static NBattleshipCodingContest.Protocol.Manager;

    public class BattleHostService : IHostedService
    {
        private readonly ILogger<BattleHostService> logger;
        private readonly ManagerClient client;
        private readonly IManagerConnection connection;
        private readonly IHostApplicationLifetime appLifetime;
        private AsyncDuplexStreamingCall<PlayerResponse, GameRequest>? Connection;

        public BattleHostService(ILogger<BattleHostService> logger, ManagerClient client, IManagerConnection connection,
            IHostApplicationLifetime appLifetime)
        {
            this.logger = logger;
            this.client = client;
            this.connection = connection;
            this.appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Register callback for application stopping event. App shutdown
            // will be delayed until this callback is finished.
            appLifetime.ApplicationStopping.Register(() =>
            {
                if (Connection != null)
                {
                    logger.LogInformation("Stopping battle host.");
                    Connection.RequestStream.CompleteAsync().Wait();
                }
            });

            Connection = client.Connect(cancellationToken: cancellationToken);

            // Start background task handling incoming battle requests.
            Task.Run(async () =>
            {
                connection.Connect(Connection.RequestStream);

                try
                { 
                    await foreach (var item in Connection.ResponseStream.ReadAllAsync(CancellationToken.None))
                    {
                        connection.Handle(item);
                    }
                }
                finally
                {
                    // When connection to manager stops, the application should stop.
                    appLifetime.StopApplication();
                }
            }, CancellationToken.None);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken _) => Task.CompletedTask;
    }
}
