using Core;
using Search;

namespace UCI
{
    public static class UciStdInputThread
    {
        public static event EventHandler<string>? CommandReceived;
        private static bool _running;


        public static void StartAcceptingInput()
        {
            _running = true;
            while (_running)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;
                CommandReceived?.Invoke(null, input);
                if (input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    _running = false;
                }
            }
        }

        public static void Stop() => _running = false;
    }

    public static class UniversalChessInterface
    {
        private const string Name = "Eric";
        private const string Author = "EricAnderson85";
        private static Position _position = new();

        public static void Main(string[] args)
        {
            LaunchUci();
        }

        private static void LaunchUci()
        {
            UciStdInputThread.CommandReceived += UciCommandHandler;
            Thread inputThread = new(UciStdInputThread.StartAcceptingInput);
            inputThread.Start();
        }

        private static void UciCommandHandler(object? sender, string command)
        {
            switch (command.Split(' ')[0].ToLower())
            {
                case "uci":
                    Console.WriteLine($"id name {Name}");
                    Console.WriteLine($"id author {Author}");
                    Console.WriteLine("option name OwnBook type check default ");
                    Console.WriteLine("uciok");
                    break;

                case "isready":
                    Console.WriteLine("readyok");
                    break;

                case "ucinewgame":
                    _position = new();
                    break;

                case "position":
                    HandlePosition(command);
                    break;

                case "go":
                    HandleGo(command);
                    break;

                case "clear":
                    TranspositionTable.Clear();
                    MoveOrderer.Clear();
                    PerftTranspositionTable.Clear();
                    break;

                case "d":
                    Console.WriteLine(_position);
                    break;

                case "score":
                    Console.WriteLine(Evaluate.Evaluation(_position));
                    break;

                case "turn":
                    Console.WriteLine(_position.State.Turn);
                    break;

                case "quit":
                    UciStdInputThread.Stop();
                    Environment.Exit(0);
                    break;
            }
        }

        private static void HandlePosition(string command)
        {
            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 1)
            {
                switch (parts[1].ToLower())
                {
                    case "fen":
                        int movesIndex = Array.FindIndex(parts, x => x.ToLower() == "moves");

                        string fen = movesIndex != -1 ? string.Join(" ", parts.Skip(2).Take(movesIndex - 2)) : string.Join(" ", parts.Skip(2));
                        _position = new Position(fen);

                        if (movesIndex != -1)
                        {
                            var moves = parts.Skip(movesIndex + 1);
                            foreach (string move in moves)
                            {
                                _position.PerformMove(MoveWrapper.CreateMove(_position, move));
                            }
                        }
                        break;

                    case "startpos":
                        _position = new Position();

                        if (parts.Length > 2 && parts[2].ToLower() == "moves")
                            foreach (var move in parts.Skip(3))
                                _position.PerformMove(MoveWrapper.CreateMove(_position, move));

                        break;

                    default:
                        Console.WriteLine("Incorrect position");
                        break;
                }
            }
        }



        private static void HandleGo(string command)
        {
            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int depth = 7;
            bool perftRequested = false;
            int perftDepth = 1;

            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i].ToLower() == "depth" && i + 1 < parts.Length && int.TryParse(parts[i + 1], out int parsedDepth))
                {
                    depth = parsedDepth;
                    i++;
                }
                else if (parts[i].ToLower() == "perft")
                {
                    perftRequested = true;

                    if (i + 1 < parts.Length && int.TryParse(parts[i + 1], out int parsedPerftDepth))
                    {
                        perftDepth = parsedPerftDepth;
                        i++;
                    }

                }

            }

            if (perftRequested)
            {
                Console.WriteLine();

                var nodesSearched = Perft.RunPerft(_position, perftDepth, out int elapsedMilliseconds);

                Console.WriteLine($"\n{nodesSearched:N0} nodes searched in {elapsedMilliseconds / 1000.0} seconds");
                Console.WriteLine($"{nodesSearched / (elapsedMilliseconds / 1000.0):N0} nodes per second");
            }
            else
            {
                var move = Searcher.StartSearch(_position, depth);
                Console.WriteLine("bestmove " + move);
            }
        }


    }
}
