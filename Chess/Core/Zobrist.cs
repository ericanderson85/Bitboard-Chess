using Types;
using File = Types.File;

namespace Core
{

    public static class Zobrist
    {
        private const int SEED = 8502;
        private static readonly ulong[,] _table;
        private static readonly ulong _whiteToMove;
        private static readonly ulong[] _castlingRights;
        private static readonly ulong[] _enPassantFile;

        static Zobrist()
        {
            Random rng = new(SEED);

            _table = new ulong[12, 64];
            GenerateZobristTable(rng);

            _castlingRights = new ulong[16];
            for (int i = 0; i < _castlingRights.Length; i++)
                _castlingRights[i] = NextRandomULong(rng);

            _enPassantFile = new ulong[8];
            for (int i = 0; i < _enPassantFile.Length; i++)
                _enPassantFile[i] = NextRandomULong(rng);

            _whiteToMove = NextRandomULong(rng);
        }

        private static void GenerateZobristTable(Random rng)
        {
            for (Piece piece = Piece.WhitePawn; piece <= Piece.BlackKing; piece++)
            {
                for (Square square = Square.A1; square <= Square.H8; square++)
                {
                    _table[(int)piece, (int)square] = NextRandomULong(rng);
                }
            }
        }

        private static ulong NextRandomULong(Random rng)
        {
            byte[] buffer = new byte[8];
            rng.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static ulong CalculateHash(Board board, State state)
        {
            ulong hash = 0;

            foreach (Square square in Bitboards.ToSquares(board.WhitePawns))
                hash ^= _table[(int)Piece.WhitePawn, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.WhiteKnights))
                hash ^= _table[(int)Piece.WhiteKnight, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.WhiteBishops))
                hash ^= _table[(int)Piece.WhiteBishop, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.WhiteRooks))
                hash ^= _table[(int)Piece.WhiteRook, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.WhiteQueens))
                hash ^= _table[(int)Piece.WhiteQueen, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.WhiteKing))
                hash ^= _table[(int)Piece.WhiteKing, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.BlackPawns))
                hash ^= _table[(int)Piece.BlackPawn, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.BlackKnights))
                hash ^= _table[(int)Piece.BlackKnight, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.BlackBishops))
                hash ^= _table[(int)Piece.BlackBishop, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.BlackRooks))
                hash ^= _table[(int)Piece.BlackRook, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.BlackQueens))
                hash ^= _table[(int)Piece.BlackQueen, (int)square];

            foreach (Square square in Bitboards.ToSquares(board.BlackKing))
                hash ^= _table[(int)Piece.BlackKing, (int)square];


            if (state.Turn == Color.White)
                hash ^= _whiteToMove;

            hash ^= _castlingRights[(int)state.CastlingRights];

            if (state.EnPassantSquare.HasValue)
                hash ^= _enPassantFile[(int)Files.Of((Square)state.EnPassantSquare)];

            return hash;
        }


        public static void UpdateHash(ref ulong hash, MoveWrapper move, State previousState, State newState)
        {
            Color movedPieceColor = previousState.Turn;

            if (move.Move.IsCastling())
            {
                Square previousRookSquare = Squares.Of(Files.Of(move.Move.To()) == File.C ? File.A : File.H, Ranks.Of(move.Move.To()));
                Square newRookSquare = Squares.Of(Files.Of(move.Move.To()) == File.C ? File.D : File.F, Ranks.Of(move.Move.To()));

                hash ^= _table[(int)Pieces.GetPiece(PieceType.Rook, movedPieceColor), (int)previousRookSquare];  // Remove rook
                hash ^= _table[(int)Pieces.GetPiece(PieceType.Rook, movedPieceColor), (int)newRookSquare];  // Add king

                hash ^= _table[(int)Pieces.GetPiece(PieceType.King, movedPieceColor), (int)move.Move.From()];  // Remove king
                hash ^= _table[(int)Pieces.GetPiece(PieceType.King, movedPieceColor), (int)move.Move.To()];  // Add king
            }

            else
            {
                hash ^= _table[(int)Pieces.GetPiece(move.PieceType, movedPieceColor), (int)move.Move.From()];  // Remove moved piece
                hash ^= _table[(int)Pieces.GetPiece(move.PieceType, movedPieceColor), (int)move.Move.To()];  // Add moved piece to new location

                if (move.Move.IsEnPassant())
                {
                    Square capturedPawnSquare = move.Move.To() + (movedPieceColor == Color.White ? -8 : 8);
                    hash ^= _table[(int)Pieces.GetPiece(PieceType.Pawn, movedPieceColor ^ Color.Black), (int)capturedPawnSquare];
                }
                else if (move.Move.IsPromotion())
                {
                    PieceType promotionPiece = move.Move.PromotionPieceType();
                    hash ^= _table[(int)Pieces.GetPiece(PieceType.Pawn, movedPieceColor), (int)move.Move.To()];
                    hash ^= _table[(int)Pieces.GetPiece(promotionPiece, movedPieceColor), (int)move.Move.To()];
                }

                if (move.EnemyPieceType != PieceType.None && !move.Move.IsEnPassant())
                    hash ^= _table[(int)Pieces.GetPiece(move.EnemyPieceType, movedPieceColor ^ Color.Black), (int)move.Move.To()];  // Remove captured piece
            }

            hash ^= _whiteToMove;

            if (previousState.CastlingRights != newState.CastlingRights)
            {
                hash ^= _castlingRights[(int)previousState.CastlingRights];
                hash ^= _castlingRights[(int)newState.CastlingRights];
            }

            if (previousState.EnPassantSquare != newState.EnPassantSquare)
            {
                if (previousState.EnPassantSquare.HasValue) hash ^= _enPassantFile[(int)Files.Of((Square)previousState.EnPassantSquare)];
                if (newState.EnPassantSquare.HasValue) hash ^= _enPassantFile[(int)Files.Of((Square)newState.EnPassantSquare)];
            }
        }
    }
}