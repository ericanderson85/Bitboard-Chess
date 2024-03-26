using System.Numerics;
using Core;
using Types;
namespace Search
{
    public static class Evaluate
    {
        public const int PAWN_VALUE = 100;
        public const int KNIGHT_VALUE = 320;
        public const int BISHOP_VALUE = 330;
        public const int ROOK_VALUE = 500;
        public const int QUEEN_VALUE = 900;
        public const int KING_VALUE = 0;
        public const int CASTLING_RIGHT_VALUE = 25;

        public static readonly int[] PAWN_TABLE = {
            0, 0, 0, 0, 0, 0, 0, 0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5, 5, 10, 25, 25, 10, 5, 5,
            0, 0, 0, 20, 20, 0, 0, 0,
            5, -5, -10, 0, 0, -10, -5, 5,
            5, 10, 10, -20, -20, 10, 10, 5,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        public static readonly int[] KNIGHT_TABLE = {
            -50, -40, -30, -30, -30, -30, -40, -50,
            -40, -20, 0, 0, 0, 0, -20, -40,
            -30, 0, 10, 15, 15, 10, 0, -30,
            -30, 5, 15, 20, 20, 15, 5, -30,
            -30, 0, 15, 20, 20, 15, 0, -30,
            -30, 5, 10, 15, 15, 10, 5, -30,
            -40, -20, 0, 5, 5, 0, -20, -40,
            -50, -40, -30, -30, -30, -30, -40, -50
        };

        public static readonly int[] BISHOP_TABLE = {
            -20, -10, -10, -10, -10, -10, -10, -20,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -10, 0, 5, 10, 10, 5, 0, -10,
            -10, 5, 5, 10, 10, 5, 5, -10,
            -10, 0, 10, 10, 10, 10, 0, -10,
            -10, 10, 10, 10, 10, 10, 10, -10,
            -10, 5, 0, 0, 0, 0, 5, -10,
            -20, -10, -10, -10, -10, -10, -10, -20
        };

        public static readonly int[] ROOK_TABLE = {
            0, 0, 0, 0, 0, 0, 0, 0,
            5, 10, 10, 10, 10, 10, 10, 5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            0, 0, 0, 5, 5, 0, 0, 0
        };


        public static readonly int[] QUEEN_TABLE = {
            -20, -10, -10, -5, -5, -10, -10, -20,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -10, 0, 5, 5, 5, 5, 0, -10,
            -5, 0, 5, 5, 5, 5, 0, -5,
            0, 0, 5, 5, 5, 5, 0, -5,
            -10, 5, 5, 5, 5, 5, 0, -10,
            -10, 0, 5, 0, 0, 0, 0, -10,
            -20, -10, -10, -5, -5, -10, -10, -20
        };

        public static readonly int[] KING_TABLE = {
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -20, -30, -30, -40, -40, -30, -30, -20,
            -10, -20, -20, -20, -20, -20, -20, -10,
            20, 20, 0, 0, 0, 0, 20, 20,
            20, 30, 10, 0, 0, 10, 30, 20
        };

        public static readonly int[] KING_END_GAME_TABLE = {
            -50, -40, -30, -20, -20, -30, -40, -50,
            -30, -20, -10, 0, 0, -10, -20, -30,
            -30, -10, 20, 30, 30, 20, -10, -30,
            -30, -10, 30, 40, 40, 30, -10, -30,
            -30, -10, 30, 40, 40, 30, -10, -30,
            -30, -10, 20, 30, 30, 20, -10, -30,
            -30, -30, 0, 0, 0, 0, -30, -30,
            -50, -30, -30, -30, -30, -30, -30, -50
        };

        public static int Evaluation(Position position)
        {
            Board board = position.Board;
            int whiteEvaluation = 0, blackEvaluation = 0;

            whiteEvaluation += EvaluatePiece(board.WhitePawns, PAWN_VALUE, PAWN_TABLE, true);
            whiteEvaluation += EvaluatePiece(board.WhiteKnights, KNIGHT_VALUE, KNIGHT_TABLE, true);
            whiteEvaluation += EvaluatePiece(board.WhiteBishops, BISHOP_VALUE, BISHOP_TABLE, true);
            whiteEvaluation += EvaluatePiece(board.WhiteRooks, ROOK_VALUE, ROOK_TABLE, true);
            whiteEvaluation += EvaluatePiece(board.WhiteQueens, QUEEN_VALUE, QUEEN_TABLE, true);

            blackEvaluation += EvaluatePiece(board.BlackPawns, PAWN_VALUE, PAWN_TABLE, false);
            blackEvaluation += EvaluatePiece(board.BlackKnights, KNIGHT_VALUE, KNIGHT_TABLE, false);
            blackEvaluation += EvaluatePiece(board.BlackBishops, BISHOP_VALUE, BISHOP_TABLE, false);
            blackEvaluation += EvaluatePiece(board.BlackRooks, ROOK_VALUE, ROOK_TABLE, false);
            blackEvaluation += EvaluatePiece(board.BlackQueens, QUEEN_VALUE, QUEEN_TABLE, false);

            bool endGame = (whiteEvaluation + blackEvaluation) / 2 < 1500;

            whiteEvaluation += EvaluatePiece(board.WhiteKing, KING_VALUE, endGame ? KING_END_GAME_TABLE : KING_TABLE, true);
            blackEvaluation += EvaluatePiece(board.BlackKing, KING_VALUE, endGame ? KING_END_GAME_TABLE : KING_TABLE, false);


            return whiteEvaluation - blackEvaluation + (endGame ? 0 : CastlingRightsEvaluation(position.State.CastlingRights));
        }

        private static int CastlingRightsEvaluation(CastlingRights castlingRights)
        {
            int castlingRightsEvaluation = 0;

            if (castlingRights.HasFlag(CastlingRights.WhiteKingSide))
                castlingRightsEvaluation += CASTLING_RIGHT_VALUE;

            if (castlingRights.HasFlag(CastlingRights.WhiteQueenSide))
                castlingRightsEvaluation += CASTLING_RIGHT_VALUE;

            if (castlingRights.HasFlag(CastlingRights.BlackKingSide))
                castlingRightsEvaluation -= CASTLING_RIGHT_VALUE;

            if (castlingRights.HasFlag(CastlingRights.BlackQueenSide))
                castlingRightsEvaluation -= CASTLING_RIGHT_VALUE;

            return castlingRightsEvaluation;
        }

        private static int EvaluatePiece(ulong pieces, int pieceValue, int[] positionTable, bool isWhite)
        {
            int evaluation = 0;
            while (pieces != 0)
            {
                int squareIndex = BitOperations.TrailingZeroCount(pieces);
                pieces &= pieces - 1;
                evaluation += pieceValue;
                evaluation += isWhite ? positionTable[squareIndex] : positionTable[63 - squareIndex];
            }
            return evaluation;
        }
    }
}