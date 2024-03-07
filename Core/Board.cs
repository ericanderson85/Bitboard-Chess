using Types;
using File = Types.File;

namespace Core
{
    public struct Board
    {
        public ulong WhitePawns; public ulong WhiteKnights; public ulong WhiteBishops; public ulong WhiteRooks; public ulong WhiteQueens; public ulong WhiteKing;
        public ulong BlackPawns; public ulong BlackKnights; public ulong BlackBishops; public ulong BlackRooks; public ulong BlackQueens; public ulong BlackKing;
        public ulong WhitePieces; public ulong BlackPieces; public ulong AllPieces;
        public Board(ulong P, ulong N, ulong B, ulong R, ulong Q, ulong K,  // White pieces
                     ulong p, ulong n, ulong b, ulong r, ulong q, ulong k)  // Black pieces
        {
            WhitePawns = P; WhiteKnights = N; WhiteBishops = B; WhiteRooks = R; WhiteQueens = Q; WhiteKing = K;
            BlackPawns = p; BlackKnights = n; BlackBishops = b; BlackRooks = r; BlackQueens = q; BlackKing = k;
            WhitePieces = P | N | B | R | Q | K;
            BlackPieces = p | n | b | r | q | k;
            AllPieces = WhitePieces | BlackPieces;
        }

        public override string ToString()
        {
            char piece;
            string output = "+---+---+---+---+---+---+---+---+\n";
            for (Rank rank = Rank.Eight; rank >= Rank.One; rank--)
            {
                for (File file = File.A; file <= File.H; file++)
                {
                    ulong mask = Bitboards.Of(file, rank);
                    piece = ' ';
                    if ((WhitePawns & mask) != 0) piece = 'P';
                    else if ((WhiteKnights & mask) != 0) piece = 'N';
                    else if ((WhiteBishops & mask) != 0) piece = 'B';
                    else if ((WhiteRooks & mask) != 0) piece = 'R';
                    else if ((WhiteQueens & mask) != 0) piece = 'Q';
                    else if ((WhiteKing & mask) != 0) piece = 'K';
                    else if ((BlackPawns & mask) != 0) piece = 'p';
                    else if ((BlackKnights & mask) != 0) piece = 'n';
                    else if ((BlackBishops & mask) != 0) piece = 'b';
                    else if ((BlackRooks & mask) != 0) piece = 'r';
                    else if ((BlackQueens & mask) != 0) piece = 'q';
                    else if ((BlackKing & mask) != 0) piece = 'k';

                    output += "| " + piece + " ";
                }
                output += "| " + ((int)rank + 1) + "\n+---+---+---+---+---+---+---+---+\n";
            }
            output += "  a   b   c   d   e   f   g   h\n";
            return output;
        }
    }
}