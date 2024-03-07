using Types;
using File = Types.File;

namespace Core
{
    public class Position
    {
        public Board CurrentBoard;
        public State CurrentState;
        public void StartingPosition()
        {
            CurrentBoard = new(
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
            CurrentState = new(
                playerTurn: Color.White,
                castlingRights: CastlingRights.AnyCastling,
                enPassantSquare: Square.None,
                halfMoveClock: 0,
                moveCount: 0
            );
        }

        public void PerformMove(Move move)
        {
            // State newState = new(State);
        }

        public ulong PawnPush()
        {
            ulong pawns = CurrentState.PlayerTurn == Color.White ? CurrentBoard.WhitePawns : CurrentBoard.BlackPawns;
            ulong pawnsOnSecondRank = pawns & Bitboards.Of(Ranks.RelativeRank(CurrentState.PlayerTurn, Rank.Two));
            ulong doubleMoves = CurrentState.PlayerTurn == Color.White ? pawnsOnSecondRank << 16 : pawnsOnSecondRank >> 16;
            ulong singleMoves = CurrentState.PlayerTurn == Color.White ? pawns << 8 : pawns >> 8;
            return doubleMoves | singleMoves;
        }

        public ulong KnightMoves()
        {
            ulong knights = CurrentState.PlayerTurn == Color.White ? CurrentBoard.WhiteKnights : CurrentBoard.BlackKnights;
            return knights << 17  // North + NorthEast
                 | knights << 10  // East + NorthEast
                 | knights >> 6   // East + SouthEast
                 | knights >> 15  // South + SouthEast
                 | knights >> 17  // South + SouthWest
                 | knights >> 10  // West + SouthWest
                 | knights << 6   // West + NorthWest 
                 | knights << 15; // North + NorthWest
        }

        public ulong KingMoves()
        {
            ulong king = CurrentState.PlayerTurn == Color.White ? CurrentBoard.WhiteKing : CurrentBoard.BlackKing;
            return king << 8  // North
                 | king << 9  // NorthEast
                 | king << 1  // East
                 | king >> 7  // SouthEast
                 | king >> 8  // South
                 | king >> 9  // SouthWest
                 | king >> 1  // West
                 | king << 7; // NorthWest
        }

    }
}