namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections.Generic;

    // NOTE that this interface is for future use. Currently not part of the project.

    /// <summary>
    /// Represents a log entry in a single-player game' log
    /// </summary>
    public record SinglePlayerGameLogRecord(BoardIndex Location, SquareContent ShotResult);

    /// <summary>
    /// Represents a single-player Battleship game.
    /// </summary>
    /// <remarks>
    /// In real life, two human players play the Battleship game against each other. However, in our context
    /// we want to evaluate computer players. We want to find out how many shots they need in order to sink
    /// all ships. Therefore, there is no need for a second player. Games are simply played until the computer
    /// player has sunk all ships. By counting the number of required shots, we can find the computer player
    /// with the best strategy.
    /// </remarks>
    public interface ISinglePlayerGame
    {
        /// <summary>
        /// Gets the game ID
        /// </summary>
        Guid GameId { get; }

        /// <summary>
        /// Gets the player index
        /// </summary>
        int PlayerIndex { get; }

        /// <summary>
        /// Gets the board with ships on it
        /// </summary>
        /// <remarks>
        /// The computer player has to sink those ships. Does not contain <see cref="SquareContent.Unknown"/>
        /// squares.
        /// </remarks>
        IReadOnlyBoard Board { get; }

        /// <summary>
        /// Gets the board with all the shots of the computer player
        /// </summary>
        /// <remarks>
        /// Typically contains some <see cref="SquareContent.Unknown"/> squares.
        /// </remarks>
        IReadOnlyBoard ShootingBoard { get; }

        /// <summary>
        /// Gets the history of shots
        /// </summary>
        IEnumerable<SinglePlayerGameLogRecord> Log { get; }

        /// <summary>
        /// Gets the last shot (if there is one)
        /// </summary>
        /// <returns>Last shot</returns>
        BoardIndex? LastShot { get; }

        /// <summary>
        /// Given player shoots at a given index
        /// </summary>
        /// <param name="ix">Square on which the player shoots</param>
        /// <returns>Square content after the shot.</returns>
        /// <remarks>
        /// If the shot hits a <see cref="SquareContent.Ship"/> square, the square
        /// turns into a <see cref="SquareContent.HitShip"/>. If all squares of a ship
        /// have been hit, the entire ship turns into <see cref="SquareContent.SunkenShip"/>.
        /// </remarks>
        SquareContent Shoot(BoardIndex ix);

        /// <summary>
        /// Builds a <see cref="ShotRequest"/> from a shot.
        /// </summary>
        /// <returns>
        /// Created <see cref="ShotRequest"/>
        /// </returns>
        ShotRequest GetShotRequest();
    }
}
