namespace NBattleshipCodingContest
{
    using CommandLine;
    using Microsoft.Extensions.Configuration;
    using NBattleshipCodingContest.PlayerHost;
    using NBattleshipCodingContest.ConsoleGame;
    using NBattleshipCodingContest.Manager;
    using NBattleshipCodingContest.Players;
    using Serilog;
    using System;
    using System.IO;
    using System.Text;

    class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
             .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
             .AddEnvironmentVariables()
             .Build();

        static void Main(string[] args)
        {
            // Set proper output encoding for ASCII extended characters
            // (used to draw battleship boards on the screen).
            Console.OutputEncoding = Encoding.UTF8;

            // Setup logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("App Name", "NBattleshipCodingContest")
                .CreateLogger();

            // Parse command line arguments and start services
            // depending on given verb.
            Parser.Default.ParseArguments<ManagerOptions, PlayerHostOptions, AboutOptions,
                PlayerListOptions, ConsoleGameOptions>(args)
                .MapResult(
                  (ManagerOptions options) => StartRunnerAndReturnExitCode(options),
                  (PlayerHostOptions options) => StartPlayerHostAndReturnExitCode(options),
                  (AboutOptions options) => ShowAboutAndReturnExitCode(options),
                  (PlayerListOptions options) => ShowPlayerList(options),
                  (ConsoleGameOptions options) => RunConsoleGame(options),
                  errors => 1);
        }

        private static int ShowAboutAndReturnExitCode(AboutOptions _)
        {
            Console.WriteLine("Learn C# with classical Battleship game.");
            Console.WriteLine("By Rainer Stropek");
            Console.WriteLine(@"
              |    |    |                 
             )_)  )_)  )_)              
            )___))___))___)\            
           )____)____)_____)\\
         _____|____|____|____\\\__
---------\                   /---------
    ^^^^^ ^^^^^^^^^^^^^^^^^^^^^
      ^^^^      ^^^^     ^^^    ^^
           ^^^^      ^^^");
            return 0;
        }

        private static int ShowPlayerList(PlayerListOptions _)
        {
            Console.WriteLine("Here is a list of players:\n");

            for(var i = 0; i < PlayerList.Players.Length; i++)
            {
                Console.WriteLine($"{i}: {PlayerList.Players[i].Name}");
            }

            return 0;
        }

        private static int StartPlayerHostAndReturnExitCode(PlayerHostOptions options)
        {
            var log = Log.Logger.ForContext<Program>();
            log.Information("Starting battle host process...");
            var host = new PlayerHostMain();
            host.StartPlayerHost(options).Wait();
            log.Information("Battle host process ended.");
            return 0;
        }

        private static int StartRunnerAndReturnExitCode(ManagerOptions options)
        {
            var log = Log.Logger.ForContext<Program>();
            log.Information("Starting manager process...");
            var manager = new ManagerMain();
            manager.StartManager(options).Wait();
            log.Information("Manager process ended.");
            return 0;
        }

        private static int RunConsoleGame(ConsoleGameOptions options)
        {
            var consoleGameManger = new ConsoleGameMain();
            consoleGameManger.StartConsoleGame(options).Wait();
            return 0;
        }
    }
}
