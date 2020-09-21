using NBattleshipCodingContest.Logic;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBattleshipCodingContest.Players.Tests
{
    public class SequentialTests
    {
        [Fact]
        public void ThrowIfTooManyShots()
        {
            var board = new BoardContent(SquareContent.Water);
            var player = new Sequential();
            Assert.ThrowsAsync<InvalidOperationException>(() => player.GetShot(Guid.Empty, string.Empty, board, _ => Task.FromResult(SquareContent.Water)));
        }

        [Fact]
        public void SequentialShots()
        {
            var board = new BoardContent(SquareContent.Unknown);
            var player = new Sequential();
            Assert.Equal("A1", Shoot(board, player));
            Assert.Equal("B1", Shoot(board, player));
            for (var i = 0; i < 8; i++)
            {
                Shoot(board, player);
            }

            Assert.Equal("A2", Shoot(board, player));
        }

        private string Shoot(BoardContent board, PlayerBase player)
        {
            string? location = null;
            Task<SquareContent> shoot(string l)
            {
                location = l;
                board[l] = SquareContent.Water;
                return Task.FromResult(SquareContent.Water);
            }

            player.GetShot(Guid.Empty, string.Empty, board, shoot);
            Assert.NotNull(location);
            return location!;
        }
    }
}
