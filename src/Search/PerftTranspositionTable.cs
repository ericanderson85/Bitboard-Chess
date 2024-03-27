namespace Search
{


    public static class PerftTranspositionTable
    {
        private const int ENTRY_SIZE = 24;
        private const int HASH_MEMORY_MEGABYTES = 256;
        private const int TABLE_SIZE = HASH_MEMORY_MEGABYTES * 1024 * 1024 / ENTRY_SIZE;
        private static readonly PerftEntry[] _table = new PerftEntry[TABLE_SIZE];

        private struct PerftEntry
        {
            public ulong Hash;
            public int Depth;
            public ulong Count;

        }

        public static void Clear()
        {
            Array.Clear(_table);
        }

        private static int GetIndex(ulong hash)
        {
            return (int)(hash % TABLE_SIZE);
        }

        public static void Store(ulong hash, int depth, ulong count)
        {
            int index = GetIndex(hash);
            _table[index].Hash = hash;
            _table[index].Depth = depth;
            _table[index].Count = count;
        }

        public static bool TryGet(ulong hash, int depth, out ulong count)
        {
            int index = GetIndex(hash);

            bool success = _table[index].Depth == depth && _table[index].Hash == hash;

            count = success ? _table[index].Count : 0;

            return success;
        }
    }
}