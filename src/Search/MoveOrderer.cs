using Core;
using Types;
namespace Search
{
    public static class MoveOrderer
    {
        private const int MILLION = 1000000;
        private const int WINNING_CAPTURE = 8 * MILLION;
        private const int PROMOTION = 6 * MILLION;
        // private const int KILLER = 4 * MILLION;
        private const int LOSING_CAPTURE = 2 * MILLION;

        // private static readonly MoveWrapper?[,] _killerMoves = new MoveWrapper?[2, MAX_DEPTH];
        // private static readonly int[,,] _history = new int[2, 64, 64];

        public static void OrderMoves(Position position, List<MoveWrapper> moves, bool isQuiescentSearch = false)
        {
            moves.Sort((move1, move2) => ScoreMove(position, move2, isQuiescentSearch) - ScoreMove(position, move1, isQuiescentSearch));
        }

        private static int ScoreMove(Position position, MoveWrapper move, bool isQuiescentSearch)
        {
            int score = 0;
            Square from = move.Move.From();
            Square to = move.Move.To();
            Color turn = position.State.Turn;

            PieceType pieceType = move.PieceType;
            PieceType enemyPieceType = move.EnemyPieceType;
            bool opponentPawnCanCapture = (PseudoLegalMoveGenerator.PawnAttacks(position, turn ^ Color.Black) & Bitboards.From(to)) != 0;

            if (move.IsCapture())  // Order moves to try capturing the most valuable opponent piece with least valuable of own pieces first
            {
                int captureMaterialDelta = GetPieceValue(enemyPieceType) - GetPieceValue(pieceType);

                if (opponentPawnCanCapture)
                {
                    score += (captureMaterialDelta >= 0 ? WINNING_CAPTURE : LOSING_CAPTURE) + captureMaterialDelta;
                }
                else
                {
                    score += WINNING_CAPTURE + captureMaterialDelta;
                }
            }
            // else
            // {
            //     bool isKiller = _killerMoves[0, movesRemaining].Equals(move) || _killerMoves[1, movesRemaining].Equals(move);

            //     score += isKiller ? KILLER : 0;

            //     score += _history[(int)turn, (int)from, (int)to];
            // }

            if (move.Move.IsPromotion())
                score += PROMOTION + GetPieceValue(move.Move.PromotionPieceType());

            else
            {
                int[] table = Evaluate.TableOf(pieceType);

                int fromScore = Evaluate.PieceSquareTableEvaluation(Bitboards.From(from), table, turn == Color.White, out int _);
                int toScore = Evaluate.PieceSquareTableEvaluation(Bitboards.From(to), table, turn == Color.White, out int _);

                score += toScore - fromScore;

                if (opponentPawnCanCapture)
                    score -= 50;
            }



            return score;
        }

        // public static void UpdateKillerMoves(MoveWrapper move, int movesRemaining)
        // {
        //     _killerMoves[1, movesRemaining] = _killerMoves[0, movesRemaining];
        //     _killerMoves[0, movesRemaining] = move;
        // }

        // public static void UpdateHistory(Color turn, Square from, Square to, int movesRemaining)
        // {
        //     _history[(int)turn, (int)from, (int)to] += movesRemaining * movesRemaining;
        // }

        private static int GetPieceValue(PieceType pieceType)
        {
            return pieceType switch
            {
                PieceType.Pawn => 100,
                PieceType.Knight => 320,
                PieceType.Bishop => 330,
                PieceType.Rook => 500,
                PieceType.Queen => 900,
                PieceType.King => 20000,
                _ => 0,
            };
        }

        public static void Clear()
        {
            // Array.Clear(_killerMoves);
            // Array.Clear(_history);
        }
    }
}