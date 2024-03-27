using Core;

namespace Book
{
    public class OpeningBook
    {
        private readonly Dictionary<string, BookMove[]> _movesByPosition;
        private static readonly Random _randomNumberGenerator = new();

        public OpeningBook(string openingBookFileName = "OpeningBook.txt")
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "src", "OpeningBook", openingBookFileName);

            string openingBook = File.ReadAllText(filePath);

            Span<string> entries = openingBook.Trim(new char[] { ' ', '\n' }).Split("pos").AsSpan(1);
            _movesByPosition = new Dictionary<string, BookMove[]>(entries.Length);


            for (int i = 0; i < entries.Length; i++)
            {
                string[] entryData = entries[i].Trim('\n').Split('\n');
                string positionFen = entryData[0].Trim();
                Span<string> allMoveData = entryData.AsSpan(1);

                BookMove[] bookMoves = new BookMove[allMoveData.Length];

                for (int moveIndex = 0; moveIndex < bookMoves.Length; moveIndex++)
                {
                    string[] moveData = allMoveData[moveIndex].Split(' ');
                    bookMoves[moveIndex] = new BookMove(moveData[0], uint.Parse(moveData[1]));
                }

                _movesByPosition.Add(positionFen, bookMoves);
            }
        }

        public bool HasBookMove(string positionFen)
        {
            return _movesByPosition.ContainsKey(ProcessFen(positionFen));
        }

        // weightInfluence is a value between 0 and 1.
        // 0 means all moves are picked with equal probablity, 1 means moves are weighted by num times played.
        public bool TryGetBookMove(Position position, out MoveWrapper bookMove, double weightInfluence = 0.75)
        {
            string positionFen = position.ToFEN();
            weightInfluence = Math.Clamp(weightInfluence, 0, 1);


            if (_movesByPosition.TryGetValue(ProcessFen(positionFen), out BookMove[]? moves))
            {
                uint totalWeight = 0;
                foreach (BookMove move in moves)
                {
                    totalWeight += (uint)Math.Ceiling(Math.Pow(move.Weight, weightInfluence));
                }

                double[] weights = new double[moves.Length];
                double weightSum = 0;
                for (int i = 0; i < moves.Length; i++)
                {
                    double weight = Math.Ceiling(Math.Pow(moves[i].Weight, weightInfluence));
                    weightSum += weight;
                    weights[i] = weight;
                }

                double[] probCumul = new double[moves.Length];
                for (int i = 0; i < weights.Length; i++)
                {
                    double prob = weights[i] / weightSum;
                    probCumul[i] = probCumul[Math.Max(0, i - 1)] + prob;
                }


                double random = _randomNumberGenerator.NextDouble();
                for (int i = 0; i < moves.Length; i++)
                {

                    if (random <= probCumul[i])
                    {
                        bookMove = MoveWrapper.CreateMove(position, moves[i].MoveString);
                        return true;
                    }
                }
            }
            bookMove = new();  // Placeholder
            return false;
        }

        public static string ProcessFen(string fen)
        {
            var parts = fen.Split(' ');

            parts[3] = "-";  // Negate en passant square

            var result = string.Join(" ", parts, 0, 4);  // Remove move counter and half move clock

            return result;
        }
    }
}
