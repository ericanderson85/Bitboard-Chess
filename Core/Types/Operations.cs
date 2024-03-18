namespace Types
{
    public static class Operations
    {
        public static CastlingRights And(CastlingRights castlingRights, Color color)
        {
            return (color == Color.White ? CastlingRights.WhiteCastling : CastlingRights.BlackCastling) & castlingRights;
        }
        public static int Add(Direction direction1, Direction direction2)
        {
            return (int)direction1 + (int)direction2;
        }
        public static Square Add(Square square, Direction direction)
        {
            return (Square)((int)square + (int)direction);
        }
        public static Square Subtract(Square square, Direction direction)
        {
            return (Square)((int)square - (int)direction);

        }
        public static Direction Multiply(int i, Direction direction)
        {
            return (Direction)(i * (int)direction);
        }
        public static Square FlipFile(Square square)
        {
            return square ^ Square.H1;
        }
        public static Square FlipRank(Square square)
        {
            return square ^ Square.A8;
        }
    }
}