namespace Types
{
    public enum Color
    {
        White,
        Black
    };

    public enum Bounds
    {
        None,
        Upper,
        Lower,
        Exact = Upper | Lower
    }

    public enum CastlingRights
    {
        NoCastling,
        WhiteKingSide,
        WhiteQueenSide = WhiteKingSide << 1,
        BlackKingSide = WhiteKingSide << 2,
        BlackQueenSide = WhiteKingSide << 3,

        KingSide = WhiteKingSide | BlackKingSide,
        QueenSide = WhiteQueenSide | BlackQueenSide,
        WhiteCastling = WhiteKingSide | WhiteQueenSide,
        BlackCastling = BlackKingSide | BlackQueenSide,
        AnyCastling = WhiteCastling | BlackCastling
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

    public enum MoveType
    {
        Normal,
        Promotion = 1 << 14,
        EnPassant = 2 << 14,
        Castling = 3 << 14
    };

}