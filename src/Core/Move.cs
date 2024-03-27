using Types;

namespace Core
{
    public enum MoveType
    {
        Normal,
        Promotion = 1 << 14,
        EnPassant = 2 << 14,
        Castling = 3 << 14
    };

    /*
    Move represented as a 16-bit unsigned integer.
    Bits 0 - 5 : Destination square
    Bits 6-11 : Origin square
    Bits 12-13 : Promotion piece type
    Bits 14-15 : Special move type
    */
    public class Move
    {
        private readonly ushort _data;
        public Move(Square from, Square to, MoveType moveType = MoveType.Normal, PieceType promotionPieceType = PieceType.None)
        {
            if (moveType == MoveType.Promotion)
            {
                // 2 subtracted from promotionPiece to fit inside the short (pieces valid for promotion enumerated starting at 2)
                _data = (ushort)((int)moveType | ((int)promotionPieceType - 2) << 12 | ((int)from << 6) | (int)to);
            }
            else
            {
                _data = (ushort)((int)moveType | (int)from << 6 | (int)to);
            }
        }

        public Move(Position position, string uciMove)
        {
            Square from = Squares.FromString(uciMove[..2]);
            Square to = Squares.FromString(uciMove.Substring(2, 2));
            PieceType pieceType = position.Board.TypeAtSquare(from);
            MoveType moveType = MoveType.Normal;
            PieceType promotionPieceType = PieceType.None;


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
                else if (position.State.EnPassantSquare == to) moveType = MoveType.EnPassant;
            }


            if (moveType == MoveType.Promotion)
            {
                // 2 subtracted from promotionPiece to fit inside the short (pieces valid for promotion enumerated starting at 2)
                _data = (ushort)((int)moveType | ((int)promotionPieceType - 2) << 12 | ((int)from << 6) | (int)to);
            }
            else
            {
                _data = (ushort)((int)moveType | (int)from << 6 | (int)to);
            }
        }

        public Square From()
        {
            return (Square)((_data >> 6) & 0x3F);
        }

        public Square To()
        {
            return (Square)(_data & 0x3F);
        }

        public MoveType TypeOfMove()
        {
            return (MoveType)(_data & (3 << 14));
        }

        public PieceType PromotionPieceType()
        {
            if (TypeOfMove() == MoveType.Promotion)
            {
                return (PieceType)(((_data >> 12) & 0x03) + 2);
            }
            return PieceType.None;
        }

        public bool IsPromotion() => TypeOfMove() == MoveType.Promotion;
        public bool IsEnPassant() => TypeOfMove() == MoveType.EnPassant;
        public bool IsCastling() => TypeOfMove() == MoveType.Castling;

        public static bool operator ==(Move move1, Move move2)
        {
            return move1._data == move2._data;
        }

        public static bool operator !=(Move move1, Move move2)
        {
            return move1._data != move2._data;
        }

        public override int GetHashCode()
        {
            return _data;
        }

        public override bool Equals(object? other)
        {
            return other is Move move && _data == move._data;
        }

        public override string ToString()
        {
            string move = From().ToString() + To().ToString();
            return move.ToLower();
        }
    }
}