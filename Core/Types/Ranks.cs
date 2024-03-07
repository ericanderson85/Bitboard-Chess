namespace Types
{
    public enum Rank
    {
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight
    };
    public static class Ranks
    {

        public const ulong BitboardOne = 0xFF;
        public const ulong BitboardTwo = BitboardOne << (8 * 1);
        public const ulong BitboardThree = BitboardOne << (8 * 2);
        public const ulong BitboardFour = BitboardOne << (8 * 3);
        public const ulong BitboardFive = BitboardOne << (8 * 4);
        public const ulong BitboardSix = BitboardOne << (8 * 5);
        public const ulong BitboardSeven = BitboardOne << (8 * 6);
        public const ulong BitboardEight = BitboardOne << (8 * 7);

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
            return rank >= Rank.One && rank <= Rank.Eight;
        }

    }
}