namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Indicates that no placement of all the ships for the board could have been found.
    /// </summary>
    public class BoardTooOccupiedException : ApplicationException
    {
        public BoardTooOccupiedException()
        {
        }

        public BoardTooOccupiedException(string? message) : base(message)
        {
        }

        public BoardTooOccupiedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected BoardTooOccupiedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
