namespace Types
{
    public readonly struct Bitboard
    {
        private readonly ulong _data;
        public Bitboard(ulong initialValue)
        {
            this._data = initialValue;
        }
        public Bitboard(Bitboard bitboard)
        {
            this._data = bitboard.Data;
        }

        public ulong Data
        {
            get => _data;
        }

        public static Bitboard operator &(Bitboard bitboard1, Bitboard bitboard2)
        {
            return new(bitboard1.Data & bitboard2.Data);
        }

        public static Bitboard operator &(Bitboard bitboard, Square square)
        {
            return new Bitboard(bitboard & Squares.BitBoard(square));
        }


        public static Bitboard RankBitBoard(Rank rank)
        {
            return new(Ranks.Rank1BitBoard << (8 * (int)rank));
        }
    }
}