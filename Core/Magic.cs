using Types;
using File = Types.File;

namespace Core
{
    public readonly struct MagicEntry
    {
        public readonly ulong _mask;
        public readonly ulong _magic;
        public readonly int _indexShift;
        public readonly ulong[] _table;

        public ulong Magic => _magic;

        public MagicEntry(ulong mask, ulong magic, int indexShift, ulong[] table)
        {
            _mask = mask;
            _magic = magic;
            _indexShift = indexShift;
            _table = table;
        }

        public ulong Lookup(ulong allPieces, ulong ownPieces)
        {
            ulong blockers = allPieces & _mask;
            return _table[MagicHashIndex(blockers, _magic, _indexShift)] & ~ownPieces;
        }

        public static int MagicHashIndex(ulong blockers, ulong magic, int indexShift)
        {
            return (int)((blockers * magic) >> indexShift);
        }
    }
    public class Magic
    {
        private readonly MagicEntry[] _rookMagic;
        private readonly MagicEntry[] _bishopMagic;

        private Magic(MagicEntry[] rookMagic, MagicEntry[] bishopMagic)
        {
            _rookMagic = rookMagic;
            _bishopMagic = bishopMagic;
        }

        public static Magic Generate(int indexBits, Random rng)
        {
            var rookMagic = Squares.All.Select(square => FindRookMagic(square, indexBits, rng)).ToArray();
            var bishopMagic = Squares.All.Select(square => FindBishopMagic(square, indexBits, rng)).ToArray();
            return new Magic(rookMagic, bishopMagic);
        }

        public static ulong[] GenerateBishopMagics(int indexBits, Random rng)
        {
            return Squares.All.Select(square => FindBishopMagic(square, indexBits, rng)).Select(m => m.Magic).ToArray();
        }

        public static ulong[] GenerateRookMagics(int indexBits, Random rng)
        {
            return Squares.All.Select(square => FindRookMagic(square, indexBits, rng)).Select(m => m.Magic).ToArray();
        }

        public static Magic Hardcoded()
        {
            var rookMagic = BuildMagicRookTables(RookMagics);
            var bishopMagic = BuildMagicBishopTables(BishopMagics);
            return new(rookMagic, bishopMagic);
        }

        public ulong GetRookMoves(Square square, ulong allPieces, ulong ownPieces) =>
            _rookMagic[(int)square].Lookup(allPieces, ownPieces);

        public ulong GetBishopMoves(Square square, ulong allPieces, ulong ownPieces) =>
            _bishopMagic[(int)square].Lookup(allPieces, ownPieces);


        public static MagicEntry FindRookMagic(Square square, int indexBits, Random rng)
        {
            int indexShift = 64 - indexBits;
            while (true)
            {
                ulong mask = RookMask[(int)square];
                ulong magic = (ulong)(rng.NextInt64() & rng.NextInt64() & rng.NextInt64());
                if (TryMakeMagicTable(PieceType.Rook, square, mask, magic, indexBits, out var table))
                {
                    return new MagicEntry(mask, magic, indexShift, table);
                }
            }
        }

        public static MagicEntry FindBishopMagic(Square square, int indexBits, Random rng)
        {
            int indexShift = 64 - indexBits;
            ulong mask = BishopMask[(int)square];
            while (true)
            {
                ulong magic = (ulong)(rng.NextInt64() & rng.NextInt64() & rng.NextInt64());
                if (TryMakeMagicTable(PieceType.Bishop, square, mask, magic, indexBits, out var table))
                {
                    return new MagicEntry(mask, magic, indexShift, table);
                }
            }
        }

        private static bool TryMakeMagicTable(PieceType pieceType, Square square, ulong mask, ulong magic, int indexBits, out ulong[] table)
        {
            table = new ulong[1 << indexBits];
            int indexShift = 64 - indexBits;

            foreach (ulong blockers in Bitboards.Subsets(mask))
            {
                ulong moves = 0;
                foreach (IReadOnlyList<Square> moveSequence in pieceType == PieceType.Rook ? RookSequences(square) : BishopSequences(square))
                {
                    foreach (Square destination in moveSequence)
                    {
                        ulong destinationBoard = Bitboards.From(destination);
                        moves |= destinationBoard;
                        if ((blockers & destinationBoard) != 0) break;
                    }
                }

                int index = MagicEntry.MagicHashIndex(blockers, magic, indexShift);
                if (table[index] != 0)
                {
                    if (table[index] != moves)
                    {
                        return false;
                    }
                }
                else
                {
                    table[index] = moves;
                }
            }
            return true;
        }

