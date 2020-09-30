namespace NBattleshipCodingContest.Logic.Tests
{
    using System.Text.Json;
    using Xunit;

    public class BoardIndexConverterTests
    {
        [Fact]
        public void Serialize()
        {
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new BoardIndexJsonConverter());
            var jsonString = JsonSerializer.Serialize(new BoardIndex(), serializeOptions);
            Assert.Equal("\"A1\"", jsonString);
        }

        [Fact]
        public void SerializeDefaultOptions()
        {
            var jsonString = JsonSerializer.Serialize(new BoardIndex());
            Assert.Equal("\"A1\"", jsonString);
        }

        [Fact]
        public void Deserialize()
        {
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new BoardIndexJsonConverter());
            var ix = JsonSerializer.Deserialize<BoardIndex>("\"A1\"", serializeOptions);
            Assert.Equal(new BoardIndex(), ix);
        }

        [Fact]
        public void DeserializeDefaultOptions()
        {
            var ix = JsonSerializer.Deserialize<BoardIndex>("\"A1\"");
            Assert.Equal(new BoardIndex(), ix);
        }
    }
}
