namespace Types
{
    public enum Rank
    {
        Rank1,
        Rank2,
        Rank3,
        Rank4,
        Rank5,
        Rank6,
        Rank7,
        Rank8
    };
    public static class Ranks
    {

        public const ulong Rank1BitBoard = 0xFF;
        public const ulong Rank2BitBoard = Rank1BitBoard << (8 * 1);
        public const ulong Rank3BitBoard = Rank1BitBoard << (8 * 2);
        public const ulong Rank4BitBoard = Rank1BitBoard << (8 * 3);
        public const ulong Rank5BitBoard = Rank1BitBoard << (8 * 4);
        public const ulong Rank6BitBoard = Rank1BitBoard << (8 * 5);
        public const ulong Rank7BitBoard = Rank1BitBoard << (8 * 6);
        public const ulong Rank8BitBoard = Rank1BitBoard << (8 * 7);

        public static Rank Of(Square square)
        {
            return (Rank)((int)square >> 3);
        }
        public static Rank RelativeRank(Color color, Rank rank)
        {
            return (Rank)((int)rank ^ ((int)color * 7));
        }
        public static Rank RelativeRank(Color color, Square square)
        {
            return RelativeRank(color, Of(square));
        }
        public static bool InRange(Rank rank)
        {
            return rank >= Rank.Rank1 && rank <= Rank.Rank8;
        }

    }
}