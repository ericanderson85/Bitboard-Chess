namespace Types
{
    public enum PieceType
    {
        None,
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    };

    public enum Piece
    {
        WhitePawn,
        WhiteKnight,
        WhiteBishop,
        WhiteRook,
        WhiteQueen,
        WhiteKing,
        BlackPawn,
        BlackKnight,
        BlackBishop,
        BlackRook,
        BlackQueen,
        BlackKing
    }

    public static class Pieces
    {
        public static Piece GetPiece(PieceType pieceType, Color color)
        {
            return (Piece)(pieceType + (color == Color.White ? -1 : 5));
        }
    }
}