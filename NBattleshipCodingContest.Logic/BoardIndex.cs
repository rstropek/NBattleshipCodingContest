namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Diagnostics;
    using System.Text.Json.Serialization;

    // Note readonly struct here. Read more at
    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct#readonly-struct

    /// <summary>
    /// Index in a battleship board
    /// </summary>
    /// <remarks>
    /// <para>Contains helper methods useful when implementing the battleship board as a continuous array.</para>
    /// <para>The implementation assumes the classic battleship game with a side length of 10.</para>
    /// </remarks>
    [JsonConverter(typeof(BoardIndexJsonConverter))]
    public readonly struct BoardIndex : IEquatable<BoardIndex>
    {
        private readonly int index;

        #region Internal helper functions
        private static int GetIndex(int col, int row)
        {
            // This is a private method, so we can assume that col and row
            // contain valid values. We check them only in debug builds.

            // Read more about assertation in C#
            // https://docs.microsoft.com/en-us/visualstudio/debugger/assertions-in-managed-code
            Debug.Assert(col is >= 0 and <= 9 || row is >= 0 and <= 9);

            return row * 10 + col;
        }

        private static bool TryParse(string location, out int index)
        {
            // Note range expression. Read more at
            // https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes

            // Note out parameter declaration in method call. Read more at
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/out-parameter-modifier

            if (location.Length is >= 2 and <= 3 && location[0] is >= 'A' and <= 'J' && int.TryParse(location[1..], out var row) && row is >= 1 and <= 10)
            {
                index = GetIndex(location[0] - 'A', row - 1);
                return true;
            }

            index = 0;
            return false;
        }
        #endregion

        #region Constructors
        // Note throw expression. Read more at
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/throw#the-throw-expression

        // Note expression-bodied constructor. Read more at
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/expression-bodied-members#constructors

        // Note new C# 9 relational pattern matching
        // https://devblogs.microsoft.com/dotnet/welcome-to-c-9-0/#relational-patterns

        /// <summary>
        /// Initializes a new instance of the <see cref="BoardIndex"/> type.
        /// </summary>
        /// <param name="index">Zero-based board index between 0 and 9</param>
        /// <exception cref="ArgumentOutOfRangeException">Given index is out of range</exception>
        public BoardIndex(int index) => 
            this.index = (index is >= 0 and < 100) ? index : throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>
        /// Initializes a new instance of the <see cref="BoardIndex"/> type.
        /// </summary>
        /// <param name="col">Column index between A and J</param>
        /// <param name="row">One-based row index between 1 and 10</param>
        /// <exception cref="ArgumentOutOfRangeException">At least one of the given indexes is out of range</exception>
        public BoardIndex(char col, int row)
        {
            #region Check input parameter
            // This is a public method, so we have to check that parameters
            // contain valid values.

            if (col is < 'A' or > 'J')
            {
                throw new ArgumentOutOfRangeException(nameof(col), "Must be between A and J");
            }

            if (row is < 1 or > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(row), "Must be between 1 and 10");
            }
            #endregion

            index = GetIndex(col - 'A', row - 1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoardIndex"/> type.
        /// </summary>
        /// <param name="col">Zero-based column index between 0 and 9</param>
        /// <param name="row">Zero-based row index between 0 and 9</param>
        /// <exception cref="ArgumentOutOfRangeException">At least one of the given indexes is out of range</exception>
        public BoardIndex(int col, int row)
        {
            #region Check input parameter
            // This is a public method, so we have to check that parameters
            // contain valid values.

            if (col is < 0 or > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(col), "Must be between 0 and 9");
            }

            if (row is < 0 or > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(row), "Must be between 0 and 9");
            }
            #endregion

            index = GetIndex(col, row);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoardIndex"/> type.
        /// </summary>
        /// <param name="location">Location string (e.g. A1, B5, J10) consisting of column (A..J) and row (1..10)</param>
        /// <exception cref="ArgumentOutOfRangeException">Given location has invalid format or index is out of range</exception>
        public BoardIndex(string location)
        {
            // Note out parameter declaration in method call. Read more at
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/out-parameter-modifier
            if (TryParse(location, out int ix))
            {
                index = new BoardIndex(ix);
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(location), "Has to be in the format <column><row> where column is A..J and row is 1..10");
        }
        #endregion

        #region Type conversion and deconstruction
        /// <summary>
        /// Tries to parse a given location string.
        /// </summary>
        /// <param name="location">Location string (e.g. A1, B5, J10) consisting of column (A..J) and row (1..10)</param>
        /// <param name="index">Parsed index. Content is undefined if location could not be parsed.</param>
        /// <returns>
        /// <c>true</c> if location could be parsed, otherwise <c>false</c>.
        /// </returns>
        public static bool TryParse(string location, out BoardIndex index)
        {
            if (TryParse(location, out int ix))
            {
                index = new BoardIndex(ix);
                return true;
            }

            index = new BoardIndex();
            return false;
        }

        /// <summary>
        /// Converts a <see cref="BoardIndex"/> into a location string (e.g. A1, B5, J10) consisting of column (A..J) and row (1..10)
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static implicit operator string(BoardIndex value) => $"{(char)('A' + value.Column)}{value.Row + 1}";

        /// <summary>
        /// Returns a new board index referencing the next square (wraps to next row if necessary)
        /// </summary>
        /// <param name="value">Value to increase</param>
        /// <returns>
        /// New board index
        /// </returns>
        /// <exception cref="InvalidOperationException">Already on last square</exception>
        public static BoardIndex operator ++(BoardIndex value) => value.Next();

        /// <summary>
        /// Returns a new board index referencing the previous square (wraps to next row if necessary)
        /// </summary>
        /// <param name="value">Value to decrease</param>
        /// <returns>
        /// New board index
        /// </returns>
        /// <exception cref="InvalidOperationException">Already on first square</exception>
        public static BoardIndex operator --(BoardIndex value) => value.Previous();

        /// <summary>
        /// Converts a given string to a board index.
        /// </summary>
        /// <param name="location">Location string (e.g. A1, B5, J10) consisting of column (A..J) and row (1..10)</param>
        /// <exception cref="ArgumentOutOfRangeException">Given location has invalid format or index is out of range</exception>
        public static implicit operator BoardIndex(string location) => new BoardIndex(location);

        /// <summary>
        /// Converts a <see cref="BoardIndex"/> into a zero-based board index
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static implicit operator int(BoardIndex value) => value.index;

        // Note readonly modifier on method. Read more at
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/readonly-instance-members

        /// <summary>
        /// Deconstructs into column and row indexes
        /// </summary>
        /// <param name="col">Receives zero-based column index</param>
        /// <param name="row">Receives zero-based row index</param>
        public readonly void Deconstruct(out int col, out int row) => (col, row) = (Column, Row);
        #endregion

        #region IEquatable and object
        /// <inheritdoc/>
        public readonly override bool Equals(object? obj) => obj is BoardIndex index && Equals(index);

        /// <inheritdoc/>
        public readonly bool Equals(BoardIndex other) => index == other.index;

        /// <inheritdoc/>
        public readonly override int GetHashCode() => index.GetHashCode();

        public static bool operator ==(BoardIndex left, BoardIndex right) => left.Equals(right);

        public static bool operator !=(BoardIndex left, BoardIndex right) => !(left == right);
        #endregion

        #region Navigation functions
        /// <summary>
        /// Tries to retrieve a board index referencing the next column or row.
        /// </summary>
        /// <param name="direction">Direction in which to go</param>
        /// <param name="newIndex">New index. Value is undefined if method returns <c>false</c>.</param>
        /// <returns>
        /// <c>true</c> if movement is possible (i.e. not on outermost right or bottom column or row), otherwise <c>false</c>.
        /// </returns>
        public readonly bool TryNext(Direction direction, out BoardIndex newIndex)
        {
            if (direction == Direction.Horizontal && Column < 9)
            {
                newIndex = new BoardIndex(index + 1);
                return true;
            }

            if (direction == Direction.Vertical && Row < 9)
            {
                newIndex = new BoardIndex(index + 10);
                return true;
            }

            newIndex = new BoardIndex();
            return false;
        }

        /// <summary>
        /// Tries to retrieve a board index referencing the previous column or row.
        /// </summary>
        /// <param name="direction">Direction in which to go</param>
        /// <param name="newIndex">New index. Value is undefined if method returns <c>false</c>.</param>
        /// <returns>
        /// <c>true</c> if movement is possible (i.e. not on outermost left or top column or row), otherwise <c>false</c>.
        /// </returns>
        public readonly bool TryPrevious(Direction direction, out BoardIndex newIndex)
        {
            if (direction == Direction.Horizontal && Column > 0)
            {
                newIndex = new BoardIndex(index - 1);
                return true;
            }

            if (direction == Direction.Vertical && Row > 0)
            {
                newIndex = new BoardIndex(index - 10);
                return true;
            }

            newIndex = new BoardIndex();
            return false;
        }

        /// <summary>
        /// Returns a new board index referencing the next square (wraps to next row if necessary)
        /// </summary>
        /// <returns>
        /// New board index
        /// </returns>
        /// <exception cref="InvalidOperationException">Already on last square</exception>
        public readonly BoardIndex Next()
        {
            if (index < 10 * 10 - 1)
            {
                return new BoardIndex(index + 1);
            }

            throw new InvalidOperationException("Already on last square");
        }

        /// <summary>
        /// Returns a new board index referencing the previous square (wraps to previous row if necessary)
        /// </summary>
        /// <returns>
        /// New board index
        /// </returns>
        /// <exception cref="InvalidOperationException">Already on first square</exception>
        public readonly BoardIndex Previous()
        {
            if (index > 0)
            {
                return new BoardIndex(index - 1);
            }

            throw new InvalidOperationException("Already on first square");
        }

        /// <summary>
        /// Returns a new board index referencing the next column (i.e. column to the right)
        /// </summary>
        /// <returns>
        /// New board index
        /// </returns>
        /// <exception cref="InvalidOperationException">Already on last column</exception>
        public readonly BoardIndex NextColumn() => Column < 9 ? new BoardIndex(index + 1) : throw new InvalidOperationException("Already on last column");

        /// <summary>
        /// Returns a new board index referencing the previous column (i.e. column to the left)
        /// </summary>
        /// <returns>
        /// New board index
        /// </returns>
        /// <exception cref="InvalidOperationException">Already on first column</exception>
        public readonly BoardIndex PreviousColumn() => Column > 0 ? new BoardIndex(index - 1) : throw new InvalidOperationException("Already on first column");

        /// <summary>
        /// Returns a new board index referencing the next row (i.e. below)
        /// </summary>
        /// <returns>
        /// New board index
        /// </returns>
        /// <exception cref="InvalidOperationException">Already on last row</exception>
        public readonly BoardIndex NextRow() => Row < 9 ? new BoardIndex(index + 10) : throw new InvalidOperationException("Already on last row");

        /// <summary>
        /// Returns a new board index referencing the previous row (i.e. up)
        /// </summary>
        /// <returns>
        /// New board index
        /// </returns>
        /// <exception cref="InvalidOperationException">Already on first row</exception>
        public readonly BoardIndex PreviousRow() => Row > 0 ? new BoardIndex(index - 10) : throw new InvalidOperationException("Already on first row");
        #endregion

        #region Properties
        /// <summary>
        /// Gets the zero-based column
        /// </summary>
        public readonly int Column => index % 10;

        /// <summary>
        /// Gets the zero-based row
        /// </summary>
        public readonly int Row => index / 10;
        #endregion
    }
}
