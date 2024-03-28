using Core;
using Types;
using System.Diagnostics;

namespace Search
{
    public static class Perft
    {
        public static (ulong, List<(MoveWrapper, ulong)>) PerftMoves(Position position, int depth, out int elapsedMilliseconds)
        {
            List<(MoveWrapper, ulong)> moveCounts = new();
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (var move in position.AllMoves())
            {
                position.PerformMove(move);

                if (!position.Board.MovePutsKingInCheck(position.State.Turn ^ Color.Black, move))
                    moveCounts.Add((move, RunPerft(position, depth - 1)));

                position.UndoMove(move);
            }
            elapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;
            ulong totalPositions = 0UL;
            Clear();
            foreach (var move in moveCounts) totalPositions += move.Item2;
            return (totalPositions, moveCounts);
        }

        public static ulong RunPerft(Position position, int depth, out int elapsedMilliseconds)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            ulong nodesSearched = 0UL;
            foreach (var move in position.AllMoves())
            {
                position.PerformMove(move);

                if (!position.Board.MovePutsKingInCheck(position.State.Turn ^ Color.Black, move))
                    nodesSearched += RunPerft(position, depth - 1);

                position.UndoMove(move);
            }
            elapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;
            Clear();
            return nodesSearched;
        }


        public static ulong RunPerft(Position position, int depth)
        {
            if (depth == 0)
                return 1;

            if (PerftTranspositionTable.TryGet(position.ZobristKey, depth, out ulong count))
                return count;

            ulong numPositions = 0;

            foreach (var move in position.AllMoves())
            {
                position.PerformMove(move);

                if (!position.Board.MovePutsKingInCheck(position.State.Turn ^ Color.Black, move))
                    numPositions += RunPerft(position, depth - 1);

                position.UndoMove(move);
            }

            PerftTranspositionTable.Store(position.ZobristKey, depth, numPositions);
            return numPositions;
        }




        public static void Clear()
        {
            PerftTranspositionTable.Clear();
        }

    }
}