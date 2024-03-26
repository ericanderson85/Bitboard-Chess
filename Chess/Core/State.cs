using Types;
namespace Core
{
    public class State
    {
        public State? Previous;
        public ulong? PreviousKey;
        public Color Turn;
        public CastlingRights CastlingRights;
        public Square? EnPassantSquare;  // Target square for an en passant move
        public int HalfMoveClock;  // Fifty move rule: Incremented on a move from either side, reset on capture or pawn move
        public int MoveCount;  // Incremented on black move

        public State(State? previous, ulong? previousKey, Color turn, CastlingRights castlingRights, Square? enPassantSquare, int halfMoveClock, int moveCount)
        {
            Previous = previous;
            PreviousKey = previousKey;
            Turn = turn;
            CastlingRights = castlingRights;
            EnPassantSquare = enPassantSquare;
            HalfMoveClock = halfMoveClock;
            MoveCount = moveCount;
        }
    }
}