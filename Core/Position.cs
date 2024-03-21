using Types;
using File = Types.File;

namespace Core
{

    public readonly struct MoveWrapper
    {
        public readonly Move Move;
        public readonly PieceType PieceType;
        public readonly PieceType EnemyPieceType;
        public readonly Square? EnPassantSquare;

        public MoveWrapper(Move move, PieceType pieceType, PieceType enemyPieceType = PieceType.None, Square? enPassantSquare = null)
        {
            Move = move;
            PieceType = pieceType;
            EnemyPieceType = enemyPieceType;
            EnPassantSquare = enPassantSquare;
        }

        public override string ToString()
        {
            return Move.ToString();
        }
    }

    public class Position
    {
        private static readonly Magic _magic = Magic.Hardcoded();
        public Board CurrentBoard;
        public State CurrentState;
        public Position()
        {
            CurrentBoard = new(
                P: 0x000000000000FF00UL,
                N: 0x0000000000000042UL,
                B: 0x0000000000000024UL,
                R: 0x0000000000000081UL,
                Q: 0x0000000000000008UL,
                K: 0x0000000000000010UL,
                p: 0x00FF000000000000UL,
                n: 0x4200000000000000UL,
                b: 0x2400000000000000UL,
                r: 0x8100000000000000UL,
                q: 0x0800000000000000UL,
                k: 0x1000000000000000UL
            );
            CurrentState = new(
                previous: null,
                turn: Color.White,
                castlingRights: CastlingRights.All,
                enPassantSquare: null,
                halfMoveClock: 0,
                moveCount: 0
            );
        }

        public Position(string fen)
        {
            string[] parts = fen.Split(' ');
            if (parts.Length != 6) throw new ArgumentException("FEN must have 6 space-separated fields.", nameof(fen));

            CurrentBoard = ParseBoardLayout(parts[0]);

            CurrentState = ParseStateLayout(parts);
        }

        public MoveWrapper CreateMove(PieceType pieceType, Square from, Square to, MoveType moveType = MoveType.Normal, PieceType promotionPieceType = PieceType.None, Square? enPassantSquare = null)
        {
            return new(
                pieceType: pieceType,
                move: new Move(from, to, moveType, promotionPieceType),
                enemyPieceType: CurrentBoard.TypeAtSquare(to),
                enPassantSquare: enPassantSquare
            );
        }

        private State NewState(MoveWrapper move)
        {
            CastlingRights castlingRights = CurrentState.CastlingRights;

            if (move.PieceType == PieceType.King)
                castlingRights &= CurrentState.Turn == Color.White ? ~CastlingRights.WhiteCastling : ~CastlingRights.BlackCastling;
            else if (move.PieceType == PieceType.Rook)
            {
                if (CurrentState.Turn == Color.White)
                {
                    if (move.Move.To() == Square.H1) castlingRights &= ~CastlingRights.WhiteKingSide;
                    else if (move.Move.To() == Square.A1) castlingRights &= ~CastlingRights.WhiteQueenSide;
                }
                else
                {
                    if (move.Move.To() == Square.H8) castlingRights &= ~CastlingRights.BlackKingSide;
                    else if (move.Move.To() == Square.A8) castlingRights &= ~CastlingRights.BlackQueenSide;
                }
            }

            bool resetClock = move.PieceType == PieceType.Pawn || move.EnemyPieceType == PieceType.None;

            return new(
                turn: CurrentState.Turn ^ Color.Black,
                castlingRights: castlingRights,
                halfMoveClock: resetClock ? 0 : CurrentState.HalfMoveClock + 1,
                moveCount: CurrentState.MoveCount + (int)CurrentState.Turn,
                enPassantSquare: move.EnPassantSquare,
                previous: CurrentState
            );

        }
        public void PerformMove(MoveWrapper move)
        {
            CurrentBoard.Move(move.Move, move.PieceType, move.EnemyPieceType, CurrentState.Turn);
            CurrentState = NewState(move);
        }



