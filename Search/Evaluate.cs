using System.Numerics;
using Core;
using Types;
using File = Types.File;
namespace Search
{
    public static class Evaluate
    {
        public const int PAWN_VALUE = 100;
        public const int KNIGHT_VALUE = 320;
        public const int BISHOP_VALUE = 330;
        public const int ROOK_VALUE = 500;
        public const int QUEEN_VALUE = 900;
        public const int CASTLING_RIGHT_VALUE = 50;

        public static readonly int[] PAWN_TABLE = new int[]{
            0, 0, 0, 0, 0, 0, 0, 0,
            5, 10, 10, -30, -30, 10, 10, 5,
            5, -5, -10, -5, -5, -10, -5, 5,
            0, 0, 0, 40, 40, 0, 0, 0,
            5, 5, 10, 45, 45, 10, 5, 5,
            10, 10, 35, 50, 50, 35, 10, 10,
            50, 50, 55, 55, 55, 55, 50, 50,
            0, 0, 0, 0, 0, 0, 0, 0,
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

        public static int Evaluation(Position position)
        {
            Board board = position.CurrentBoard;
            int evaluation = 0;
            evaluation += EvaluatePiece(board.WhitePawns, PAWN_VALUE, PAWN_TABLE, true) - EvaluatePiece(board.BlackPawns, PAWN_VALUE, PAWN_TABLE, false);
            evaluation += EvaluatePiece(board.WhiteKnights, KNIGHT_VALUE, KNIGHT_TABLE, true) - EvaluatePiece(board.BlackKnights, KNIGHT_VALUE, KNIGHT_TABLE, false);
            evaluation += EvaluatePiece(board.WhiteBishops, BISHOP_VALUE, BISHOP_TABLE, true) - EvaluatePiece(board.BlackBishops, BISHOP_VALUE, BISHOP_TABLE, false);
            evaluation += EvaluatePiece(board.WhiteRooks, ROOK_VALUE, ROOK_TABLE, true) - EvaluatePiece(board.BlackRooks, ROOK_VALUE, ROOK_TABLE, false);
            evaluation += EvaluatePiece(board.WhiteQueens, QUEEN_VALUE, QUEEN_TABLE, true) - EvaluatePiece(board.BlackQueens, QUEEN_VALUE, QUEEN_TABLE, false);

            evaluation += EvaluateKingSafety(board, true) - EvaluateKingSafety(board, false);

            evaluation += CastlingRightsEvaluation(position.CurrentState.CastlingRights);

            return evaluation;
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

        private static int EvaluateKingSafety(Board board, bool isWhite)
        {
            int evaluation = 0;
            Square kingSquare;
            if (isWhite)
            {
                kingSquare = (Square)BitOperations.TrailingZeroCount(board.WhiteKing);
                evaluation += KING_TABLE[(int)kingSquare];

                if (Ranks.Of(kingSquare) <= Rank.Two && Files.Of(kingSquare) != File.E)
                {
                    if ((Bitboards.From(kingSquare + 8) & board.WhitePawns) != 0)
                        evaluation += 12;
                    else if ((Bitboards.From(kingSquare + 16) & board.WhitePawns) != 0)
                        evaluation += 6;
                    else
                        evaluation -= 4;

                    if (Files.Of(kingSquare) != File.A)
                    {
                        if ((Bitboards.From(kingSquare + 7) & board.WhitePawns) != 0)
                            evaluation += 10;
                        else if ((Bitboards.From(kingSquare + 15) & board.WhitePawns) != 0)
                            evaluation += 5;
                    }

                    if (Files.Of(kingSquare) != File.H)
                    {
                        if ((Bitboards.From(kingSquare + 9) & board.WhitePawns) != 0)
                            evaluation += 10;
                        else if ((Bitboards.From(kingSquare + 17) & board.WhitePawns) != 0)
                            evaluation += 5;
                    }
                }
            }

            else
            {
                kingSquare = (Square)BitOperations.TrailingZeroCount(board.BlackKing);
                evaluation += KING_TABLE[63 - (int)kingSquare];

                if (Ranks.Of(kingSquare) >= Rank.Seven && Files.Of(kingSquare) != File.E)
                {
                    if ((Bitboards.From(kingSquare - 8) & board.BlackPawns) != 0)
                        evaluation += 12;
                    else if ((Bitboards.From(kingSquare - 16) & board.BlackPawns) != 0)
                        evaluation += 6;
                    else
                        evaluation -= 4;

                    if (Files.Of(kingSquare) != File.A)
                    {
                        if ((Bitboards.From(kingSquare - 9) & board.BlackPawns) != 0)
                            evaluation += 10;
                        else if ((Bitboards.From(kingSquare - 17) & board.BlackPawns) != 0)
                            evaluation += 5;
                    }

                    if (Files.Of(kingSquare) != File.H)
                    {
                        if ((Bitboards.From(kingSquare - 7) & board.BlackPawns) != 0)
                            evaluation += 10;
                        else if ((Bitboards.From(kingSquare - 15) & board.BlackPawns) != 0)
                            evaluation += 5;
                    }
                }
            }

            return evaluation;
        }
    }
}