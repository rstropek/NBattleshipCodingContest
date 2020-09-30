namespace NBattleshipCodingContest.Manager
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Microsoft.AspNetCore.Builder;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using NBattleshipCodingContest.Players;
    using NBattleshipCodingContest.Protocol;
    using NBattleshipCodingContest.Logic;
    using System.Diagnostics;
    using Microsoft.Extensions.FileProviders;

    internal class ManagerMain
    {
        [SuppressMessage("Microsoft.Performance", "CA1822", Scope = "method", Justification = "Static method could not act as context for logging")]
        public async Task StartManager(ManagerOptions options)
        {
            var logger = Log.Logger.ForContext<ManagerMain>();

            var webTask = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureServices(services =>
                        {
                            // Add all players
                            services.AddSingleton(PlayerList.Players);

                            services.AddOpenApiDocument(settings =>
                            {
                                settings.Title = "Battleship Manager API";
                                settings.Description = "API for Battleship manager";
                            });
                            services.AddGrpc();
                            services.AddControllers();
                            services.AddSingleton<IGameFactory, GameFactory>();
                            services.AddSingleton<IBoardFiller, RandomBoardFiller>();
                            services.AddSingleton<IBattleHostConnection, BattleHostConnection>();
                        })
                        .Configure((context, app) =>
                        {
                            if (context.HostingEnvironment.IsDevelopment())
                            {
                                app.UseDeveloperExceptionPage();
                            }

                            var fp = new ManifestEmbeddedFileProvider(typeof(ManagerMain).Assembly, "wwwroot");
                            app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fp });
                            app.UseStaticFiles(new StaticFileOptions { FileProvider = fp });

                            app.UseOpenApi();
                            app.UseSwaggerUi3();

                            app.UseRouting();
                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapGrpcService<ManagerService>();
                                endpoints.MapControllers();
                            });
                        })
                        .UseUrls(options.ManagerUrl);
                })
                .UseSerilog()
                .UseConsoleLifetime()
                .Build()
                .RunAsync();

            Process? bhp = null;
            if (options.StartBattleHost)
            {
                bhp = Process.Start("dotnet", "run -- battlehost");
            }

            await webTask;

            if (options.StartBattleHost && bhp != null)
            {
                bhp.WaitForExit();
            }
        }
    }
}
