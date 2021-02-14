using System.Text.Json.Serialization;

namespace NBattleshipCodingContest.Logic
{
    // Note the use of base data types for enums. Read more at
    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SquareContent : byte
    {
        Water,
        Ship,
        HitShip,
        SunkenShip,
        Unknown
    }

    public enum Direction : byte
    {
        Horizontal,
        Vertical
    }
}
