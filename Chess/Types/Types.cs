namespace Types
{
    public enum Color
    {
        White,
        Black
    };

    [Flags]
    public enum CastlingRights
    {
        None = 0,
        WhiteKingSide = 1,
        WhiteQueenSide = 2,
        BlackKingSide = 4,
        BlackQueenSide = 8,

        WhiteCastling = WhiteKingSide | WhiteQueenSide,
        BlackCastling = BlackKingSide | BlackQueenSide,
        All = WhiteCastling | BlackCastling
    };
}