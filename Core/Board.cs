using Types;
using File = Types.File;

namespace Core
{
    public class Board
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

        public ref ulong Pieces(PieceType pieceType, Color turn)
        {

            switch (pieceType)
            {
                case PieceType.Pawn:
                    return ref turn == Color.White ? ref WhitePawns : ref BlackPawns;
                case PieceType.Knight:
                    return ref turn == Color.White ? ref WhiteKnights : ref BlackKnights;
                case PieceType.Bishop:
                    return ref turn == Color.White ? ref WhiteBishops : ref BlackBishops;
                case PieceType.Rook:
                    return ref turn == Color.White ? ref WhiteRooks : ref BlackRooks;
                case PieceType.Queen:
                    return ref turn == Color.White ? ref WhiteQueens : ref BlackQueens;
                case PieceType.King:
                    return ref turn == Color.White ? ref WhiteKing : ref BlackKing;
                default:
                    return ref AllPieces;
            }
        }

        public void Move(Move move, PieceType pieceType, PieceType enemyPieceType, Color turn)
        {

            if (move.TypeOfMove() == MoveType.Normal)
            {
                ref ulong pieces = ref Pieces(pieceType, turn);
                if (enemyPieceType != PieceType.None)
                {
                    ref ulong enemyPieces = ref Pieces(enemyPieceType, ~turn);
                    Capture(move, ref pieces, ref enemyPieces);
                }
                else
                {
                    Move(move, ref pieces);
                }
            }

        }
        private void Move(Move move, ref ulong pieces)
        {
            RemovePiece(ref pieces, move.From());
            AddPiece(ref pieces, move.To());
        }

        private void Capture(Move move, ref ulong pieces, ref ulong enemyPieces)
        {
            RemovePiece(ref pieces, move.From());
            RemovePiece(ref enemyPieces, move.To());
            AddPiece(ref pieces, move.To());
        }

        private void RemovePiece(ref ulong pieces, Square square)
        {
            ulong squareBitboard = Bitboards.FromSquare(square);
            pieces &= ~squareBitboard;
            WhitePieces &= ~squareBitboard;
            BlackPieces &= ~squareBitboard;
            AllPieces &= ~squareBitboard;
        }

        private void AddPiece(ref ulong pieces, Square square)
        {
            ulong squareBitboard = Bitboards.FromSquare(square);
            pieces |= squareBitboard;
            WhitePieces |= squareBitboard;
            BlackPieces |= squareBitboard;
            AllPieces |= squareBitboard;
        }


        public override string ToString()
        {
            char piece;
            string output = "+---+---+---+---+---+---+---+---+\n";
            for (Rank rank = Rank.Eight; rank >= Rank.One; rank--)
            {
                for (File file = File.A; file <= File.H; file++)
                {
                    ulong mask = Bitboards.FromSquare(file, rank);
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