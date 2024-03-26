using Core;
using Types;
namespace Search
{
    public class MoveOrderer
    {
        private readonly int MAX_DEPTH = 9;
        private readonly MoveWrapper[,] _killerMoves;

        public MoveOrderer()
        {
            _killerMoves = new MoveWrapper[2, MAX_DEPTH];
        }

        public void OrderMoves(List<MoveWrapper> moves, int depth)
        {
            PrioritizeKillerMoves(moves, depth);
            moves.Sort((a, b) => ScoreMove(b) - ScoreMove(a));
        }

        public static void OrderMoves(List<MoveWrapper> moves)
        {
            moves.Sort((a, b) => ScoreMove(b) - ScoreMove(a));
        }

        private void PrioritizeKillerMoves(List<MoveWrapper> moves, int depth)
        {
            MoveWrapper killerMove1 = _killerMoves[0, depth];
            MoveWrapper killerMove2 = _killerMoves[1, depth];

            if (moves.Contains(killerMove1))
            {
                moves.Remove(killerMove1);
                moves.Insert(0, killerMove1);
            }

            if (moves.Contains(killerMove2))
            {
                moves.Remove(killerMove2);
                moves.Insert(moves.IndexOf(killerMove1) + 1, killerMove2);
            }
        }

        private static int ScoreMove(MoveWrapper move)
        {
            int score = 0;

            if (move.EnemyPieceType != PieceType.None)
                score += GetPieceValue(move.EnemyPieceType) - GetPieceValue(move.PieceType) / 5;

            if (move.Move.IsCastling())
                score += 25;


            return score;
        }

        public void UpdateKillerMoves(MoveWrapper move, int depth)
        {
            _killerMoves[1, depth] = _killerMoves[0, depth];
            _killerMoves[0, depth] = move;
        }

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
    }
}