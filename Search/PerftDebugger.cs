using Core;

namespace Search
{
    public class PerftDebugger
    {
        private readonly Dictionary<MoveWrapper, Dictionary<MoveWrapper, ulong>> _movesToSubmoves;
        private int _maxDepth;

        public PerftDebugger()
        {
            _movesToSubmoves = new();
        }

        public IEnumerable<KeyValuePair<MoveWrapper, Dictionary<MoveWrapper, ulong>>> IterateMovesToSubmoves()
        {
            foreach (var movePair in _movesToSubmoves)
            {
                yield return movePair;
            }
        }

        public void SetMaxDepth(int maxDepth)
        {
            _maxDepth = maxDepth;
        }

        public int GetMaxDepth()
        {
            return _maxDepth;
        }


        public bool TryGet(MoveWrapper move, out Dictionary<MoveWrapper, ulong> submoves)
        {

            if (!_movesToSubmoves.ContainsKey(move))
            {
                submoves = new();
                return false;
            }

            submoves = _movesToSubmoves[move];

            return true;
        }

        public void Add(MoveWrapper move, MoveWrapper submove)
        {
            if (!_movesToSubmoves.ContainsKey(move))
            {
                Dictionary<MoveWrapper, ulong> dict = new() { [submove] = 0UL };
                _movesToSubmoves[move] = dict;
            }
            else
            {
                Dictionary<MoveWrapper, ulong> dict = _movesToSubmoves[move];
                dict.Add(submove, 0UL);
            }
        }

        public bool TryIncrement(MoveWrapper move, MoveWrapper submove)
        {
            if (!_movesToSubmoves.ContainsKey(move))
            {
                return false;
            }

            Dictionary<MoveWrapper, ulong> dict = _movesToSubmoves[move];
            if (!dict.ContainsKey(submove))
            {
                return false;
            }

            dict[submove]++;
            return true;
        }

        public ulong Total(MoveWrapper move)
        {
            if (!_movesToSubmoves.ContainsKey(move)) return 0;
            ulong total = 0UL;
            foreach (var submove in _movesToSubmoves[move])
            {
                total += submove.Value;
            }
            return total;
        }

        public int Count(MoveWrapper move)
        {
            if (!_movesToSubmoves.ContainsKey(move)) return 0;

            return _movesToSubmoves[move].Count;
        }

        public bool TryCount(MoveWrapper move, MoveWrapper submove, out ulong count)
        {
            count = 0UL;
            if (!_movesToSubmoves.ContainsKey(move))
                return false;

            if (!_movesToSubmoves[move].ContainsKey(submove))
                return false;

            count = _movesToSubmoves[move][submove];
            return true;
        }

        public void Clear()
        {
            _movesToSubmoves.Clear();
        }
    }
}