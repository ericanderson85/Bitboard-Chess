using Types;
using File = Types.File;

namespace Core
{
    public class Position
    {
        public Board Board;
        public State State;
        public ulong ZobristKey;
        public Position()
        {
            Board = new(
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
            State = new(
                previous: null,
                previousKey: null,
                turn: Color.White,
                castlingRights: CastlingRights.All,
                enPassantSquare: null,
                halfMoveClock: 0,
                moveCount: 0
            );
            ZobristKey = Zobrist.CalculateHash(Board, State);
        }

        public Position(string fen)
        {
            string[] parts = fen.Split(' ');
            if (parts.Length != 6) throw new ArgumentException("FEN must have 6 space-separated fields.", nameof(fen));

            Board = ParseBoardLayout(parts[0]);

            State = ParseStateLayout(parts);

            ZobristKey = Zobrist.CalculateHash(Board, State);
        }

        public void PerformMove(MoveWrapper move)
        {
            Board.Move(move.Move, move.PieceType, move.EnemyPieceType, State.Turn);
            State prevState = State;
            State = NewState(move);
            Zobrist.UpdateHash(ref ZobristKey, move, prevState, State);
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
            if (State.Previous == null || State.PreviousKey == null)
                throw new Exception("Can't undo move");

            ZobristKey = (ulong)State.PreviousKey;
            State = State.Previous;
            Board.UndoMove(move.Move, move.PieceType, move.EnemyPieceType, State.Turn);
        }

        private State NewState(MoveWrapper move)
        {
            CastlingRights castlingRights = State.CastlingRights;

            if (move.PieceType == PieceType.King)
                castlingRights &= State.Turn == Color.White ? ~CastlingRights.WhiteCastling : ~CastlingRights.BlackCastling;
            else if (move.PieceType == PieceType.Rook)
            {
                if (State.Turn == Color.White)
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
                if (State.Turn == Color.White)
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
                previous: State,
                previousKey: ZobristKey,
                turn: State.Turn ^ Color.Black,
                castlingRights: castlingRights,
                enPassantSquare: move.EnPassantSquare,
                halfMoveClock: resetClock ? 0 : State.HalfMoveClock + 1,
                moveCount: State.FullMoveCount + (int)State.Turn
            );

        }

        public string ToFEN()
        {
            return Board.BoardFEN().Append(State.StateFEN()).ToString();
        }

        public List<MoveWrapper> AllMoves()
        {
            return PseudoLegalMoveGenerator.AllMoves(this);
        }

        public List<MoveWrapper> LoudMoves()
        {
            return PseudoLegalMoveGenerator.LoudMoves(this);
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
            return Board.ToString() + "\n" + ToFEN();
        }
    }
}