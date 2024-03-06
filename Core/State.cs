using Types;
namespace Core
{
    public struct State
    {
        public Color PlayerTurn;
        public CastlingRights CastlingRights;
        public Square EnPassantSquare;  // Target square for an en passant move
        public short HalfMoveClock;  // Fifty move rule: Incremented on a move from either side, reset on capture or pawn move
        public short MoveCount;  // Incremented on black move

        public State(Color playerTurn, CastlingRights castlingRights, Square enPassantSquare, short halfMoveClock, short moveCount)
        {
            PlayerTurn = playerTurn;
            CastlingRights = castlingRights;
            EnPassantSquare = enPassantSquare;
            HalfMoveClock = halfMoveClock;
            MoveCount = moveCount;
        }
    }
}