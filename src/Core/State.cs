using Types;
using System.Text;

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
        public int FullMoveCount;  // Incremented on black move

        public State(State? previous, ulong? previousKey, Color turn, CastlingRights castlingRights, Square? enPassantSquare, int halfMoveClock, int moveCount)
        {
            Previous = previous;
            PreviousKey = previousKey;
            Turn = turn;
            CastlingRights = castlingRights;
            EnPassantSquare = enPassantSquare;
            HalfMoveClock = halfMoveClock;
            FullMoveCount = moveCount;
        }

        public StringBuilder StateFEN()
        {
            StringBuilder fen = new(" ");
            fen.Append(Turn == Color.White ? 'w' : 'b').Append(' ');

            StringBuilder castlingRights = new();
            if (CastlingRights.HasFlag(CastlingRights.WhiteKingSide)) castlingRights.Append('K');
            if (CastlingRights.HasFlag(CastlingRights.WhiteQueenSide)) castlingRights.Append('Q');
            if (CastlingRights.HasFlag(CastlingRights.BlackKingSide)) castlingRights.Append('k');
            if (CastlingRights.HasFlag(CastlingRights.BlackQueenSide)) castlingRights.Append('q');

            if (castlingRights.Length == 0) fen.Append("- ");
            else fen.Append(castlingRights).Append(' ');

            fen.Append(EnPassantSquare?.ToString().ToLower() ?? "-").Append(' ');

            fen.Append(HalfMoveClock).Append(' ');
            fen.Append(FullMoveCount);

            return fen;
        }
    }
}