        public static IEnumerable<IReadOnlyList<Square>> BishopSequences(Square square)
        {
            Rank rank = Ranks.Of(square);
            File file = Files.Of(square);

            int[] rankDirections = { -1, -1, 1, 1 };
            int[] fileDirections = { -1, 1, 1, -1 };

            for (int direction = 0; direction < 4; direction++)
            {
                List<Square> sequence = new();
                File newFile = file + fileDirections[direction];
                Rank newRank = rank + rankDirections[direction];

                while (newFile >= File.A && newFile <= File.H && newRank >= Rank.One && newRank <= Rank.Eight)
                {
                    sequence.Add(Squares.Of(newFile, newRank));
                    newRank += rankDirections[direction];
                    newFile += fileDirections[direction];
                }

                yield return sequence;
            }
        }

        public static IEnumerable<IReadOnlyList<Square>> RookSequences(Square square)
        {
            Rank rank = Ranks.Of(square);
            File file = Files.Of(square);

            int[] rankDirections = { 0, 0, 1, -1 };
            int[] fileDirections = { 1, -1, 0, 0 };

            for (int direction = 0; direction < 4; direction++)
            {
                List<Square> sequence = new();
                File newFile = file + fileDirections[direction];
                Rank newRank = rank + rankDirections[direction];

                while (newFile >= File.A && newFile <= File.H && newRank >= Rank.One && newRank <= Rank.Eight)
                {
                    sequence.Add(Squares.Of(newFile, newRank));
                    newRank += rankDirections[direction];
                    newFile += fileDirections[direction];
                }

                yield return sequence;
            }
        }

        private static MagicEntry[] BuildMagicBishopTables(IEnumerable<(int, ulong)> hardcodedMagics)
        {
            List<MagicEntry> entries = new();
            int i = 0;
            foreach (var (indexBits, magic) in hardcodedMagics)
            {
                int indexShift = 64 - indexBits;
                Square square = (Square)i++;
                ulong mask = BishopMask[(int)square];
                if (!TryMakeMagicTable(PieceType.Bishop, square, mask, magic, indexBits, out var tables))
                {
                    continue;
                }

                entries.Add(new MagicEntry(mask, magic, indexShift, tables));
            }

            return entries.ToArray();
        }

        private static MagicEntry[] BuildMagicRookTables(IEnumerable<(int, ulong)> hardcodedMagics)
        {
            List<MagicEntry> entries = new();
            int i = 0;
            foreach (var (indexBits, magic) in hardcodedMagics)
            {
                int indexShift = 64 - indexBits;
                Square square = (Square)i++;
                ulong mask = RookMask[(int)square];
                if (!TryMakeMagicTable(PieceType.Rook, square, mask, magic, indexBits, out var tables))
                {
                    continue;
                }

                entries.Add(new MagicEntry(mask, magic, indexShift, tables));
            }

            return entries.ToArray();
        }

