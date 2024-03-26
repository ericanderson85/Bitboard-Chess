using Types;
using File = Types.File;

namespace Core
{
    public static class AttackTables
    {
        public static readonly ulong[] KnightAttacks = new Func<ulong[]>(() =>
        {
            ulong[] knightAttacks = new ulong[64];
            for (Square square = Square.A1; square <= Square.H8; square++)
            {
                File file = Files.Of(square);
                ulong knight = Bitboards.From(square);
                ulong currentAttacks = 0;
                if (file != File.H) currentAttacks |= knight << 17 | knight >> 15;  // NorthNorthWest and SouthSouthWest
                if (file < File.G) currentAttacks |= knight << 10 | knight >> 6;    // NorthWestWest and SouthWestWest
                if (file != File.A) currentAttacks |= knight << 15 | knight >> 17;  // NorthNorthEast and SouthSouthEast
                if (file > File.B) currentAttacks |= knight << 6 | knight >> 10;    // NorthEastEast and SouthEastEast
                knightAttacks[(int)square] = currentAttacks;
            }
            return knightAttacks;
        })();

        public static readonly ulong[] KingAttacks = new Func<ulong[]>(() =>
        {
            ulong[] kingAttacks = new ulong[64];
            for (Square square = Square.A1; square <= Square.H8; square++)
            {
                ulong king = Bitboards.From(square);
                ulong currentAttacks = Bitboards.East(king) | Bitboards.West(king);
                king |= currentAttacks;
                kingAttacks[(int)square] = currentAttacks |= king >> 8 | king << 8;
            }
            return kingAttacks;
        }
        )();
    }
}