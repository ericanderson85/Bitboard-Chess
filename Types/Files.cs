using Chess;

namespace Types
{
    public enum File
    {
        FileA,
        FileB,
        FileC,
        FileD,
        FileE,
        FileF,
        FileG,
        FileH
    };
    public static class Files
    {
        public const ulong FileABitBoard = 0x0101010101010101;
        public const ulong FileBBitBoard = FileABitBoard << (8 * 1);
        public const ulong FileCBitBoard = FileABitBoard << (8 * 2);
        public const ulong FileDBitBoard = FileABitBoard << (8 * 3);
        public const ulong FileEBitBoard = FileABitBoard << (8 * 4);
        public const ulong FileFBitBoard = FileABitBoard << (8 * 5);
        public const ulong FileGBitBoard = FileABitBoard << (8 * 6);
        public const ulong FileHBitBoard = FileABitBoard << (8 * 7);
        public static File FileOf(Square square)
        {
            return (File)((int)square & 7);
        }
        public static bool InRange(File file)
        {
            return file >= File.FileA && file <= File.FileH;
        }
        public static Bitboard BitBoard(File file)
        {
            return new(FileABitBoard << (int)file);
        }

    }
}