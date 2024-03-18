using Types;
namespace Core
{
    public class State
    {
        public State? Previous;
        public Color Turn;
        public CastlingRights CastlingRights;
        public Square? EnPassantSquare;  // Target square for an en passant move
        public int HalfMoveClock;  // Fifty move rule: Incremented on a move from either side, reset on capture or pawn move
        public int MoveCount;  // Incremented on black move

        public State(Color turn, CastlingRights castlingRights, int halfMoveClock, int moveCount, Square? enPassantSquare = null)
        {
            Previous = null;
            Turn = turn;
            CastlingRights = castlingRights;
            EnPassantSquare = enPassantSquare;
            HalfMoveClock = halfMoveClock;
            MoveCount = moveCount;
        }

        public State(State state)
        {
            Previous = state;
            Turn = state.Turn ^ Color.Black;
            CastlingRights = state.CastlingRights;
            EnPassantSquare = null;
            HalfMoveClock = state.HalfMoveClock + 1;
            MoveCount = state.MoveCount + (int)state.Turn;
        }

        public State(State state, CastlingRights removedCastlingRights)
        {
            Previous = state;
            Turn = state.Turn ^ Color.Black;
            CastlingRights = state.CastlingRights & ~removedCastlingRights;
            EnPassantSquare = null;
            HalfMoveClock = state.HalfMoveClock + 1;
            MoveCount = state.MoveCount + (int)state.Turn;
        }

        public State(State state, Square enPassantSquare)
        {
            Previous = state;
            Turn = state.Turn ^ Color.Black;
            CastlingRights = state.CastlingRights;
            EnPassantSquare = enPassantSquare;
            HalfMoveClock = 0;
            MoveCount = state.MoveCount + (int)state.Turn;
        }

        public State(State state, bool resetClock)
        {
            Previous = state;
            Turn = state.Turn ^ Color.Black;
            CastlingRights = state.CastlingRights;
            EnPassantSquare = null;
            HalfMoveClock = resetClock ? 0 : state.HalfMoveClock + 1;
            MoveCount = state.MoveCount + (int)state.Turn;
        }
    }
}