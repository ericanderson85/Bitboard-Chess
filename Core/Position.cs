using System.Diagnostics;
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
            string destination = Move.ToString();
            string promotion = Move.PromotionPieceType() switch
            {
                PieceType.Queen => "q",
                PieceType.Rook => "r",
                PieceType.Bishop => "b",
                PieceType.Knight => "n",
                _ => "",
            };

            return $"{destination}{promotion}";
        }
    }

    public class Position
    {
        private static readonly Magic _magic = Magic.Hardcoded();
        public Board CurrentBoard;
        public State CurrentState;
        public ulong ZobristKey;
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
                previousKey: null,
                turn: Color.White,
                castlingRights: CastlingRights.All,
                enPassantSquare: null,
                halfMoveClock: 0,
                moveCount: 0
            );
            ZobristKey = Zobrist.CalculateHash(CurrentBoard, CurrentState);
        }

        public Position(string fen)
        {
            string[] parts = fen.Split(' ');
            if (parts.Length != 6) throw new ArgumentException("FEN must have 6 space-separated fields.", nameof(fen));

            CurrentBoard = ParseBoardLayout(parts[0]);

            CurrentState = ParseStateLayout(parts);

            ZobristKey = Zobrist.CalculateHash(CurrentBoard, CurrentState);
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

        public MoveWrapper CreateMove(string uciMove)
        {
            Square from = Squares.FromString(uciMove[..2]);
            Square to = Squares.FromString(uciMove.Substring(2, 2));
            PieceType pieceType = CurrentBoard.TypeAtSquare(from);
            MoveType moveType = MoveType.Normal;
            PieceType promotionPieceType = PieceType.None;
            Square? enPassantSquare = null;


            if (pieceType == PieceType.King && int.Abs(Files.Of(from) - Files.Of(to)) == 2)
                moveType = MoveType.Castling;
            else if (pieceType == PieceType.Pawn)
            {
                if (uciMove.Length > 4)
                {
                    char promotionChar = uciMove[4];
                    promotionPieceType = promotionChar switch
                    {
                        'q' => PieceType.Queen,
                        'r' => PieceType.Rook,
                        'b' => PieceType.Bishop,
                        'n' => PieceType.Knight,
                        _ => PieceType.None,
                    };
                    moveType = MoveType.Promotion;
                }
                else if (CurrentState.EnPassantSquare == to) moveType = MoveType.EnPassant;
                else if (int.Abs(from - to) == 16) enPassantSquare = to + (to - from) / 2;
            }

            return CreateMove(pieceType, from, to, moveType, promotionPieceType, enPassantSquare);
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
                    if (move.Move.From() == Square.H1) castlingRights &= ~CastlingRights.WhiteKingSide;
                    else if (move.Move.From() == Square.A1) castlingRights &= ~CastlingRights.WhiteQueenSide;
                }
                else
                {
                    if (move.Move.From() == Square.H8) castlingRights &= ~CastlingRights.BlackKingSide;
                    else if (move.Move.From() == Square.A8) castlingRights &= ~CastlingRights.BlackQueenSide;
                }
            }

            if (move.EnemyPieceType == PieceType.Rook)
            {
                if (CurrentState.Turn == Color.White)
                {
                    if (move.Move.To() == Square.H8) castlingRights &= ~CastlingRights.BlackKingSide;
                    else if (move.Move.To() == Square.A8) castlingRights &= ~CastlingRights.BlackQueenSide;
                }
                else
                {
                    if (move.Move.To() == Square.H1) castlingRights &= ~CastlingRights.WhiteKingSide;
                    else if (move.Move.To() == Square.A1) castlingRights &= ~CastlingRights.WhiteQueenSide;
                }
            }

            bool resetClock = move.PieceType == PieceType.Pawn || move.EnemyPieceType == PieceType.None;

            return new(
                previous: CurrentState,
                previousKey: ZobristKey,
                turn: CurrentState.Turn ^ Color.Black,
                castlingRights: castlingRights,
                enPassantSquare: move.EnPassantSquare,
                halfMoveClock: resetClock ? 0 : CurrentState.HalfMoveClock + 1,
                moveCount: CurrentState.MoveCount + (int)CurrentState.Turn
            );

        }
        public void PerformMove(MoveWrapper move)
        {
            CurrentBoard.Move(move.Move, move.PieceType, move.EnemyPieceType, CurrentState.Turn);
            State prevState = CurrentState;
            CurrentState = NewState(move);
            Zobrist.UpdateHash(ref ZobristKey, move, prevState, CurrentState);
            // ulong h = Zobrist.CalculateHash(CurrentBoard, CurrentState);
            // if (h != ZobristKey)
            // {
            //     Console.WriteLine(move);
            //     Console.WriteLine($"{h} != {ZobristKey}");
            //     if (move.EnemyPieceType != PieceType.None) Console.WriteLine($"{move.PieceType} captures {move.EnemyPieceType}");
            //     else Console.WriteLine("\nNO CAPTURE\n");
            //     Console.WriteLine(this);
            //     Console.WriteLine("\n");
            // }

        }
        public void UndoMove(MoveWrapper move)
        {
            if (CurrentState.Previous == null || CurrentState.PreviousKey == null)
                throw new Exception("Can't undo move");

            ZobristKey = (ulong)CurrentState.PreviousKey;
            CurrentState = CurrentState.Previous;
            CurrentBoard.UndoMove(move.Move, move.PieceType, move.EnemyPieceType, CurrentState.Turn);
        }

        public List<MoveWrapper> AllMoves()
        {
            return PawnMoves().Concat(KnightMoves()).Concat(BishopMoves()).Concat(RookMoves()).Concat(QueenMoves()).Concat(KingMoves()).ToList();
        }

        public List<MoveWrapper> LoudMoves()
        {
            return LoudPawnMoves().Concat(KnightCaptures()).Concat(BishopCaptures()).Concat(RookCaptures()).Concat(QueenCaptures()).Concat(KingCaptures()).ToList();
        }

        public IEnumerable<MoveWrapper> PawnMoves()
        {
            return CurrentState.Turn == Color.White ? WhitePawnMoves().Concat(LoudWhitePawnMoves()) : BlackPawnMoves().Concat(LoudBlackPawnMoves());
        }

        public IEnumerable<MoveWrapper> WhitePawnMoves()
        {
            List<MoveWrapper> pawnMoves = new();

            ulong singlePush = ((CurrentBoard.WhitePawns & ~Ranks.BitboardSeven) << 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.ToSquares(singlePush))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to - 8, to));

            ulong doublePush = ((singlePush & Ranks.BitboardThree) << 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.ToSquares(doublePush))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to - 16, to, enPassantSquare: to - 8));

            return pawnMoves;
        }

        public IEnumerable<MoveWrapper> BlackPawnMoves()
        {
            List<MoveWrapper> pawnMoves = new();

            ulong singlePush = ((CurrentBoard.BlackPawns & ~Ranks.BitboardTwo) >> 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.ToSquares(singlePush))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to + 8, to));

            ulong doublePush = ((singlePush & Ranks.BitboardSix) >> 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.ToSquares(doublePush))
                pawnMoves.Add(CreateMove(PieceType.Pawn, to + 16, to, enPassantSquare: to + 8));

            return pawnMoves;
        }


        public IEnumerable<MoveWrapper> LoudPawnMoves()
        {
            return CurrentState.Turn == Color.White ? LoudWhitePawnMoves() : LoudBlackPawnMoves();
        }

        public IEnumerable<MoveWrapper> LoudWhitePawnMoves()
        {
            List<MoveWrapper> loudWhiteMoves = new();


            ulong promotionPush = ((CurrentBoard.WhitePawns & Ranks.BitboardSeven) << 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.ToSquares(promotionPush))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudWhiteMoves.Add(CreateMove(PieceType.Pawn, to - 8, to, MoveType.Promotion, promotionPieceType));

            ulong captureWest = ((CurrentBoard.WhitePawns & ~Files.BitboardA & ~Ranks.BitboardSeven) << 7) & CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.ToSquares(captureWest))
                loudWhiteMoves.Add(CreateMove(PieceType.Pawn, to - 7, to));

            ulong promotionCaptureWest = ((CurrentBoard.WhitePawns & ~Files.BitboardA & Ranks.BitboardSeven) << 7) & CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.ToSquares(promotionCaptureWest))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudWhiteMoves.Add(CreateMove(PieceType.Pawn, to - 7, to, MoveType.Promotion, promotionPieceType));

            ulong captureEast = ((CurrentBoard.WhitePawns & ~Files.BitboardH & ~Ranks.BitboardSeven) << 9) & CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.ToSquares(captureEast))
                loudWhiteMoves.Add(CreateMove(PieceType.Pawn, to - 9, to));

            ulong promotionCaptureEast = ((CurrentBoard.WhitePawns & ~Files.BitboardH & Ranks.BitboardSeven) << 9) & CurrentBoard.BlackPieces;
            foreach (Square to in Bitboards.ToSquares(promotionCaptureEast))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudWhiteMoves.Add(CreateMove(PieceType.Pawn, to - 9, to, MoveType.Promotion, promotionPieceType));

            if (CurrentState.EnPassantSquare != null)
            {
                ulong enPassant = Bitboards.From((Square)CurrentState.EnPassantSquare);

                if ((((CurrentBoard.WhitePawns & ~Files.BitboardA) << 7) & enPassant) != 0)
                    loudWhiteMoves.Add(CreateMove(PieceType.Pawn, (Square)CurrentState.EnPassantSquare - 7, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant));

                if ((((CurrentBoard.WhitePawns & ~Files.BitboardH) << 9) & enPassant) != 0)
                    loudWhiteMoves.Add(CreateMove(PieceType.Pawn, (Square)CurrentState.EnPassantSquare - 9, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant));
            }


            return loudWhiteMoves;
        }

        public IEnumerable<MoveWrapper> LoudBlackPawnMoves()
        {
            List<MoveWrapper> loudPawnMoves = new();

            ulong promotionPush = ((CurrentBoard.BlackPawns & Ranks.BitboardTwo) >> 8) & ~CurrentBoard.AllPieces;
            foreach (Square to in Bitboards.ToSquares(promotionPush))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudPawnMoves.Add(CreateMove(PieceType.Pawn, to + 8, to, MoveType.Promotion, promotionPieceType));

            ulong captureWest = ((CurrentBoard.BlackPawns & ~Files.BitboardH & ~Ranks.BitboardTwo) >> 7) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.ToSquares(captureWest))
                loudPawnMoves.Add(CreateMove(PieceType.Pawn, to + 7, to));

            ulong promotionCaptureEast = ((CurrentBoard.BlackPawns & ~Files.BitboardH & Ranks.BitboardTwo) >> 7) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.ToSquares(promotionCaptureEast))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudPawnMoves.Add(CreateMove(PieceType.Pawn, to + 7, to, MoveType.Promotion, promotionPieceType));

            ulong captureEast = ((CurrentBoard.BlackPawns & ~Files.BitboardA & ~Ranks.BitboardTwo) >> 9) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.ToSquares(captureEast))
                loudPawnMoves.Add(CreateMove(PieceType.Pawn, to + 9, to));

            ulong promotionCaptureWest = ((CurrentBoard.BlackPawns & ~Files.BitboardA & Ranks.BitboardTwo) >> 9) & CurrentBoard.WhitePieces;
            foreach (Square to in Bitboards.ToSquares(promotionCaptureWest))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudPawnMoves.Add(CreateMove(PieceType.Pawn, to + 9, to, MoveType.Promotion, promotionPieceType));

            if (CurrentState.EnPassantSquare != null)
            {
                ulong enPassant = Bitboards.From((Square)CurrentState.EnPassantSquare);

                if ((((CurrentBoard.BlackPawns & ~Files.BitboardA) >> 9) & enPassant) != 0)
                    loudPawnMoves.Add(CreateMove(PieceType.Pawn, (Square)CurrentState.EnPassantSquare + 9, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant));

                if ((((CurrentBoard.BlackPawns & ~Files.BitboardH) >> 7) & enPassant) != 0)
                    loudPawnMoves.Add(CreateMove(PieceType.Pawn, (Square)CurrentState.EnPassantSquare + 7, (Square)CurrentState.EnPassantSquare, MoveType.EnPassant));
            }


            return loudPawnMoves;
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

            foreach (Square from in Bitboards.ToSquares(knights))
            {
                IEnumerable<Square> potentialSquares = Bitboards.ToSquares(AttackTables.KnightAttacks[(int)from]);
                foreach (Square to in potentialSquares)
                    if ((Bitboards.From(to) & friendlyPieces) == 0)
                        knightMoves.Add(CreateMove(PieceType.Knight, from, to));
            }

            return knightMoves;
        }

        public IEnumerable<MoveWrapper> KnightCaptures()
        {
            List<MoveWrapper> knightCaptures = new();
            ulong knights;
            ulong enemyPieces;
            if (CurrentState.Turn == Color.White)
            {
                knights = CurrentBoard.WhiteKnights;
                enemyPieces = CurrentBoard.BlackPieces;
            }
            else
            {
                knights = CurrentBoard.BlackKnights;
                enemyPieces = CurrentBoard.WhitePieces;
            }

            foreach (Square from in Bitboards.ToSquares(knights))
            {
                IEnumerable<Square> potentialSquares = Bitboards.ToSquares(AttackTables.KnightAttacks[(int)from]);
                foreach (Square to in potentialSquares)
                    if ((Bitboards.From(to) & enemyPieces) != 0)
                        knightCaptures.Add(CreateMove(PieceType.Knight, from, to));
            }

            return knightCaptures;
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

            foreach (Square from in Bitboards.ToSquares(bishops))
            {
                ulong destinations = _magic.GetBishopMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations))
                    bishopMoves.Add(CreateMove(PieceType.Bishop, from, to));
            }

            return bishopMoves;
        }

        public IEnumerable<MoveWrapper> BishopCaptures()
        {
            List<MoveWrapper> bishopCaptures = new();
            ulong bishops;
            ulong friendlyPieces;
            ulong enemyPieces;
            if (CurrentState.Turn == Color.White)
            {
                bishops = CurrentBoard.WhiteBishops;
                friendlyPieces = CurrentBoard.WhitePieces;
                enemyPieces = CurrentBoard.BlackPieces;
            }
            else
            {
                bishops = CurrentBoard.BlackBishops;
                friendlyPieces = CurrentBoard.BlackPieces;
                enemyPieces = CurrentBoard.WhitePieces;
            }

            foreach (Square from in Bitboards.ToSquares(bishops))
            {
                ulong destinations = _magic.GetBishopMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations & enemyPieces))
                    bishopCaptures.Add(CreateMove(PieceType.Bishop, from, to));
            }

            return bishopCaptures;
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

            foreach (Square from in Bitboards.ToSquares(rooks))
            {
                ulong destinations = _magic.GetRookMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations))
                    rookMoves.Add(CreateMove(PieceType.Rook, from, to));
            }

            return rookMoves;
        }

        public IEnumerable<MoveWrapper> RookCaptures()
        {
            List<MoveWrapper> rookCaptures = new();
            ulong rooks;
            ulong friendlyPieces;
            ulong enemyPieces;
            if (CurrentState.Turn == Color.White)
            {
                rooks = CurrentBoard.WhiteRooks;
                friendlyPieces = CurrentBoard.WhitePieces;
                enemyPieces = CurrentBoard.BlackPieces;
            }
            else
            {
                rooks = CurrentBoard.BlackRooks;
                friendlyPieces = CurrentBoard.BlackPieces;
                enemyPieces = CurrentBoard.WhitePieces;
            }

            foreach (Square from in Bitboards.ToSquares(rooks))
            {
                ulong destinations = _magic.GetRookMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations & enemyPieces))
                    rookCaptures.Add(CreateMove(PieceType.Rook, from, to));
            }

            return rookCaptures;
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

            foreach (Square from in Bitboards.ToSquares(queens))
            {
                ulong destinations = _magic.GetRookMoves(from, CurrentBoard.AllPieces, friendlyPieces) | _magic.GetBishopMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations))
                    queenMoves.Add(CreateMove(PieceType.Queen, from, to));
            }

            return queenMoves;
        }

        public IEnumerable<MoveWrapper> QueenCaptures()
        {
            List<MoveWrapper> queenCaptures = new();
            ulong queens;
            ulong friendlyPieces;
            ulong enemyPieces;
            if (CurrentState.Turn == Color.White)
            {
                queens = CurrentBoard.WhiteQueens;
                friendlyPieces = CurrentBoard.WhitePieces;
                enemyPieces = CurrentBoard.BlackPieces;
            }
            else
            {
                queens = CurrentBoard.BlackQueens;
                friendlyPieces = CurrentBoard.BlackPieces;
                enemyPieces = CurrentBoard.WhitePieces;
            }

            foreach (Square from in Bitboards.ToSquares(queens))
            {
                ulong destinations = _magic.GetRookMoves(from, CurrentBoard.AllPieces, friendlyPieces) | _magic.GetBishopMoves(from, CurrentBoard.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations & enemyPieces))
                    queenCaptures.Add(CreateMove(PieceType.Queen, from, to));
            }

            return queenCaptures;
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

            Square kingSquare = Bitboards.LSB(king);
            IEnumerable<Square> potentialSquares = Bitboards.ToSquares(AttackTables.KingAttacks[(int)kingSquare]);
            foreach (Square to in potentialSquares)
                if ((Bitboards.From(to) & friendlyPieces) == 0)
                    kingMoves.Add(CreateMove(PieceType.King, kingSquare, to));

            kingMoves.AddRange(CastlingMoves());

            return kingMoves;
        }

        public IEnumerable<MoveWrapper> KingCaptures()
        {
            List<MoveWrapper> kingCaptures = new();
            ulong king;
            ulong enemyPieces;
            if (CurrentState.Turn == Color.White)
            {
                king = CurrentBoard.WhiteKing;
                enemyPieces = CurrentBoard.BlackPieces;
            }
            else
            {
                king = CurrentBoard.BlackKing;
                enemyPieces = CurrentBoard.WhitePieces;
            }

            Square kingSquare = Bitboards.LSB(king);
            IEnumerable<Square> potentialSquares = Bitboards.ToSquares(AttackTables.KingAttacks[(int)kingSquare]);
            foreach (Square to in potentialSquares)
                if ((Bitboards.From(to) & enemyPieces) != 0)
                    kingCaptures.Add(CreateMove(PieceType.King, kingSquare, to));

            kingCaptures.AddRange(CastlingMoves());

            return kingCaptures;
        }

        private IEnumerable<MoveWrapper> CastlingMoves()
        {
            List<MoveWrapper> castlingMoves = new();

            if (CurrentState.Turn == Color.White)
            {
                if (CurrentState.CastlingRights.HasFlag(CastlingRights.WhiteKingSide) && (CurrentBoard.AllPieces & 0x60UL) == 0)
                    castlingMoves.Add(CreateMove(PieceType.King, Square.E1, Square.G1, MoveType.Castling));

                if (CurrentState.CastlingRights.HasFlag(CastlingRights.WhiteQueenSide) && (CurrentBoard.AllPieces & 0xEUL) == 0)
                    castlingMoves.Add(CreateMove(PieceType.King, Square.E1, Square.C1, MoveType.Castling));
            }
            else
            {
                if (CurrentState.CastlingRights.HasFlag(CastlingRights.BlackKingSide) && (CurrentBoard.AllPieces & 0x6000000000000000UL) == 0)
                    castlingMoves.Add(CreateMove(PieceType.King, Square.E8, Square.G8, MoveType.Castling));

                if (CurrentState.CastlingRights.HasFlag(CastlingRights.BlackQueenSide) && (CurrentBoard.AllPieces & 0xE00000000000000UL) == 0)
                    castlingMoves.Add(CreateMove(PieceType.King, Square.E8, Square.C8, MoveType.Castling));
            }

            return castlingMoves;
        }

        public bool KingInCheck(Color kingColor)
        {
            return kingColor == Color.White ? AttackedByBlack(Bitboards.LSB(CurrentBoard.WhiteKing)) : AttackedByWhite(Bitboards.LSB(CurrentBoard.BlackKing));
        }

        public bool KingInCheck()
        {
            return CurrentState.Turn == Color.White ? AttackedByWhite(Bitboards.LSB(CurrentBoard.BlackKing)) : AttackedByBlack(Bitboards.LSB(CurrentBoard.WhiteKing));
        }

        public bool CastlingThroughCheck(MoveWrapper move)
        {
            File file = File.E;
            Rank rank = Ranks.Of(move.Move.From());
            File fileTo = Files.Of(move.Move.To());

            int dx = fileTo == File.G ? 1 : -1;
            while (file != fileTo)
            {
                if (CurrentState.Turn == Color.White ? AttackedByWhite(Squares.Of(file, rank)) : AttackedByBlack(Squares.Of(file, rank)))
                    return true;

                file += dx;
            }
            return CurrentState.Turn == Color.White ? AttackedByWhite(Squares.Of(file, rank)) : AttackedByBlack(Squares.Of(file, rank));
        }

        public bool AttackedByBlack(Square square)
        {
            if ((AttackTables.KnightAttacks[(int)square] & CurrentBoard.BlackKnights) != 0) return true;
            if ((AttackTables.KingAttacks[(int)square] & CurrentBoard.BlackKing) != 0) return true;
            if ((_magic.GetBishopMoves(square, CurrentBoard.AllPieces, CurrentBoard.WhitePieces) & (CurrentBoard.BlackBishops | CurrentBoard.BlackQueens)) != 0) return true;
            if ((_magic.GetRookMoves(square, CurrentBoard.AllPieces, CurrentBoard.WhitePieces) & (CurrentBoard.BlackRooks | CurrentBoard.BlackQueens)) != 0) return true;
            if (((((CurrentBoard.BlackPawns & ~Files.BitboardH) >> 7) | ((CurrentBoard.BlackPawns & ~Files.BitboardA) >> 9)) & Bitboards.From(square)) != 0) return true;
            return false;
        }

        public bool AttackedByWhite(Square square)
        {
            if ((AttackTables.KnightAttacks[(int)square] & CurrentBoard.WhiteKnights) != 0) return true;
            if ((AttackTables.KingAttacks[(int)square] & CurrentBoard.WhiteKing) != 0) return true;
            if ((_magic.GetBishopMoves(square, CurrentBoard.AllPieces, CurrentBoard.BlackPieces) & (CurrentBoard.WhiteBishops | CurrentBoard.WhiteQueens)) != 0) return true;
            if ((_magic.GetRookMoves(square, CurrentBoard.AllPieces, CurrentBoard.BlackPieces) & (CurrentBoard.WhiteRooks | CurrentBoard.WhiteQueens)) != 0) return true;
            if (((((CurrentBoard.WhitePawns & ~Files.BitboardA) << 7) | ((CurrentBoard.WhitePawns & ~Files.BitboardH) << 9)) & Bitboards.From(square)) != 0) return true;
            return false;
        }

        public bool IsInCheck(MoveWrapper move)
        {
            if (move.Move.IsCastling()) return CastlingThroughCheck(move);
            else return KingInCheck();
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
                        ulong bit = Bitboards.From(square);

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
                previousKey: null,
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