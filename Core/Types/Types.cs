namespace Types
{
    public enum Color
    {
        White,
        Black
    };

    public enum Bounds
    {
        Exact,
        Lower,
        Upper
    }

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

    public enum Direction
    {
        North = 8,
        East = 1,
        South = -8,
        West = -1,

        NorthEast = North + East,
        SouthEast = South + East,
        SouthWest = South + West,
        NorthWest = North + West
    };
}