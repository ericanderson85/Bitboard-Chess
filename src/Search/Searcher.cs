using Book;
using Core;
using Types;
namespace Search
{
    public static class Searcher
    {
        private const int MAX_QUIESCENT_SEARCH_DEPTH = 1;
        private static readonly OpeningBook _openingBookReader = new();

        public static MoveWrapper StartSearch(Position position, int depth)
        {
            if (_openingBookReader.TryGetBookMove(position, out MoveWrapper bookMove))
            {
                return bookMove;
            }

            bool isWhiteTurn = position.State.Turn == Color.White;


            return isWhiteTurn ? StartSearchMaximize(position, depth) : StartSearchMinimize(position, depth);
        }

        private static MoveWrapper StartSearchMaximize(Position position, int depth)
        {
            int maxEval = int.MinValue;

            List<MoveWrapper> moves = position.AllMoves();

            MoveOrderer.OrderMoves(position, moves);

            MoveWrapper maxMove = moves[0];
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            foreach (MoveWrapper move in moves)
            {
                position.PerformMove(move);
                if (position.Board.MovePutsKingInCheck(Color.White, move))
                {
                    position.UndoMove(move);
                    continue;
                }

                int eval = SearchMinimize(position, depth - 1, alpha, beta);

                position.UndoMove(move);

                if (eval > maxEval)
                {
                    maxEval = eval;
                    maxMove = move;
                };

                alpha = Math.Max(alpha, eval);
                if (alpha >= beta)
                {
                    break; // Beta cut-off
                }

            }

            return maxMove;
        }

        private static MoveWrapper StartSearchMinimize(Position position, int depth)
        {
            int minEval = int.MaxValue;

            List<MoveWrapper> moves = position.AllMoves();
            MoveWrapper minMove = moves[0];

            MoveOrderer.OrderMoves(position, moves);

            int alpha = int.MinValue;
            int beta = int.MaxValue;

            foreach (MoveWrapper move in moves)
            {
                position.PerformMove(move);
                if (position.Board.MovePutsKingInCheck(Color.Black, move))
                {
                    position.UndoMove(move);
                    continue;
                }

                int eval = SearchMaximize(position, depth - 1, alpha, beta);

                position.UndoMove(move);

                if (eval < minEval)
                {
                    minEval = eval;
                    minMove = move;
                };

                beta = Math.Min(beta, eval);
                if (alpha >= beta)
                {
                    break; // Alpha cut-off
                }
            }

            return minMove;
        }



        private static int SearchMaximize(Position position, int movesRemaining, int alpha, int beta)
        {
            if (TranspositionTable.TryGet(position.ZobristKey, movesRemaining, out int evaluation, out Bounds storedBounds))
            {
                switch (storedBounds)
                {
                    case Bounds.Exact:
                        return evaluation;
                    case Bounds.Lower:
                        alpha = Math.Max(alpha, evaluation);
                        break;
                    case Bounds.Upper:
                        beta = Math.Min(beta, evaluation);
                        break;
                }

                if (alpha >= beta)
                    return evaluation;
            }

            if (movesRemaining == 0)
                return QuiescentSearchMaximize(position, MAX_QUIESCENT_SEARCH_DEPTH, alpha, beta);

            int maxEval = int.MinValue;
            List<MoveWrapper> moves = position.AllMoves();

            MoveOrderer.OrderMoves(position, moves);

            if (moves.Count == 0)
            {
                if (position.Board.KingInCheck(Color.White))
                    return -(Evaluate.MATE_SCORE - movesRemaining);

                return 0;  // Draw
            }


            foreach (MoveWrapper move in moves)
            {
                position.PerformMove(move);
                if (position.Board.MovePutsKingInCheck(Color.White, move))
                {
                    position.UndoMove(move);
                    continue;
                }

                int eval = SearchMinimize(position, movesRemaining - 1, alpha, beta);

                position.UndoMove(move);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                // Move is too good, opponent will avoid this position
                if (alpha >= beta)
                {
                    // MoveOrderer.UpdateKillerMoves(move, movesRemaining);
                    // MoveOrderer.UpdateHistory(Color.White, move.Move.From(), move.Move.To(), movesRemaining);

                    break; // Beta cut-off}
                }
            }

            Bounds bounds;
            if (maxEval <= alpha)
                bounds = Bounds.Upper;
            else if (maxEval >= beta)
                bounds = Bounds.Lower;
            else
                bounds = Bounds.Exact;

            TranspositionTable.Store(position.ZobristKey, movesRemaining, maxEval, bounds);

            return maxEval;
        }

