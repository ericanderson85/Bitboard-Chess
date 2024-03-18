using Types;

namespace Core
{
    public class Position
    {
        private static readonly Magic _magic = Magic.Hardcoded();
        public Board CurrentBoard;
        public State CurrentState;
        public Position()
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
                turn: Color.White,
                castlingRights: CastlingRights.AnyCastling,
                halfMoveClock: 0,
                moveCount: 0,
                enPassantSquare: null
            );
        }

        public void PerformMove(Move move, PieceType pieceType, PieceType enemyPieceType = PieceType.None)
        {
            CurrentBoard.Move(move, pieceType, enemyPieceType, CurrentState.Turn);
            CurrentState = new(CurrentState);
        }

        public List<Move> PawnMoves()
        {
            if (CurrentState.Turn == Color.White)
            {
                return WhitePawnMoves();
            }
            return BlackPawnMoves();
        }

        public List<Move> WhitePawnMoves()
        {
            List<Move> pawnMoves = new();
            ulong enPassantSquare = CurrentState.EnPassantSquare == null ? 0 : Bitboards.FromSquare((Square)CurrentState.EnPassantSquare);

            ulong singlePush = ((CurrentBoard.WhitePawns & ~Ranks.BitboardSeven) << 8) & ~CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.GetSquaresFromBitboard(singlePush))
            {
                Square from = to - 8;
                pawnMoves.Add(new Move(from, to));
            }

            ulong doublePush = ((singlePush & Ranks.BitboardThree) << 8) & ~CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.GetSquaresFromBitboard(doublePush))
            {
                Square from = to - 16;
                pawnMoves.Add(new Move(from, to));
            }

            ulong promotionPush = ((CurrentBoard.WhitePawns & Ranks.BitboardSeven) << 8) & ~CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.GetSquaresFromBitboard(promotionPush))
            {
                Square from = to - 8;
                for (PieceType pieceType = PieceType.Knight; pieceType <= PieceType.Queen; pieceType++)
                {
                    pawnMoves.Add(new Move(from, to, MoveType.Promotion, pieceType));
                }
            }

            ulong captureWest = ((CurrentBoard.WhitePawns & ~Files.BitboardH & ~Ranks.BitboardSeven) << 7) & (CurrentBoard.BlackPieces | enPassantSquare);
            foreach (Square to in Bitboards.GetSquaresFromBitboard(captureWest))
            {
                Square from = to - 7;
                pawnMoves.Add(new Move(from, to));
            }

            ulong promotionCaptureWest = ((CurrentBoard.WhitePawns & ~Files.BitboardH & Ranks.BitboardSeven) << 7) & CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.GetSquaresFromBitboard(promotionCaptureWest))
            {
                Square from = to - 7;
                for (PieceType pieceType = PieceType.Knight; pieceType <= PieceType.Queen; pieceType++)
                {
                    pawnMoves.Add(new Move(from, to, MoveType.Promotion, pieceType));
                }
            }

            ulong captureEast = ((CurrentBoard.WhitePawns & ~Files.BitboardA & ~Ranks.BitboardSeven) << 9) & (CurrentBoard.BlackPieces | enPassantSquare);
            foreach (Square to in Bitboards.GetSquaresFromBitboard(captureEast))
            {
                Square from = to - 9;
                pawnMoves.Add(new Move(from, to));
            }

            ulong promotionCaptureEast = ((CurrentBoard.WhitePawns & ~Files.BitboardA & Ranks.BitboardSeven) << 9) & CurrentBoard.BlackPieces;
            if (promotionCaptureEast != 0)
                foreach (Square to in Bitboards.GetSquaresFromBitboard(promotionCaptureEast))
                {
                    Square from = to - 9;
                    for (PieceType pieceType = PieceType.Knight; pieceType <= PieceType.Queen; pieceType++)
                    {
                        pawnMoves.Add(new Move(from, to, MoveType.Promotion, pieceType));
                    }
                }

            return pawnMoves;
        }

        public List<Move> BlackPawnMoves()
        {
            List<Move> pawnMoves = new();
            ulong enPassantSquare = CurrentState.EnPassantSquare == null ? 0 : Bitboards.FromSquare((Square)CurrentState.EnPassantSquare);

            ulong singlePush = ((CurrentBoard.BlackPawns & ~Ranks.BitboardTwo) >> 8) & ~CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.GetSquaresFromBitboard(singlePush))
            {
                Square from = to + 8;
                pawnMoves.Add(new Move(from, to));
            }

            ulong doublePush = ((singlePush & Ranks.BitboardSix) >> 8) & ~CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.GetSquaresFromBitboard(doublePush))
            {
                Square from = to + 16;
                pawnMoves.Add(new Move(from, to));
            }

            ulong promotionPush = ((CurrentBoard.BlackPawns & Ranks.BitboardTwo) >> 8) & ~CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.GetSquaresFromBitboard(promotionPush))
            {
                Square from = to + 8;
                for (PieceType pieceType = PieceType.Knight; pieceType <= PieceType.Queen; pieceType++)
                {
                    pawnMoves.Add(new Move(from, to, MoveType.Promotion, pieceType));
                }
            }

            ulong captureWest = ((CurrentBoard.BlackPawns & ~Files.BitboardH & ~Ranks.BitboardTwo) >> 9) & (CurrentBoard.WhitePieces | enPassantSquare);
            foreach (Square to in Bitboards.GetSquaresFromBitboard(captureWest))
            {
                Square from = to + 9;
                pawnMoves.Add(new Move(from, to));
            }

            ulong promotionCaptureWest = ((CurrentBoard.BlackPawns & ~Files.BitboardH & Ranks.BitboardTwo) >> 9) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.GetSquaresFromBitboard(promotionCaptureWest))
            {
                Square from = to + 9;
                for (PieceType pieceType = PieceType.Knight; pieceType <= PieceType.Queen; pieceType++)
                {
                    pawnMoves.Add(new Move(from, to, MoveType.Promotion, pieceType));
                }
            }

            ulong captureEast = ((CurrentBoard.BlackPawns & ~Files.BitboardA & ~Ranks.BitboardTwo) >> 7) & (CurrentBoard.WhitePieces | enPassantSquare);
            foreach (Square to in Bitboards.GetSquaresFromBitboard(captureEast))
            {
                Square from = to + 7;
                pawnMoves.Add(new Move(from, to));
            }

            ulong promotionCaptureEast = ((CurrentBoard.BlackPawns & ~Files.BitboardA & Ranks.BitboardTwo) >> 7) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.GetSquaresFromBitboard(promotionCaptureEast))
            {
                Square from = to + 7;
                for (PieceType pieceType = PieceType.Knight; pieceType <= PieceType.Queen; pieceType++)
                {
                    pawnMoves.Add(new Move(from, to, MoveType.Promotion, pieceType));
                }
            }

            return pawnMoves;
        }

        public List<Move> KnightMoves()
        {
            List<Move> knightMoves = new();
            ulong knights;
            ulong friendlyPieces;
            if (CurrentState.Turn == Color.White)
            {
                knights = CurrentBoard.WhiteKnights;
                friendlyPieces = CurrentBoard.WhitePieces;
            }
            else
            {
                knights = CurrentBoard.BlackKnights;
                friendlyPieces = CurrentBoard.BlackPieces;
            }

            foreach (Square from in Bitboards.GetSquaresFromBitboard(knights))
            {
                IEnumerable<Square> potentialSquares = Bitboards.GetSquaresFromBitboard(AttackTables.KnightAttacks[(int)from]);
                foreach (Square to in potentialSquares)
                {
                    if ((Bitboards.FromSquare(to) & friendlyPieces) == 0)
                        knightMoves.Add(new(from, to));
                }
            }

            return knightMoves;
        }

        public List<Move> BishopMoves()
        {
            List<Move> bishopMoves = new();
            ulong bishops;
            ulong friendlyPieces;
            if (CurrentState.Turn == Color.White)
            {
                bishops = CurrentBoard.WhiteBishops;
                friendlyPieces = CurrentBoard.WhitePieces;
            }
            else
            {
                bishops = CurrentBoard.BlackBishops;
                friendlyPieces = CurrentBoard.BlackPieces;
            }

            foreach (Square from in Bitboards.GetSquaresFromBitboard(bishops))
            {
                ulong destinations = _magic.GetBishopMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.GetSquaresFromBitboard(destinations))
                {
                    bishopMoves.Add(new(from, to));
                }
            }

            return bishopMoves;
        }

        public List<Move> RookMoves()
        {
            List<Move> rookMoves = new();
            ulong rooks;
            ulong friendlyPieces;
            if (CurrentState.Turn == Color.White)
            {
                rooks = CurrentBoard.WhiteRooks;
                friendlyPieces = CurrentBoard.WhitePieces;
            }
            else
            {
                rooks = CurrentBoard.BlackRooks;
                friendlyPieces = CurrentBoard.BlackPieces;
            }

            foreach (Square from in Bitboards.GetSquaresFromBitboard(rooks))
            {
                ulong destinations = _magic.GetRookMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.GetSquaresFromBitboard(destinations))
                {
                    rookMoves.Add(new(from, to));
                }
            }

            return rookMoves;
        }

        public List<Move> QueenMoves()
        {
            List<Move> queenMoves = new();
            ulong queens;
            ulong friendlyPieces;
            if (CurrentState.Turn == Color.White)
            {
                queens = CurrentBoard.WhiteBishops;
                friendlyPieces = CurrentBoard.WhitePieces;
            }
            else
            {
                queens = CurrentBoard.BlackBishops;
                friendlyPieces = CurrentBoard.BlackPieces;
            }

            foreach (Square from in Bitboards.GetSquaresFromBitboard(queens))
            {
                ulong destinations = _magic.GetRookMoves(from, CurrentBoard.AllPieces, friendlyPieces) | _magic.GetBishopMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.GetSquaresFromBitboard(destinations))
                {
                    queenMoves.Add(new(from, to));
                }
            }

            return queenMoves;
        }

        public List<Move> KingMoves()
        {
            List<Move> kingMoves = new();
            ulong king;
            ulong friendlyPieces;
            if (CurrentState.Turn == Color.White)
            {
                king = CurrentBoard.WhiteKing;
                friendlyPieces = CurrentBoard.WhitePieces;
            }
            else
            {
                king = CurrentBoard.BlackKing;
                friendlyPieces = CurrentBoard.BlackPieces;
            }

            Square kingSquare = (Square)Bitboards.LSB(king);
            IEnumerable<Square> potentialSquares = Bitboards.GetSquaresFromBitboard(AttackTables.KingAttacks[(int)kingSquare]);
            foreach (Square to in potentialSquares)
            {
                if ((Bitboards.FromSquare(to) & friendlyPieces) == 0)
                    kingMoves.Add(new(kingSquare, to));
            }
            return kingMoves;
        }



    }
}