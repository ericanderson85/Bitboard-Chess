using Types;
using System.Diagnostics;
using Core;



public static class Program
{
    static Position P = new();
    static readonly Random rng = new();
    static ulong _moveCount = 0;
    static readonly Timer Time = new Timer(TimerCallback, null, Timeout.Infinite, 1000);
    static readonly Stopwatch StopWatch = new();

    public static void Main(string[] args)
    {
        P = new("rnbqkbnr/pPpppppp/8/8/8/8/P1PPPPPP/RNBQK2R w KQkq - 0 1");
        Console.WriteLine(P);
        foreach (var move in P.AllMoves())
        {
            P.PerformMove(move);
            Console.WriteLine(P);
            P.UndoMove(move);
        }


        // StopWatch.Start();
        // Time.Change(0, 1000);

        // Move(int.Parse(args[0]));

        // StopWatch.Stop();

        // TimeSpan ts = StopWatch.Elapsed;

        // Console.WriteLine($"Searched {_moveCount:N0} nodes in {(ts.TotalMilliseconds / 1000.0).ToString("N")} seconds");
        // Console.WriteLine($"Nodes per second: {(_moveCount / (StopWatch.ElapsedMilliseconds / 1000.0)).ToString("N0")}");
    }

    private static void TimerCallback(Object? o)
    {
        if (_moveCount == 0) return;
        Console.WriteLine($"Searched {_moveCount:N0} nodes in {StopWatch.ElapsedMilliseconds / 1000} seconds");
        Console.WriteLine(P + "\n");
    }

    public static void Move(int depth)
    {
        if (depth == 0)
        {
            return;
        }

        foreach (MoveWrapper move in P.AllMoves())
        {
            P.PerformMove(move);
            Move(depth - 1);
            _moveCount++;
            P.UndoMove(move);
        }
    }

    public static void MoveDebug(int depth)
    {
        if (depth == 0)
        {
            Console.WriteLine("\nUndoing...\n");
            return;
        }

        var all = P.AllMoves().ToArray();
        var move = all[rng.Next(all.Length)];
        Console.Write($"{P.CurrentState.Turn.ToString()[0]} {move}");
        P.PerformMove(move);
        if (move.EnemyPieceType != PieceType.None) Console.WriteLine($" capturing {move.EnemyPieceType}");
        else Console.WriteLine();

        Console.WriteLine(P);


        MoveDebug(depth - 1);



        P.UndoMove(move);
        Console.WriteLine($"Undoing {move}");
        Console.WriteLine(P);
    }
}