        private static readonly (int, ulong)[] RookMagics =
        {
            (12, 0x4280008020400013UL), (11, 0x0480200080400012UL), (11, 0x018028A000100080UL), (11, 0x0080080104801000UL),
            (11, 0x00800A8014000800UL), (11, 0x0100040021006228UL), (11, 0x0200210800820004UL), (12, 0x010001000081C022UL),
            (11, 0x008080062C400180UL), (10, 0x2000400020100048UL), (10, 0x0051001220004902UL), (10, 0x0001001000200902UL),
            (10, 0x0080808004000800UL), (10, 0x0400800400804200UL), (10, 0x100C000203B00814UL), (11, 0x0000800240800500UL),
            (11, 0x0080004010200044UL), (10, 0x030041401000E000UL), (10, 0x2002410020010030UL), (10, 0x0249808010020804UL),
            (10, 0x0004110008000500UL), (10, 0x204101000C008248UL), (10, 0x0C800C004810010AUL), (11, 0x2001120000640181UL),
            (11, 0x2940048080004021UL), (10, 0x0010044140006000UL), (10, 0x0081401100200100UL), (10, 0x0400080080100280UL),
            (10, 0x5800080080040280UL), (10, 0x0804020080040080UL), (10, 0x0010101400080102UL), (11, 0x0080010200005184UL),
            (11, 0x48800020084000C0UL), (10, 0x20C2002082004100UL), (10, 0x0000900089802000UL), (10, 0x0C11001001002408UL),
            (10, 0x0804008004800800UL), (10, 0x2A02000A80800400UL), (10, 0x0048800300800200UL), (11, 0x1084C90086000044UL),
            (11, 0x0088814000308008UL), (10, 0x02105004A0004000UL), (10, 0x0000510220010041UL), (10, 0x10D0220210420008UL),
            (10, 0x4000080100510014UL), (10, 0x0042002410020008UL), (10, 0x1460981002540005UL), (11, 0x0480009100420004UL),
            (11, 0x40808000C0210100UL), (10, 0x4000401180200480UL), (10, 0x080A802005300080UL), (10, 0x0029A01001000900UL),
            (10, 0x00408008000C0080UL), (10, 0x0202001008850200UL), (10, 0x200C102638410400UL), (11, 0x010028C283041A00UL),
            (12, 0x0000403021060082UL), (11, 0x4211008010204001UL), (11, 0x080A704120002901UL), (11, 0x0110040900201001UL),
            (11, 0x1001000800500A05UL), (11, 0x4441006204008801UL), (11, 0x0084810802821004UL), (12, 0x0400808421040252UL)
        };



        private static readonly (int, ulong)[] BishopMagics =
        {
            (6, 0xA020410208210220UL), (5, 0x0220040400822000UL), (5, 0x8184811202030000UL), (5, 0x8110990202010902UL),
            (5, 0x002C04E018001122UL), (5, 0x006504A004044001UL), (5, 0x0002061096081014UL), (6, 0x0420440605032000UL),
            (5, 0x01080C980A141C00UL), (5, 0x0400200A02204300UL), (5, 0x4200101084810310UL), (5, 0x0200590401002100UL),
            (5, 0x84020110C042010DUL), (5, 0x00031C2420880088UL), (5, 0x10002104110440A0UL), (5, 0x0000010582104240UL),
            (5, 0x00080D501010009CUL), (5, 0x4092000408080100UL), (7, 0x0001001828010010UL), (7, 0x40220220208030A0UL),
            (7, 0x8201008090400000UL), (7, 0x0000202202012008UL), (5, 0x0008400404020810UL), (5, 0x0042004082088202UL),
            (5, 0x007A080110101000UL), (5, 0x6094101002028800UL), (7, 0x0018080004004410UL), (9, 0x688200828800810AUL),
            (9, 0x0881004409004004UL), (7, 0x1051020009004144UL), (5, 0x0202008102080100UL), (5, 0x0401010000484800UL),
            (5, 0x4001300800302100UL), (5, 0x50240C0420204926UL), (7, 0x0008640100102102UL), (9, 0x4800100821040400UL),
            (9, 0x00200240400400B0UL), (7, 0x0008030100027004UL), (5, 0x2001080200A48242UL), (5, 0x000400AA02002100UL),
            (5, 0x0A82501004000820UL), (5, 0x0002480211282840UL), (7, 0x0081001802001400UL), (7, 0x4008014010400203UL),
            (7, 0x0000080900410400UL), (7, 0x0220210301080200UL), (5, 0x00200B0401010080UL), (5, 0x0301012408890100UL),
            (5, 0x2202015016101444UL), (5, 0x0801008084210000UL), (5, 0x0A20051480900032UL), (5, 0x0000400042120880UL),
            (5, 0x000006100E020000UL), (5, 0x0600083004082800UL), (5, 0x2C88501312140010UL), (5, 0x0804080200420000UL),
            (6, 0x0040802090042000UL), (5, 0x4020006486107088UL), (5, 0x0008801052080400UL), (5, 0x631000244420A802UL),
            (5, 0x0080400820204100UL), (5, 0x101000100C100420UL), (5, 0x011040044840C100UL), (6, 0x0040080104012242UL)
        };

