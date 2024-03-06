using Types;

namespace Chess
{
    public class Board
    {
        public Bitboard[] Pieces;

        public Board()
        {
            Pieces = new Bitboard[12];
            InitializePiecesToStartingPositions();
        }

        private void InitializePiecesToStartingPositions()
        {
            Pieces[0] = new(Constants.WhitePawnStartingPosition);
            Pieces[1] = new(Constants.WhiteKnightStartingPosition);
            Pieces[2] = new(Constants.WhiteBishopStartingPosition);
            Pieces[3] = new(Constants.WhiteRookStartingPosition);
            Pieces[4] = new(Constants.WhiteQueenStartingPosition);
            Pieces[5] = new(Constants.WhiteKingStartingPosition);
            Pieces[6] = new(Constants.BlackPawnStartingPosition);
            Pieces[7] = new(Constants.BlackKnightStartingPosition);
            Pieces[8] = new(Constants.BlackBishopStartingPosition);
            Pieces[9] = new(Constants.BlackRookStartingPosition);
            Pieces[10] = new(Constants.BlackQueenStartingPosition);
            Pieces[11] = new(Constants.BlackKingStartingPosition);
        }



        public char GetPieceChar(Bitboard bitboard)
        {
            char[] pieceChars = new char[] { 'P', 'N', 'B', 'R', 'Q', 'K', 'p', 'n', 'b', 'r', 'q', 'k' };
            for (int i = 0; i < Pieces.Length; i++)
            {
                if ((Pieces[i] & bitboard).Data != 0)
                {
                    return pieceChars[i];
                }
            }
            return ' ';
        }

        public override string ToString()
        {
            string s = "+---+---+---+---+---+---+---+---+\n";

            for (int rank = 7; rank >= 0; --rank)
            {
                for (int file = 0; file < 8; ++file)
                {
                    Bitboard bitboard = new(1UL << (rank * 8 + file));
                    char piece = GetPieceChar(bitboard);
                    s += piece != ' ' ? $"| {piece} " : "|   ";
                }
                s += "| " + (rank + 1) + "\n+---+---+---+---+---+---+---+---+\n";
            }
            s += "  a   b   c   d   e   f   g   h\n";

            return s;
        }
    }
}