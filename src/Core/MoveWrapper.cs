using System.Diagnostics.CodeAnalysis;
using Types;
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

        public bool IsCapture()
        {
            return EnemyPieceType != PieceType.None;
        }

        public bool Equals(MoveWrapper? otherMove)
        {
            if (otherMove == null) return false;

            MoveWrapper otherMoveNotNull = (MoveWrapper)otherMove;

            return Move == otherMoveNotNull.Move && EnPassantSquare == otherMoveNotNull.EnPassantSquare &&
            EnemyPieceType == otherMoveNotNull.EnemyPieceType && PieceType == otherMoveNotNull.PieceType;
        }

        public static MoveWrapper From(Position position, Move move)
        {
            if (move.IsEnPassant())
                return new(move, PieceType.Pawn, PieceType.Pawn);

            if (move.IsCastling())
                return new(move, PieceType.King, PieceType.None);

            if (move.IsPromotion())
                return new(move, PieceType.Pawn, position.Board.TypeAtSquare(move.To()));

            PieceType pieceType = position.Board.TypeAtSquare(move.From());

            if (pieceType == PieceType.Pawn)
            {
                if (move.To() == move.From() + 16) return new(move, PieceType.Pawn, PieceType.None, move.From() + 8);
                if (move.To() == move.From() - 16) return new(move, PieceType.Pawn, PieceType.None, move.From() - 8);
            }

            PieceType enemyPieceType = position.Board.TypeAtSquare(move.To());

            return new(move, pieceType, enemyPieceType);
        }

        public static MoveWrapper CreateMove(Position position, PieceType pieceType, Square from, Square to, MoveType moveType = MoveType.Normal, PieceType promotionPieceType = PieceType.None, Square? enPassantSquare = null)
        {
            Board board = position.Board;
            return new(
                pieceType: pieceType,
                move: new Move(from, to, moveType, promotionPieceType),
                enemyPieceType: board.TypeAtSquare(to),
                enPassantSquare: enPassantSquare
            );
        }

        public static MoveWrapper CreateMove(Position position, string uciMove)
        {
            Board board = position.Board;
            State state = position.State;
            Square from = Squares.FromString(uciMove[..2]);
            Square to = Squares.FromString(uciMove.Substring(2, 2));
            PieceType pieceType = board.TypeAtSquare(from);
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
                else if (state.EnPassantSquare == to) moveType = MoveType.EnPassant;
                else if (int.Abs(from - to) == 16) enPassantSquare = to + (to - from) / 2;
            }

            return CreateMove(position, pieceType, from, to, moveType, promotionPieceType, enPassantSquare);
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
}