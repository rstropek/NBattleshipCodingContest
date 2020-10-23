﻿namespace NBattleshipCodingContest.Players
{
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a battleship player that randomly shoots at squares
    /// </summary>
    public class RandomShots : PlayerBase
    {
        /// <inheritdoc />
        public override async Task GetShot(Guid _, IReadOnlyBoard ___, Shoot shoot)
        {
            // Return a random shot between A1 and J10
            var rand = new Random();
            await shoot(new BoardIndex(rand.Next(10), rand.Next(10)));
        }
    }
}
