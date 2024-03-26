using System.Numerics;

namespace Types
{
    public static class Bitboards
    {
        public static ulong From(Square square)
        {
            return 1UL << (int)square;
        }
        public static ulong From(File file, Rank rank)
        {
            return From(Squares.Of(file, rank));
        }
        public static ulong From(File file)
        {
            return Files.BitboardA << (int)file;
        }
        public static ulong From(Rank rank)
        {
            return Ranks.BitboardOne << (8 * (int)rank);
        }

        public static ulong East(ulong bitboard)
        {
            return (bitboard << 1) & ~Files.BitboardA;
        }

        public static ulong West(ulong bitboard)
        {
            return (bitboard >> 1) & ~Files.BitboardH;
        }

        public static IEnumerable<ulong> Subsets(ulong state)
        {
            ulong subset = 0;
            do
            {
                yield return subset;
                subset = (subset - state) & state;
            } while (subset != 0);
        }

        public static IEnumerable<Square> ToSquares(ulong bitboard)
        {
            while (bitboard != 0)
            {
                Square lsbSquare = LSB(bitboard);
                yield return lsbSquare;
                bitboard ^= From(lsbSquare);
            }
        }

        public static Square LSB(ulong bitboard)
        {
            return (Square)BitOperations.TrailingZeroCount(bitboard);
        }

        public static void Print(ulong bitboard)
        {
            char piece;
            Console.WriteLine("+---+---+---+---+---+---+---+---+");
            for (Rank rank = Rank.Eight; rank >= Rank.One; rank--)
            {
                for (File file = File.A; file <= File.H; file++)
                {
                    piece = (bitboard & From(file, rank)) != 0 ? 'X' : ' ';


                    Console.Write("| " + piece + " ");
                }
                Console.WriteLine("| " + ((int)rank + 1) + "\n+---+---+---+---+---+---+---+---+");
            }
            Console.WriteLine("  a   b   c   d   e   f   g   h");
        }
    }
}