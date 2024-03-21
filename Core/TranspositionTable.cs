using Types;

namespace Core
{
    public readonly struct TranspositionEntry
    {
        public int Depth { get; }
        public double Evaluation { get; }
        public Bounds Bound { get; }

        public TranspositionEntry(int depth, double evaluation, Bounds bound)
        {
            Depth = depth;
            Evaluation = evaluation;
            Bound = bound;
        }
    }

    public class TranspositionTable
    {
        private readonly Dictionary<ulong, TranspositionEntry> _table;

        public TranspositionTable()
        {
            _table = new();
        }

        public void Store(ulong hash, int depth, double evaluation, Bounds bound)
        {
            if (_table.ContainsKey(hash))
            {
                TranspositionEntry existingEntry = _table[hash];
                if (existingEntry.Depth <= depth)
                {
                    _table[hash] = new TranspositionEntry(depth, evaluation, bound);
                }
            }
            else
            {
                _table.Add(hash, new TranspositionEntry(depth, evaluation, bound));
            }
        }

        public bool TryGet(ulong hash, out TranspositionEntry entry)
        {
            return _table.TryGetValue(hash, out entry);
        }
    }
}