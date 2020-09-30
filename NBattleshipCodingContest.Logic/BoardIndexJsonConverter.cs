namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Custom JSON converter for <see cref="BoardIndex"/>
    /// </summary>
    public class BoardIndexJsonConverter : JsonConverter<BoardIndex>
    {
        /// <inheritdoc/>
        public override BoardIndex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions _) =>
            new BoardIndex(reader.GetString() ?? string.Empty);

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, BoardIndex value, JsonSerializerOptions _) =>
            writer.WriteStringValue(value);
    }
}
