using Core;
using Types;
namespace Search
{
    public static class Evaluate
    {
        public const int MATE_SCORE = 300000;
        public const int PAWN_VALUE = 100;
        public const int KNIGHT_VALUE = 320;
        public const int BISHOP_VALUE = 330;
        public const int ROOK_VALUE = 500;
        public const int QUEEN_VALUE = 900;
        public const int KING_VALUE = 0;

        public const int END_GAME_THRESHOLD = ROOK_VALUE + ROOK_VALUE + BISHOP_VALUE + KNIGHT_VALUE;

        public static readonly int[] PAWN_TABLE = new int[]{
            0, 0, 0, 0, 0, 0, 0, 0,
            5, 10, 10, -20, -20, 10, 10, 5,
            5, -5, -10, 0, 0, -10, -5, 5,
            0, 0, 0, 40, 40, 0, 0, 0,
            5, 5, 10, 45, 45, 10, 5, 5,
            10, 10, 35, 50, 50, 35, 10, 10,
            50, 50, 55, 55, 55, 55, 50, 50,
            0, 0, 0, 0, 0, 0, 0, 0,
        };

        public static readonly int[] PAWN_END_GAME_TABLE = new int[]{
            0, 0, 0, 0, 0, 0, 0, 0,
            -10, -10, -10, -10, -10, -10, -10, -10,
            0, 0, 0, 0, 0, 0, 0, 0,
            25, 25, 25, 25, 25, 25, 25, 25,
            40, 40, 40, 40, 40, 40, 40, 40,
            55, 55, 55, 55, 55, 55, 55, 55,
            70, 70, 70, 70, 70, 70, 70, 70,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        public static readonly int[] BISHOP_TABLE = new int[]{
            -20, -10, -10, -10, -10, -10, -10, -20,
            -10, 5, 0, 0, 0, 0, 5, -10,
            -10, 10, 10, 10, 10, 10, 10, -10,
            -10, 0, 10, 10, 10, 10, 0, -10,
            -10, 5, 5, 10, 10, 5, 5, -10,
            -10, 0, 5, 10, 10, 5, 0, -10,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -20, -10, -10, -10, -10, -10, -10, -20,
        };
        public static readonly int[] ROOK_TABLE = new int[]{
            0, 0, 0, 5, 5, 0, 0, 0,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            5, 10, 10, 10, 10, 10, 10, 5,
            0, 0, 0, 0, 0, 0, 0, 0,
        };
        public static readonly int[] KNIGHT_TABLE = new int[]{
            -50, -40, -30, -30, -30, -30, -40, -50,
            -40, -20, 0, 5, 5, 0, -20, -40,
            -30, 5, 10, 15, 15, 10, 5, -30,
            -30, 0, 15, 20, 20, 15, 0, -30,
            -30, 5, 15, 20, 20, 15, 5, -30,
            -30, 0, 10, 15, 15, 10, 0, -30,
            -40, -20, 0, 0, 0, 0, -20, -40,
            -50, -40, -30, -30, -30, -30, -40, -50,
        };
        public static readonly int[] QUEEN_TABLE = new int[]{
            -20, -10, -10, -5, -5, -10, -10, -20,
            -10, 0, 5, 0, 0, 0, 0, -10,
            -10, 5, 5, 5, 5, 5, 0, -10,
            0, 0, 5, 5, 5, 5, 0, -5,
            -5, 0, 5, 5, 5, 5, 0, -5,
            -10, 0, 5, 5, 5, 5, 0, -10,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -20, -10, -10, -5, -5, -10, -10, -20,
        };
        public static readonly int[] KING_TABLE = new int[]{
            30, 65, 15, -10, -5, 15, 65, 30,
            20, 20, 0, 0, 0, 0, 20, 20,
            -10, -20, -20, -20, -20, -20, -20, -10,
            -20, -30, -30, -40, -40, -30, -30, -20,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
        };
        public static readonly int[] KING_END_GAME_TABLE =
        {
            -50, -30, -30, -30, -30, -30, -30, -50,
            -30, -25,   0,   0,   0,   0, -25, -30,
            -25, -20,  20,  25,  25,  20, -20, -25,
            -20, -15,  30,  40,  40,  30, -15, -20,
            -15, -10,  35,  45,  45,  35, -10, -15,
            -10, -5,   20,  30,  30,  20,  -5, -10,
            -5,   0,   5,   5,   5,   5,   0,  -5,
            -20, -10, -10, -10, -10, -10, -10, -20
        };

        public static int Evaluation(Position position)
        {
            Board board = position.Board;
            int whiteSquareEvaluation = 0, blackSquareEvaluation = 0;

            whiteSquareEvaluation += PieceSquareTableEvaluation(board.WhiteKnights, KNIGHT_TABLE, true, out int whiteKnightsCount);
            whiteSquareEvaluation += PieceSquareTableEvaluation(board.WhiteBishops, BISHOP_TABLE, true, out int whiteBishopsCount);
            whiteSquareEvaluation += PieceSquareTableEvaluation(board.WhiteRooks, ROOK_TABLE, true, out int whiteRooksCount);
            whiteSquareEvaluation += PieceSquareTableEvaluation(board.WhiteQueens, QUEEN_TABLE, true, out int whiteQueensCount);

            blackSquareEvaluation += PieceSquareTableEvaluation(board.BlackKnights, KNIGHT_TABLE, false, out int blackKnightsCount);
            blackSquareEvaluation += PieceSquareTableEvaluation(board.BlackBishops, BISHOP_TABLE, false, out int blackBishopsCount);
            blackSquareEvaluation += PieceSquareTableEvaluation(board.BlackRooks, ROOK_TABLE, false, out int blackRooksCount);
            blackSquareEvaluation += PieceSquareTableEvaluation(board.BlackQueens, QUEEN_TABLE, false, out int blackQueensCount);

            int whiteMaterialScore = whiteKnightsCount * KNIGHT_VALUE
                                   + whiteBishopsCount * BISHOP_VALUE
                                   + whiteRooksCount * ROOK_VALUE
                                   + whiteQueensCount * QUEEN_VALUE;

            int blackMaterialScore = blackKnightsCount * KNIGHT_VALUE
                                   + blackBishopsCount * BISHOP_VALUE
                                   + blackRooksCount * ROOK_VALUE
                                   + blackQueensCount * QUEEN_VALUE;

            double whiteEndgamePhaseWeight = EndgamePhaseWeight(whiteMaterialScore);
            double blackEndgamePhaseWeight = EndgamePhaseWeight(blackMaterialScore);

            whiteSquareEvaluation += (int)((1 - whiteEndgamePhaseWeight) * PieceSquareTableEvaluation(board.WhitePawns, PAWN_TABLE, true, out int whitePawnsCount));
            whiteSquareEvaluation += (int)((1 - whiteEndgamePhaseWeight) * PieceSquareTableEvaluation(board.WhiteKing, KING_TABLE, true, out int _));
            whiteSquareEvaluation += (int)(whiteEndgamePhaseWeight * PieceSquareTableEvaluation(board.WhitePawns, PAWN_END_GAME_TABLE, true, out int _));
            whiteSquareEvaluation += (int)(whiteEndgamePhaseWeight * PieceSquareTableEvaluation(board.WhiteKing, KING_END_GAME_TABLE, true, out int _));

            blackSquareEvaluation += (int)((1 - blackEndgamePhaseWeight) * PieceSquareTableEvaluation(board.BlackPawns, PAWN_TABLE, false, out int blackPawnsCount));
            blackSquareEvaluation += (int)((1 - blackEndgamePhaseWeight) * PieceSquareTableEvaluation(board.BlackKing, KING_TABLE, false, out int _));
            blackSquareEvaluation += (int)(blackEndgamePhaseWeight * PieceSquareTableEvaluation(board.BlackPawns, PAWN_END_GAME_TABLE, false, out int _));
            blackSquareEvaluation += (int)(blackEndgamePhaseWeight * PieceSquareTableEvaluation(board.BlackKing, KING_END_GAME_TABLE, false, out int _));

            whiteMaterialScore += whitePawnsCount * PAWN_VALUE;
            blackMaterialScore += blackPawnsCount * PAWN_VALUE;

            int evaluation = whiteSquareEvaluation + whiteMaterialScore - blackSquareEvaluation - blackMaterialScore;

            return evaluation;
        }

        public static int PieceSquareTableEvaluation(ulong pieces, int[] positionTable, bool isWhite, out int pieceCount)
        {
            int evaluation = 0;
            pieceCount = 0;

            while (pieces != 0)
            {
                Square square = Bitboards.LSB(pieces);
                pieces &= pieces - 1;
                evaluation += positionTable[(int)Squares.RelativeSquare(isWhite ? Color.White : Color.Black, square)];

                pieceCount++;
            }

            return evaluation;
        }

        private static double EndgamePhaseWeight(int materialCountWithoutPawns)
        {
            return 1 - Math.Min(1, materialCountWithoutPawns / (double)END_GAME_THRESHOLD);
        }

        public static int[] TableOf(PieceType pieceType)
        {
            return pieceType switch
            {
                PieceType.Pawn => PAWN_TABLE,
                PieceType.Knight => KNIGHT_TABLE,
                PieceType.Bishop => BISHOP_TABLE,
                PieceType.Rook => ROOK_TABLE,
                PieceType.Queen => QUEEN_TABLE,
                _ => KING_TABLE
            };
        }
    }
}