        private static readonly ulong[] BishopMask =
        {
            0x0040201008040200UL, 0x0000402010080400UL, 0x0000004020100A00UL, 0x0000000040221400UL,
            0x0000000002442800UL, 0x0000000204085000UL, 0x0000020408102000UL, 0x0002040810204000UL,
            0x0020100804020000UL, 0x0040201008040000UL, 0x00004020100A0000UL, 0x0000004022140000UL,
            0x0000000244280000UL, 0x0000020408500000UL, 0x0002040810200000UL, 0x0004081020400000UL,
            0x0010080402000200UL, 0x0020100804000400UL, 0x004020100A000A00UL, 0x0000402214001400UL,
            0x0000024428002800UL, 0x0002040850005000UL, 0x0004081020002000UL, 0x0008102040004000UL,
            0x0008040200020400UL, 0x0010080400040800UL, 0x0020100A000A1000UL, 0x0040221400142200UL,
            0x0002442800284400UL, 0x0004085000500800UL, 0x0008102000201000UL, 0x0010204000402000UL,
            0x0004020002040800UL, 0x0008040004081000UL, 0x00100A000A102000UL, 0x0022140014224000UL,
            0x0044280028440200UL, 0x0008500050080400UL, 0x0010200020100800UL, 0x0020400040201000UL,
            0x0002000204081000UL, 0x0004000408102000UL, 0x000A000A10204000UL, 0x0014001422400000UL,
            0x0028002844020000UL, 0x0050005008040200UL, 0x0020002010080400UL, 0x0040004020100800UL,
            0x0000020408102000UL, 0x0000040810204000UL, 0x00000A1020400000UL, 0x0000142240000000UL,
            0x0000284402000000UL, 0x0000500804020000UL, 0x0000201008040200UL, 0x0000402010080400UL,
            0x0002040810204000UL, 0x0004081020400000UL, 0x000A102040000000UL, 0x0014224000000000UL,
            0x0028440200000000UL, 0x0050080402000000UL, 0x0020100804020000UL, 0x0040201008040200UL
        };


        private static readonly ulong[] RookMask =
        {
            0x000101010101017EUL, 0x000202020202027CUL, 0x000404040404047AUL, 0x0008080808080876UL,
            0x001010101010106EUL, 0x002020202020205EUL, 0x004040404040403EUL, 0x008080808080807EUL,
            0x0001010101017E00UL, 0x0002020202027C00UL, 0x0004040404047A00UL, 0x0008080808087600UL,
            0x0010101010106E00UL, 0x0020202020205E00UL, 0x0040404040403E00UL, 0x0080808080807E00UL,
            0x00010101017E0100UL, 0x00020202027C0200UL, 0x00040404047A0400UL, 0x0008080808760800UL,
            0x00101010106E1000UL, 0x00202020205E2000UL, 0x00404040403E4000UL, 0x00808080807E8000UL,
            0x000101017E010100UL, 0x000202027C020200UL, 0x000404047A040400UL, 0x0008080876080800UL,
            0x001010106E101000UL, 0x002020205E202000UL, 0x004040403E404000UL, 0x008080807E808000UL,
            0x0001017E01010100UL, 0x0002027C02020200UL, 0x0004047A04040400UL, 0x0008087608080800UL,
            0x0010106E10101000UL, 0x0020205E20202000UL, 0x0040403E40404000UL, 0x0080807E80808000UL,
            0x00017E0101010100UL, 0x00027C0202020200UL, 0x00047A0404040400UL, 0x0008760808080800UL,
            0x00106E1010101000UL, 0x00205E2020202000UL, 0x00403E4040404000UL, 0x00807E8080808000UL,
            0x007E010101010100UL, 0x007C020202020200UL, 0x007A040404040400UL, 0x0076080808080800UL,
            0x006E101010101000UL, 0x005E202020202000UL, 0x003E404040404000UL, 0x007E808080808000UL,
            0x7E01010101010100UL, 0x7C02020202020200UL, 0x7A04040404040400UL, 0x7608080808080800UL,
            0x6E10101010101000UL, 0x5E20202020202000UL, 0x3E40404040404000UL, 0x7E80808080808000UL
        };
    }
}