namespace NBattleshipCodingContest.BattleHost
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using NBattleshipCodingContest.Protocol;
    using Serilog;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using static NBattleshipCodingContest.Protocol.Manager;
    using NBattleshipCodingContest.Players;

    internal class BattleHostMain
    {
        [SuppressMessage("Microsoft.Performance", "CA1822", Scope = "method", Justification = "Static method could not act as context for logging")]
        public async Task StartBattleHost(BattleHostOptions options)
        {
            var logger = Log.Logger.ForContext<BattleHostMain>();

            if (options.AllowUnencrypted)
            {
                logger.Warning("Allowing unencrypted gRPC connections");
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            }

            await new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                    configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile("appsettings.json", optional: true);
                    configApp.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                    configApp.AddEnvironmentVariables(prefix: "PREFIX_");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddGrpcClient<ManagerClient>(o => { o.Address = new Uri(options.ManagerUrl); });

                    // Add all players
                    services.AddSingleton(PlayerList.Players);
                    services.AddSingleton<IManagerConnection, ManagerConnection>();
                    services.AddHostedService<BattleHostService>();
                })
                .UseSerilog()
                .UseConsoleLifetime()
                .Build()
                .RunAsync();
        }
    }
}
