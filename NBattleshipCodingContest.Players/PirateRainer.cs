﻿namespace NBattleshipCodingContest.Players
{
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements Rainer's battleship player
    /// </summary>
    public class PirateRainer : PlayerBase
    {
        private bool DoesItMakeSenseToShootHere(IReadOnlyBoard board, BoardIndex ix)
        {
            var isHitAbove = ix.Row > 0 && board[new BoardIndex(ix.Column, ix.Row - 1)] == SquareContent.HitShip;
            if (isHitAbove)
            {
                var isHitAboveLeft = ix.Column > 0 && board[new BoardIndex(ix.Column - 1, ix.Row - 1)] == SquareContent.HitShip;
                var isHitAboveRight = ix.Column < 9 && board[new BoardIndex(ix.Column + 1, ix.Row - 1)] == SquareContent.HitShip;
                if (isHitAboveLeft || isHitAboveRight)
                {
                    return false;
                }
            }

            return board[ix] == SquareContent.Unknown;
        }

        /// <inheritdoc />
        public override async Task GetShot(Guid _, string __, IReadOnlyBoard board, Shoot shoot)
        {
            var ix = new BoardIndex();

            // Find next unknown square
            while (!DoesItMakeSenseToShootHere(board, ix)) ix = ix.Next();

            // Shoot at first unknonwn square
            await shoot(ix);
        }
    }
}
