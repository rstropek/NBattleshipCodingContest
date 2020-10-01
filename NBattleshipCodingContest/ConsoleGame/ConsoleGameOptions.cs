namespace NBattleshipCodingContest.ConsoleGame
{
    using CommandLine;

    [Verb("consolegame", HelpText = "Executes a battleship game in the console (for debugging of players).")]
    internal class ConsoleGameOptions
    {
        [Option('p', "player-1", HelpText = "Index of player 1", Default = 0)]
        public int Player1Index { get; set; }

        [Option('q', "player-2", HelpText = "Index of player 2", Default = 1)]
        public int Player2Index { get; set; }
    }
}
