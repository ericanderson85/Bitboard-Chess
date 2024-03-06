using Types;
using File = Types.File;

namespace Core
{
    public class Position
    {
        public Board board;
        public State state;
        public void StartingPosition()
        {
            board = new(
                P: 0x000000000000FF00,
                N: 0x0000000000000042,
                B: 0x0000000000000024,
                R: 0x0000000000000081,
                Q: 0x0000000000000008,
                K: 0x0000000000000010,
                p: 0x00FF000000000000,
                n: 0x4200000000000000,
                b: 0x2400000000000000,
                r: 0x8100000000000000,
                q: 0x0800000000000000,
                k: 0x1000000000000000
            );
        }

        public void PerformMove(Move move, State newState)
        {

        }

    }
}