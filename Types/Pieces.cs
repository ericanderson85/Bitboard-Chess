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
        King,
        AllPieces = 0
    };

    public enum Piece
    {
        None,
        WhitePawn = PieceType.Pawn,
        WhiteKnight,
        WhiteBishop,
        WhiteRook,
        WhiteQueen,
        WhiteKing,
        BlackPawn = PieceType.Pawn + 8,
        BlackKnight,
        BlackBishop,
        BlackRook,
        BlackQueen,
        BlackKing
    }

    public static class Pieces
    {
        public static Piece Flip(Piece piece)
        {
            return (Piece)((int)piece ^ 8);
        }
        public static PieceType TypeOf(Piece piece)
        {
            return (PieceType)((int)piece & 7);
        }
        public static Piece MakePiece(Color color, PieceType pieceType)
        {
            return (Piece)(((int)color << 3) + pieceType);
        }
        public static Bitboard BitBoard(Square square)
        {
            return Files.BitBoard(Files.FileOf(square));
        }
    }
}