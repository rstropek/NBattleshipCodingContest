namespace NBattleshipCodingContest.Players
{
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Shoot at a given location
    /// </summary>
    /// <param name="location">Location string (e.g. A1, B5, J10) consisting of column (A..J) and row (1..10)</param>
    /// <returns>
    /// Content of the square after shot (i.e. <see cref="SquareContent.Water"/> or <see cref="SquareContent.HitShip"/>).
    /// </returns>
    public delegate Task<SquareContent> Shoot(string location);

    /// <summary>
    /// Base class for a battleship player
    /// </summary>
    public abstract class PlayerBase
    {
        /// <summary>
        /// Gets or sets the last shot of the player
        /// </summary>
        /// <remarks>
        /// If the player did not shoot before, <see cref="LastShot"/> is <c>null</c>.
        /// </remarks>
        public BoardIndex? LastShot { get; set; }

        /// <summary>
        /// Gets the next shot from the player.
        /// </summary>
        /// <param name="gameId">Unique identifier of the current game</param>
        /// <param name="opponent">identifier of the opponent</param>
        /// <param name="board">Current board content</param>
        /// <param name="shoot">Callback with which the method has to do the shooting</param>
        /// <remarks>
        /// <para>
        /// The method has to call <paramref name="shoot"/> exactly once before returning.
        /// </para>
        /// <para>
        /// The method has to finish within 250ms.
        /// </para>
        /// <para>
        /// The method is not expected to throw an exception.
        /// </para>
        /// <para>
        /// Players have to be stateless. During a battle, a new player might be instantiated
        /// at any time, maybe even for every shot. If the player needs state, it has to store
        /// its state externally (e.g. file on disk).
        /// </para>
        /// <para>
        /// The <paramref name="gameId"/> will stay the same during a single game between two players.
        /// </para>
        /// </remarks>
        public abstract Task GetShot(Guid gameId, string opponentId, IReadOnlyBoard board, Shoot shoot);
    }
}
