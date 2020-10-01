namespace NBattleshipCodingContest.Logic.Tests
{
    using System;
    using System.Text.Json;
    using Xunit;

    public class BoardContentConverterTests
    {
        private static BoardContent CreateBoard()
        {
            var board = new BoardContent(SquareContent.Unknown);
            board["A1"] = SquareContent.Water;
            board["B1"] = SquareContent.Ship;
            board["C1"] = SquareContent.HitShip;
            return board;
        }

        [Fact]
        public void Serialize()
        {
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new BoardContentJsonConverter());
            var jsonString = JsonSerializer.Serialize(CreateBoard(), serializeOptions);
            Assert.Equal("\"WSH".PadRight(101, ' ') + "\"", jsonString);
        }

        [Fact]
        public void SerializeDefaultOptions()
        {
            var jsonString = JsonSerializer.Serialize(CreateBoard());
            Assert.Equal("\"WSH".PadRight(101, ' ') + "\"", jsonString);
        }

        [Fact]
        public void Deserialize()
        {
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new BoardContentJsonConverter());
            var content = JsonSerializer.Deserialize<BoardContent>("\"WSH".PadRight(101, ' ') + "\"", serializeOptions);
            Assert.Equal(CreateBoard().ToString(), content!.ToString());
        }

        [Fact]
        public void DeserializeDefaultOptions()
        {
            var content = JsonSerializer.Deserialize<BoardContent>("\"WSH".PadRight(101, ' ') + "\"");
            Assert.Equal(CreateBoard().ToString(), content!.ToString());
        }

        [Fact]
        public void DeserializeEmpty() => 
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BoardContent>("\"\""));

        [Fact]
        public void DeserializeShort() => 
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BoardContent>("\"WSH\""));

        [Fact]
        public void DeserializeIllegalChars() =>
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BoardContent>("\"DUMMY\""));
    }
}
