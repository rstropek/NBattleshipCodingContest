namespace NBattleshipCodingContest.Manager
{
    using CommandLine;

    [Verb("manager", HelpText = "Starts a battleship game manager process (gRPC server).")]
    internal class ManagerOptions
    {
        [Option('m', "manager-url", HelpText = "URL on which the manager should listen", Default = "https://localhost:5001")]
        public string ManagerUrl { get; set; } = "https://localhost:5001";

        [Option('b', "battle-host", HelpText = "Automatically start battle host as background process", Default = false)]
        public bool StartBattleHost { get; set; }
    }
}
