namespace NBattleshipCodingContest.Protocol
{
    using Google.Protobuf;
    using NBattleshipCodingContest.Logic;
    using System;
    using System.Diagnostics;
    using System.Linq;

    public static class ProtocolTranslator
    {
        public static UUID EncodeGuid(Guid guid) => new UUID { Value = guid.ToString() };

        public static Guid DecodeGuid(UUID guid) => Guid.Parse(guid.Value);

        public static ByteString EncodeBoard(Logic.IReadOnlyBoard board) => ByteString.CopyFrom(board.Cast<byte>().ToArray());

        public static Logic.IReadOnlyBoard EncodeBoard(ByteString boardBytes) => new BoardContent(boardBytes);

        /// <summary>
        /// Encodes square content
        /// </summary>
        /// <remarks>
        /// Note that this method assumes that values of enums in <see cref="SquareContent"/>
        /// and <see cref="Logic.SquareContent"/> are aligned.
        /// </remarks>
        public static SquareContent EncodeSquareContent(Logic.SquareContent content)
        {
            var result = (SquareContent)content;

            // Check enum value in debug build. Read more about assertation in C#
            // https://docs.microsoft.com/en-us/visualstudio/debugger/assertions-in-managed-code
            Debug.Assert(Enum.IsDefined(typeof(Logic.SquareContent), content));
            Debug.Assert(Enum.IsDefined(typeof(SquareContent), result));

            return result;
        }

        public static Logic.BoardIndex DecodeLocation(string location) => new Logic.BoardIndex(location);

        // Note target-typed new in the following method. Read more at
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/target-typed-new

        public static GameRequest EncodeShotRequest(Logic.ShotRequest request) =>
            new()
            {
                ShotRequest = new() 
                {
                    GameId = EncodeGuid(request.GameId),
                    Shooter = request.Shooter,
                    Opponent = request.Opponent,
                    Board = EncodeBoard(request.BoardShooterView)
                }
            };

        public static Logic.ShotRequest DecodeShotRequest(ShotRequest request) =>
            new(DecodeGuid(request.GameId), request.Shooter, request.Opponent, EncodeBoard(request.Board));

        public static GameRequest EncodeShotResult(Logic.ShotResult result) =>
            new()
            {
                ShotResult = new()
                {
                    GameId = EncodeGuid(result.GameId),
                    SquareContent = EncodeSquareContent(result.SquareContent)
                }
            };

        public static PlayerResponse EncodeShotResponse(Logic.ShotResponse shot) =>
            new()
            {
                Shot = new()
                {
                    GameId = EncodeGuid(shot.GameId),
                    Location = shot.Index
                }
            };

        public static Logic.ShotResponse DecodeShotResponse(Shot response) =>
            new(DecodeGuid(response.GameId), DecodeLocation(response.Location));
    }
}