        private static int SearchMinimize(Position position, int movesRemaining, int alpha, int beta)
        {
            if (TranspositionTable.TryGet(position.ZobristKey, movesRemaining, out int evaluation, out Bounds storedBounds))
            {
                switch (storedBounds)
                {
                    case Bounds.Exact:
                        return evaluation;
                    case Bounds.Lower:
                        alpha = Math.Max(alpha, evaluation);
                        break;
                    case Bounds.Upper:
                        beta = Math.Min(beta, evaluation);
                        break;
                }

                if (alpha >= beta)
                    return evaluation;
            }

            if (movesRemaining == 0)
                return QuiescentSearchMinimize(position, MAX_QUIESCENT_SEARCH_DEPTH, alpha, beta);

            int minEval = int.MaxValue;
            List<MoveWrapper> moves = position.AllMoves();

            MoveOrderer.OrderMoves(position, moves);

            if (moves.Count == 0)
            {
                if (position.Board.KingInCheck(Color.Black))
                    return Evaluate.MATE_SCORE - movesRemaining;

                return 0;  // Draw
            }


            foreach (MoveWrapper move in moves)
            {
                position.PerformMove(move);
                if (position.Board.MovePutsKingInCheck(Color.Black, move))
                {
                    position.UndoMove(move);
                    continue;

                }
                int eval = SearchMaximize(position, movesRemaining - 1, alpha, beta);

                position.UndoMove(move);

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);

                // Move is too good, opponent will avoid this position
                if (alpha >= beta)
                {
                    // MoveOrderer.UpdateKillerMoves(move, movesRemaining);
                    // MoveOrderer.UpdateHistory(Color.Black, move.Move.From(), move.Move.To(), movesRemaining);

                    break; // Alpha cut-off}
                }
            }

            Bounds bounds;
            if (minEval <= alpha)
                bounds = Bounds.Upper;
            else if (minEval >= beta)
                bounds = Bounds.Lower;
            else
                bounds = Bounds.Exact;

            TranspositionTable.Store(position.ZobristKey, movesRemaining, minEval, bounds);

            return minEval;
        }

        private static int QuiescentSearchMaximize(Position position, int movesRemaining, int alpha, int beta)
        {
            int standPat = Evaluate.Evaluation(position);

            if (movesRemaining == 0)
                return standPat;

            if (standPat >= beta)
                return standPat;
            if (standPat > alpha)
                alpha = standPat;

            List<MoveWrapper> loudMoves = position.LoudMoves();
            if (loudMoves.Count == 0)
                return standPat;

            MoveOrderer.OrderMoves(position, loudMoves, true);

            int maxEval = standPat;
            foreach (MoveWrapper move in loudMoves)
            {
                position.PerformMove(move);
                if (position.Board.MovePutsKingInCheck(Color.White, move))
                {
                    position.UndoMove(move);
                    continue;

                }
                int eval = QuiescentSearchMinimize(position, alpha, beta, movesRemaining - 1);

                position.UndoMove(move);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                // Move is too good, opponent will avoid this position
                if (alpha >= beta)
                    break; // Alpha cut-off}
            }

            return maxEval;
        }

        private static int QuiescentSearchMinimize(Position position, int movesRemaining, int alpha, int beta)
        {
            int standPat = Evaluate.Evaluation(position);

            if (movesRemaining == 0)
                return standPat;

            if (standPat <= alpha)
                return standPat;
            if (standPat < beta)
                beta = standPat;

            List<MoveWrapper> loudMoves = position.LoudMoves();
            if (loudMoves.Count == 0)
                return standPat;

            MoveOrderer.OrderMoves(position, loudMoves, true);

            int minEval = int.MaxValue;
            foreach (MoveWrapper move in loudMoves)
            {
                position.PerformMove(move);
                if (position.Board.MovePutsKingInCheck(Color.Black, move))
                {
                    position.UndoMove(move);
                    continue;

                }
                int eval = QuiescentSearchMaximize(position, alpha, beta, movesRemaining - 1);

                position.UndoMove(move);

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);

                // Move is too good, opponent will avoid this position
                if (alpha >= beta)
                    break; // Alpha cut-off}
            }

            return minEval;
        }



        public static void Clear()
        {
            MoveOrderer.Clear();
        }
    }

}