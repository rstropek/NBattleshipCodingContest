namespace NBattleshipCodingContest.BattleHost
{
    using CommandLine;

    [Verb("battlehost", HelpText = "Starts a battle host process (gRPC client).")]
    internal class BattleHostOptions
    {
        [Option('u', "allow-unencrypted", HelpText = "Allow unencrypted gRPC connections.", Default = true)]
        public bool AllowUnencrypted { get; set; }

        [Option('m', "manager-url", HelpText = "URL of the manager gRPC server", Default = "https://localhost:5001")]
        public string ManagerUrl { get; set; } = "https://localhost:5001";
    }
}
