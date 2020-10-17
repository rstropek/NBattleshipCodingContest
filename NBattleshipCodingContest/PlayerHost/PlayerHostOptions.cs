namespace NBattleshipCodingContest.PlayerHost
{
    using CommandLine;

    [Verb("playerhost", HelpText = "Starts a player host process (gRPC client).")]
    internal class PlayerHostOptions
    {
        [Option('u', "allow-unencrypted", HelpText = "Allow unencrypted gRPC connections.", Default = true)]
        public bool AllowUnencrypted { get; set; }

        [Option('m', "manager-url", HelpText = "URL of the manager gRPC server", Default = "https://localhost:5001")]
        public string ManagerUrl { get; set; } = "https://localhost:5001";
    }
}
