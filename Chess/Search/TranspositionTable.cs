namespace Search
{
    public enum Bounds
    {
        Exact,
        Lower,
        Upper
    }


    public static class TranspositionTable
    {

        private const int ENTRY_SIZE = 24;
        private const int HASH_MEMORY_MEGABYTES = 1000;
        private const int TABLE_SIZE = HASH_MEMORY_MEGABYTES * 1024 * 1024 / ENTRY_SIZE;
        private static readonly TranspositionEntry[] _table = new TranspositionEntry[TABLE_SIZE];

        private struct TranspositionEntry
        {
            public ulong Hash;
            public int Depth;
            public int Evaluation;
            public Bounds Bound;
        }

        public static void Clear()
        {
            Array.Clear(_table, 0, TABLE_SIZE);
        }

        private static int GetIndex(ulong hash)
        {
            return (int)(hash % TABLE_SIZE);
        }


        public static void Store(ulong hash, int depth, int evaluation, Bounds bound)
        {
            int index = GetIndex(hash);
            TranspositionEntry existingEntry = _table[index];

            if (existingEntry.Depth <= depth)
            {
                _table[index].Hash = hash;
                _table[index].Depth = depth;
                _table[index].Evaluation = evaluation;
                _table[index].Bound = bound;
            }
        }

        public static bool TryGet(ulong hash, int depth, out int evaluation, out Bounds bound)
        {
            int index = GetIndex(hash);

            bool success = _table[index].Hash == hash && _table[index].Depth >= depth;

            evaluation = success ? _table[index].Evaluation : 0;
            bound = success ? _table[index].Bound : 0;

            return success;
        }
    }
}

