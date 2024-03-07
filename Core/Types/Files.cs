namespace Types
{
    public enum File
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H
    };
    public static class Files
    {
        public const ulong BitboardA = 0x0101010101010101;
        public const ulong BitboardB = BitboardA << (8 * 1);
        public const ulong BitboardC = BitboardA << (8 * 2);
        public const ulong BitboardD = BitboardA << (8 * 3);
        public const ulong BitboardE = BitboardA << (8 * 4);
        public const ulong BitboardF = BitboardA << (8 * 5);
        public const ulong BitboardG = BitboardA << (8 * 6);
        public const ulong BitboardH = BitboardA << (8 * 7);
        public static File Of(Square square)
        {
            return (File)((int)square & 7);
        }
        public static bool InRange(File file)
        {
            return file >= File.A && file <= File.H;
        }
    }
}