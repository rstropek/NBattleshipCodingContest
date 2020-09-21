namespace NBattleshipCodingContest.Players
{
    using System;

    // Note the use of a record here. Read more at
    // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#records

    /// <summary>
    /// Information about a player
    /// </summary>
    public record PlayerInfo(string Name, Func<PlayerBase> Create);
}
