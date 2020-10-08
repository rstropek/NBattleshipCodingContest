namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Custom JSON converter for <see cref="BoardContent"/>
    /// </summary>
    public class BoardContentJsonConverter : JsonConverter<BoardContent>
    {
        /// <inheritdoc/>
        public override BoardContent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions _)
        {
            var stringValue = reader.GetString();
            if (stringValue == null)
            {
                throw new JsonException("Board content must not be null");
            }

            if (stringValue.Length != 100)
            {
                throw new JsonException("Board content has invalid length, must be exactly 100 chars");
            }

            if (stringValue.Any(c => c is not 'W' and not 'S' and not 'H' and not ' '))
            {
                throw new JsonException("Board content contains invalid characters");
            }

            var board = new BoardContent();
            for (var i = 0; i < 100; i++)
            {
                board[new BoardIndex(i)] = stringValue[i] switch
                {
                    'W' => SquareContent.Water,
                    'S' => SquareContent.Ship,
                    'H' => SquareContent.HitShip,
                    ' ' => SquareContent.Unknown,
                    _ => throw new InvalidOperationException("Invalid square content, should never happen!")
                };
            }

            return board;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, BoardContent value, JsonSerializerOptions _) =>
            writer.WriteStringValue(value.ToShortString());
    }
}
