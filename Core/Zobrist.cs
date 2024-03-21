using Types;

namespace Core
{

    class ZobristHashing
    {
        private readonly Random rng = new(64);
        private readonly ulong[,] _zobristTable;
        private readonly ulong _whiteToMove;
        private readonly ulong[] _castlingRights;
        private readonly ulong[] _enPassantFile;

        public ZobristHashing()
        {
            _zobristTable = new ulong[12, 64];
            GenerateZobristTable();

            _castlingRights = new ulong[4];
            for (int i = 0; i < _castlingRights.Length; i++)
                _castlingRights[i] = NextRandomULong();

            _enPassantFile = new ulong[8];
            for (int i = 0; i < _enPassantFile.Length; i++)
                _enPassantFile[i] = NextRandomULong();

            _whiteToMove = NextRandomULong();
        }

        private void GenerateZobristTable()
        {
            for (Piece piece = Piece.WhitePawn; piece <= Piece.BlackKing; piece++)
            {
                for (Square square = Square.A1; square <= Square.H8; square++)
                {
                    _zobristTable[(int)piece, (int)square] = NextRandomULong();
                }
            }
        }

        private ulong NextRandomULong()
        {
            byte[] buffer = new byte[8];
            rng.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public ulong CalculateHash(Board board, State state)
        {
            ulong hash = 0;

            foreach (Square square in Bitboards.Squares(board.WhitePawns))
                hash ^= _zobristTable[(int)Piece.WhitePawn, (int)square];

            foreach (Square square in Bitboards.Squares(board.WhiteKnights))
                hash ^= _zobristTable[(int)Piece.WhiteKnight, (int)square];

            foreach (Square square in Bitboards.Squares(board.WhiteBishops))
                hash ^= _zobristTable[(int)Piece.WhiteBishop, (int)square];

            foreach (Square square in Bitboards.Squares(board.WhiteRooks))
                hash ^= _zobristTable[(int)Piece.WhiteRook, (int)square];

            foreach (Square square in Bitboards.Squares(board.WhiteQueens))
                hash ^= _zobristTable[(int)Piece.WhiteQueen, (int)square];

            foreach (Square square in Bitboards.Squares(board.WhiteKing))
                hash ^= _zobristTable[(int)Piece.WhiteKing, (int)square];

            foreach (Square square in Bitboards.Squares(board.BlackPawns))
                hash ^= _zobristTable[(int)Piece.BlackPawn, (int)square];

            foreach (Square square in Bitboards.Squares(board.BlackKnights))
                hash ^= _zobristTable[(int)Piece.BlackKnight, (int)square];

            foreach (Square square in Bitboards.Squares(board.BlackBishops))
                hash ^= _zobristTable[(int)Piece.BlackBishop, (int)square];

            foreach (Square square in Bitboards.Squares(board.BlackRooks))
                hash ^= _zobristTable[(int)Piece.BlackRook, (int)square];

            foreach (Square square in Bitboards.Squares(board.BlackQueens))
                hash ^= _zobristTable[(int)Piece.BlackQueen, (int)square];

            foreach (Square square in Bitboards.Squares(board.BlackKing))
                hash ^= _zobristTable[(int)Piece.BlackKing, (int)square];


            if (state.Turn == Color.White)
                hash ^= _whiteToMove;

            hash ^= _castlingRights[(int)state.CastlingRights];

            if (state.EnPassantSquare.HasValue)
                hash ^= _enPassantFile[(int)Files.Of((Square)state.EnPassantSquare)];


            return hash;
        }

        public void UpdateHash(ref ulong hash, Piece oldPiece, Piece newPiece, Square oldSquare, Square newSquare)
        {
            if (oldPiece != Piece.None)
                hash ^= _zobristTable[(int)oldPiece, (int)oldSquare];

            if (newPiece != Piece.None)
                hash ^= _zobristTable[(int)newPiece, (int)newSquare];
        }
    }
}