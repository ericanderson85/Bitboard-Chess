namespace Types
{
    public static class Bitboards
    {
        public static ulong Of(Square square)
        {
            return 1UL << (int)square;
        }

        public static ulong Of(File file)
        {
            return Files.FileABitBoard << (int)file;
        }
        public static ulong Of(Rank rank)
        {
            return Ranks.Rank1BitBoard << (8 * (int)rank);
        }

        public static ulong Of(File file, Rank rank)
        {
            return Of(Squares.Of(file, rank));
        }

        public static void Print(ulong bitboard)
        {
            char piece;
            Console.WriteLine("+---+---+---+---+---+---+---+---+");
            for (Rank rank = Rank.Rank8; rank >= Rank.Rank1; rank--)
            {
                for (File file = File.FileA; file <= File.FileH; file++)
                {
                    piece = (bitboard & Of(file, rank)) != 0 ? 'X' : ' ';


                    Console.Write("| " + piece + " ");
                }
                Console.WriteLine("| " + ((int)rank + 1) + "\n+---+---+---+---+---+---+---+---+");
            }
            Console.WriteLine("  a   b   c   d   e   f   g   h");
        }
    }
}