        public void UndoMove(MoveWrapper move)
        {
            if (CurrentState.Previous == null)
                throw new Exception("Can't undo move");

            CurrentState = CurrentState.Previous;
            CurrentBoard.UndoMove(move.Move, move.PieceType, move.EnemyPieceType, CurrentState.Turn);
        }

        public IEnumerable<MoveWrapper> AllMoves()
        {
            return PawnMoves().Concat(KnightMoves()).Concat(BishopMoves()).Concat(RookMoves()).Concat(QueenMoves()).Concat(KingMoves());
        }

        public IEnumerable<MoveWrapper> PawnMoves()
        {
            return CurrentState.Turn == Color.White ? WhitePawnMoves() : BlackPawnMoves();
        }

        public IEnumerable<MoveWrapper> WhitePawnMoves()
        {
            List<MoveWrapper> pawnMoves = new();

            ulong singlePush = ((CurrentBoard.WhitePawns & ~Ranks.BitboardSeven) << 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.Squares(singlePush))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to - 8, to));

            ulong doublePush = ((singlePush & Ranks.BitboardThree) << 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.Squares(doublePush))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to - 16, to, enPassantSquare: to - 8));

            ulong promotionPush = ((CurrentBoard.WhitePawns & Ranks.BitboardSeven) << 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.Squares(promotionPush))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, to - 8, to, MoveType.Promotion, promotionPieceType));

            ulong captureWest = ((CurrentBoard.WhitePawns & ~Files.BitboardA & ~Ranks.BitboardSeven) << 7) & CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.Squares(captureWest))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to - 7, to));

            ulong promotionCaptureWest = ((CurrentBoard.WhitePawns & ~Files.BitboardA & Ranks.BitboardSeven) << 7) & CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.Squares(promotionCaptureWest))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, to - 7, to, MoveType.Promotion, promotionPieceType));

            ulong captureEast = ((CurrentBoard.WhitePawns & ~Files.BitboardH & ~Ranks.BitboardSeven) << 9) & CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.Squares(captureEast))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to - 9, to));

            ulong promotionCaptureEast = ((CurrentBoard.WhitePawns & ~Files.BitboardH & Ranks.BitboardSeven) << 9) & CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.Squares(promotionCaptureEast))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, to - 9, to, MoveType.Promotion, promotionPieceType));

            if (CurrentState.EnPassantSquare != null)
            {
                ulong enPassant = Bitboards.FromSquare((Square)CurrentState.EnPassantSquare);

                if ((((CurrentBoard.WhitePawns & ~Files.BitboardA) << 7) & enPassant) != 0)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, (Square)CurrentState.EnPassantSquare - 7, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant));

                if ((((CurrentBoard.WhitePawns & ~Files.BitboardA) << 9) & enPassant) != 0)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, (Square)CurrentState.EnPassantSquare - 9, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant));
            }


            return pawnMoves;
        }

        public IEnumerable<MoveWrapper> BlackPawnMoves()
        {
            List<MoveWrapper> pawnMoves = new();

            ulong singlePush = ((CurrentBoard.BlackPawns & ~Ranks.BitboardTwo) >> 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.Squares(singlePush))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to + 8, to));

            ulong doublePush = ((singlePush & Ranks.BitboardSix) >> 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.Squares(doublePush))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to + 16, to, enPassantSquare: to - 8));

            ulong promotionPush = ((CurrentBoard.BlackPawns & Ranks.BitboardTwo) >> 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.Squares(promotionPush))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, to + 8, to, MoveType.Promotion, promotionPieceType));

            ulong captureWest = ((CurrentBoard.BlackPawns & ~Files.BitboardH & ~Ranks.BitboardTwo) >> 7) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.Squares(captureWest))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to + 7, to));

            ulong promotionCaptureEast = ((CurrentBoard.BlackPawns & ~Files.BitboardH & Ranks.BitboardTwo) >> 7) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.Squares(promotionCaptureEast))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, to + 7, to, MoveType.Promotion, promotionPieceType));

            ulong captureEast = ((CurrentBoard.BlackPawns & ~Files.BitboardA & ~Ranks.BitboardTwo) >> 9) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.Squares(captureEast))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to + 9, to));

            ulong promotionCaptureWest = ((CurrentBoard.BlackPawns & ~Files.BitboardA & Ranks.BitboardTwo) >> 9) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.Squares(promotionCaptureWest))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, to + 9, to, MoveType.Promotion, promotionPieceType));

            if (CurrentState.EnPassantSquare != null)
            {
                ulong enPassant = Bitboards.FromSquare((Square)CurrentState.EnPassantSquare);

                if ((((CurrentBoard.BlackPawns & ~Files.BitboardA) >> 9) & enPassant) != 0)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, (Square)CurrentState.EnPassantSquare + 9, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant));

                if ((((CurrentBoard.BlackPawns & ~Files.BitboardH) >> 7) & enPassant) != 0)
                    pawnMoves.Add(CreateMove(PieceType.Pawn, (Square)CurrentState.EnPassantSquare + 7, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant));
            }


            return pawnMoves;


            // List<Move> pawnMoves = new();

            // ulong singlePush = ((CurrentBoard.BlackPawns & ~Ranks.BitboardTwo) >> 8) & ~CurrentBoard.AllPieces;
            // foreach (Square to in Bitboards.Squares(singlePush))
            // {
            //     Square from = to + 8;
            //     pawnMoves.Add(new Move(from, to));
            // }

            // ulong doublePush = ((singlePush & Ranks.BitboardSix) >> 8) & ~CurrentBoard.AllPieces;
            // foreach (Square to in Bitboards.Squares(doublePush))
            // {
            //     Square from = to + 16;
            //     pawnMoves.Add(new Move(from, to));
            // }

            // ulong promotionPush = ((CurrentBoard.BlackPawns & Ranks.BitboardTwo) >> 8) & ~CurrentBoard.AllPieces;
            // foreach (Square to in Bitboards.Squares(promotionPush))
            // {
            //     Square from = to + 8;
            //     for (PieceType pieceType = PieceType.Knight; pieceType <= PieceType.Queen; pieceType++)
            //     {
            //         pawnMoves.Add(new Move(from, to, MoveType.Promotion, pieceType));
            //     }
            // }

            // ulong captureWest = ((CurrentBoard.BlackPawns & ~Files.BitboardH & ~Ranks.BitboardTwo) >> 7) & CurrentBoard.WhitePieces;
            // foreach (Square to in Bitboards.Squares(captureWest))
            // {
            //     Square from = to + 7;
            //     pawnMoves.Add(new Move(from, to));
            // }

            // ulong promotionCaptureWest = ((CurrentBoard.BlackPawns & ~Files.BitboardH & Ranks.BitboardTwo) >> 7) & CurrentBoard.WhitePieces;
            // foreach (Square to in Bitboards.Squares(promotionCaptureWest))
            // {
            //     Square from = to + 7;
            //     for (PieceType pieceType = PieceType.Knight; pieceType <= PieceType.Queen; pieceType++)
            //     {
            //         pawnMoves.Add(new Move(from, to, MoveType.Promotion, pieceType));
            //     }
            // }

            // ulong captureEast = ((CurrentBoard.BlackPawns & ~Files.BitboardA & ~Ranks.BitboardTwo) >> 9) & CurrentBoard.WhitePieces;
            // foreach (Square to in Bitboards.Squares(captureEast))
            // {
            //     Square from = to + 9;
            //     pawnMoves.Add(new Move(from, to));
            // }

            // ulong promotionCaptureEast = ((CurrentBoard.BlackPawns & ~Files.BitboardA & Ranks.BitboardTwo) >> 9) & CurrentBoard.WhitePieces;
            // foreach (Square to in Bitboards.Squares(promotionCaptureEast))
            // {
            //     Square from = to + 9;
            //     for (PieceType pieceType = PieceType.Knight; pieceType <= PieceType.Queen; pieceType++)
            //     {
            //         pawnMoves.Add(new Move(from, to, MoveType.Promotion, pieceType));
            //     }
            // }

            // if (CurrentState.EnPassantSquare == null)
            //     return pawnMoves;

            // ulong enPassant = Bitboards.FromSquare((Square)CurrentState.EnPassantSquare);

            // if ((((CurrentBoard.BlackPawns & ~Files.BitboardH) >> 7) & enPassant) != 0)
            //     pawnMoves.Add(new((Square)CurrentState.EnPassantSquare + 7, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant, PieceType.Pawn));

            // if ((((CurrentBoard.BlackPawns & ~Files.BitboardA) >> 9) & enPassant) != 0)
            //     pawnMoves.Add(new((Square)CurrentState.EnPassantSquare + 9, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant, PieceType.Pawn));

            // return pawnMoves;
        }

        public IEnumerable<MoveWrapper> KnightMoves()
        {
            List<MoveWrapper> knightMoves = new();
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

            foreach (Square from in Bitboards.Squares(knights))
            {
                IEnumerable<Square> potentialSquares = Bitboards.Squares(AttackTables.KnightAttacks[(int)from]);
                foreach (Square to in potentialSquares)
                    if ((Bitboards.FromSquare(to) & friendlyPieces) == 0)
                        knightMoves.Add(CreateMove(PieceType.Knight, from, to));
            }

            return knightMoves;
        }

        public IEnumerable<MoveWrapper> BishopMoves()
        {
            List<MoveWrapper> bishopMoves = new();
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

            foreach (Square from in Bitboards.Squares(bishops))
            {
                ulong destinations = _magic.GetBishopMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.Squares(destinations))
                    bishopMoves.Add(CreateMove(PieceType.Bishop, from, to));
            }

            return bishopMoves;
        }

        public IEnumerable<MoveWrapper> RookMoves()
        {
            List<MoveWrapper> rookMoves = new();
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

            foreach (Square from in Bitboards.Squares(rooks))
            {
                ulong destinations = _magic.GetRookMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.Squares(destinations))
                    rookMoves.Add(CreateMove(PieceType.Rook, from, to));
            }

            return rookMoves;
        }

        public IEnumerable<MoveWrapper> QueenMoves()
        {
            List<MoveWrapper> queenMoves = new();
            ulong queens;
            ulong friendlyPieces;
            if (CurrentState.Turn == Color.White)
            {
                queens = CurrentBoard.WhiteQueens;
                friendlyPieces = CurrentBoard.WhitePieces;
            }
            else
            {
                queens = CurrentBoard.BlackQueens;
                friendlyPieces = CurrentBoard.BlackPieces;
            }

            foreach (Square from in Bitboards.Squares(queens))
            {
                ulong destinations = _magic.GetRookMoves(from, CurrentBoard.AllPieces, friendlyPieces) | _magic.GetBishopMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.Squares(destinations))
                    queenMoves.Add(CreateMove(PieceType.Queen, from, to));
            }

            return queenMoves;
        }

        public IEnumerable<MoveWrapper> KingMoves()
        {
            List<MoveWrapper> kingMoves = new();
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
            IEnumerable<Square> potentialSquares = Bitboards.Squares(AttackTables.KingAttacks[(int)kingSquare]);
            foreach (Square to in potentialSquares)
                if ((Bitboards.FromSquare(to) & friendlyPieces) == 0)
                    kingMoves.Add(CreateMove(PieceType.King, kingSquare, to));

            kingMoves.AddRange(CastlingMoves());

            return kingMoves;
        }

        private IEnumerable<MoveWrapper> CastlingMoves()
        {
            List<MoveWrapper> castlingMoves = new();

            if (CurrentState.Turn == Color.White)
            {
                if (CurrentState.CastlingRights.HasFlag(CastlingRights.WhiteKingSide) && (CurrentBoard.AllPieces & 0x60UL) == 0)
                    castlingMoves.Add(CreateMove(PieceType.King, Square.E1, Square.H1, MoveType.Castling));

                if (CurrentState.CastlingRights.HasFlag(CastlingRights.WhiteQueenSide) && (CurrentBoard.AllPieces & 0xEUL) == 0)
                    castlingMoves.Add(CreateMove(PieceType.King, Square.E1, Square.A1, MoveType.Castling));
            }
            else
            {
                if (CurrentState.CastlingRights.HasFlag(CastlingRights.BlackKingSide) && (CurrentBoard.AllPieces & 0x6000000000000000UL) == 0)
                    castlingMoves.Add(CreateMove(PieceType.King, Square.E8, Square.H8, MoveType.Castling));

                if (CurrentState.CastlingRights.HasFlag(CastlingRights.BlackQueenSide) && (CurrentBoard.AllPieces & 0xE00000000000000UL) == 0)
                    castlingMoves.Add(CreateMove(PieceType.King, Square.E8, Square.H8, MoveType.Castling));
            }

            return castlingMoves;
        }

        private static Board ParseBoardLayout(string layout)
        {
            string[] ranks = layout.Split('/');
            if (ranks.Length != 8) throw new ArgumentException("Board layout must consist of 8 ranks.", nameof(layout));

            ulong whitePawns = 0UL, whiteKnights = 0UL, whiteBishops = 0UL, whiteRooks = 0UL, whiteQueens = 0UL, whiteKing = 0UL,
                blackPawns = 0UL, blackKnights = 0UL, blackBishops = 0UL, blackRooks = 0UL, blackQueens = 0UL, blackKing = 0UL;

            for (Rank rank = Rank.One; rank <= Rank.Eight; rank++)
            {
                File file = 0;
                foreach (char piece in ranks[7 - (int)rank])
                {
                    if (char.IsDigit(piece))
                    {
                        file += piece - '0';
                    }
                    else
                    {
                        Square square = Squares.Of(file, rank);
                        ulong bit = Bitboards.FromSquare(square);

                        switch (piece)
                        {
                            case 'P': whitePawns |= bit; break;
                            case 'N': whiteKnights |= bit; break;
                            case 'B': whiteBishops |= bit; break;
                            case 'R': whiteRooks |= bit; break;
                            case 'Q': whiteQueens |= bit; break;
                            case 'K': whiteKing |= bit; break;
                            case 'p': blackPawns |= bit; break;
                            case 'n': blackKnights |= bit; break;
                            case 'b': blackBishops |= bit; break;
                            case 'r': blackRooks |= bit; break;
                            case 'q': blackQueens |= bit; break;
                            case 'k': blackKing |= bit; break;
                        }

                        file++;
                    }
                }
            }

            return new(
                P: whitePawns,
                N: whiteKnights,
                B: whiteBishops,
                R: whiteRooks,
                Q: whiteQueens,
                K: whiteKing,
                p: blackPawns,
                n: blackKnights,
                b: blackBishops,
                r: blackRooks,
                q: blackQueens,
                k: blackKing
            );
        }

        private static State ParseStateLayout(string[] parts)
        {
            return new State(
                previous: null,
                turn: parts[1] == "w" ? Color.White : Color.Black,
                castlingRights: ParseCastlingRights(parts[2]),
                enPassantSquare: ParseEnPassantSquare(parts[3]),
                halfMoveClock: int.Parse(parts[4]),
                moveCount: int.Parse(parts[5])
            );
        }

        private static CastlingRights ParseCastlingRights(string castling)
        {
            CastlingRights rights = CastlingRights.None;
            if (castling.Contains('K')) rights |= CastlingRights.WhiteKingSide;
            if (castling.Contains('Q')) rights |= CastlingRights.WhiteQueenSide;
            if (castling.Contains('k')) rights |= CastlingRights.BlackKingSide;
            if (castling.Contains('q')) rights |= CastlingRights.BlackQueenSide;
            return rights;
        }

        private static Square? ParseEnPassantSquare(string enPassant)
        {
            if (enPassant == "-") return null;
            return Squares.Of(enPassant);
        }

        public override string ToString()
        {
            return CurrentBoard.ToString();
        }
    }
}