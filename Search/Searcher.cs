using Types;
using Core;
namespace Search
{
    public static class Searcher
    {
        private static readonly MoveOrderer _moveOrderer = new();

        public static MoveWrapper FindBestMove(Position P, int depth)
        {
            bool whiteToMove = P.State.Turn == Color.White;
            int bestValue = whiteToMove ? int.MinValue : int.MaxValue;
            var moves = P.AllMoves();

            _moveOrderer.OrderMoves(moves, depth);

            MoveWrapper bestMove = moves[0];
            int alpha = int.MinValue;
            int beta = int.MaxValue;


            foreach (var move in moves)
            {
                P.PerformMove(move);
                if (P.Board.PutsKingInCheck(P.State.Turn, move))
                {
                    P.UndoMove(move);
                    continue;
                }
                int value = Search(P, depth - 1, alpha, beta);
                P.UndoMove(move);

                if ((whiteToMove && value > beta) || (!whiteToMove && value < alpha))
                {
                    _moveOrderer.UpdateKillerMoves(move, depth);
                }


                if (whiteToMove)
                {
                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestMove = move;
                    }
                    alpha = Math.Max(alpha, value);
                }
                else
                {
                    if (value < bestValue)
                    {
                        bestValue = value;
                        bestMove = move;
                    }
                    beta = Math.Min(beta, value);
                }

                if (beta <= alpha)
                {
                    break;
                }
            }

            return bestMove;
        }

        private static int Search(Position P, int depth, int alpha, int beta)
        {
            if (TranspositionTable.TryGet(P.ZobristKey, depth, out int evaluation, out Bounds bound))
            {
                if (bound == Bounds.Exact) return evaluation;
                if (bound == Bounds.Lower) alpha = int.Max(alpha, evaluation);
                else if (bound == Bounds.Upper) beta = int.Min(beta, evaluation);
                if (alpha >= beta) return evaluation;
            }

            if (depth == 0)
            {
                return Evaluate.Evaluation(P);
            }

            int alphaOriginal = alpha;
            bool whiteToMove = P.State.Turn == Color.White;
            int bestScore = whiteToMove ? int.MinValue : int.MaxValue;
            var moves = P.AllMoves();

            _moveOrderer.OrderMoves(moves, depth);

            foreach (var move in moves)
            {
                P.PerformMove(move);
                if (P.Board.PutsKingInCheck(P.State.Turn, move))
                {
                    P.UndoMove(move);
                    continue;
                }
                int score = Search(P, depth - 1, alpha, beta);
                P.UndoMove(move);

                if (whiteToMove)
                {
                    bestScore = int.Max(bestScore, score);
                    alpha = int.Max(alpha, score);
                }
                else
                {
                    bestScore = int.Min(bestScore, score);
                    beta = int.Min(beta, score);
                }

                if (beta <= alpha)
                    break;
            }

            Bounds bounds = Bounds.Exact;
            if (bestScore <= alphaOriginal) bounds = Bounds.Upper;
            else if (bestScore >= beta) bounds = Bounds.Lower;

            TranspositionTable.Store(P.ZobristKey, depth, bestScore, bounds);
            return bestScore;
        }

        private static int QuietSearch(Position P, int alpha, int beta)
        {
            bool whiteToMove = P.State.Turn == Color.White;
            int bestScore = whiteToMove ? int.MinValue : int.MaxValue;
            var loudMoves = P.LoudMoves();

            MoveOrderer.OrderMoves(loudMoves);

            foreach (var move in loudMoves)
            {
                P.PerformMove(move);
                if (P.Board.PutsKingInCheck(P.State.Turn, move))
                {
                    P.UndoMove(move);
                    continue;
                }
                int score = QuietSearch(P, alpha, beta);
                P.UndoMove(move);

                if (whiteToMove)
                {
                    bestScore = int.Max(bestScore, score);
                    alpha = int.Max(alpha, score);
                }
                else
                {
                    bestScore = int.Min(bestScore, score);
                    beta = int.Min(beta, score);
                }

                if (beta <= alpha)
                    break;
            }

            return bestScore;
        }
    }
}