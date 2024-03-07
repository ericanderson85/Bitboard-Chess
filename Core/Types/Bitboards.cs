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
            return Files.BitboardA << (int)file;
        }
        public static ulong Of(Rank rank)
        {
            return Ranks.BitboardOne << (8 * (int)rank);
        }

        public static ulong Of(File file, Rank rank)
        {
            return Of(Squares.Of(file, rank));
        }

        public static ulong East(ulong bitboard)
        {
            return (bitboard << 1) & ~Files.BitboardA;
        }

        public static ulong West(ulong bitboard)
        {
            return (bitboard >> 1) & ~Files.BitboardH;
        }

        public static void Print(ulong bitboard)
        {
            char piece;
            Console.WriteLine("+---+---+---+---+---+---+---+---+");
            for (Rank rank = Rank.Eight; rank >= Rank.One; rank--)
            {
                for (File file = File.A; file <= File.H; file++)
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