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
                case PieceType.Pawn: return ref turn == Color.White ? ref WhitePawns : ref BlackPawns;
                case PieceType.Knight: return ref turn == Color.White ? ref WhiteKnights : ref BlackKnights;
                case PieceType.Bishop: return ref turn == Color.White ? ref WhiteBishops : ref BlackBishops;
                case PieceType.Rook: return ref turn == Color.White ? ref WhiteRooks : ref BlackRooks;
                case PieceType.Queen: return ref turn == Color.White ? ref WhiteQueens : ref BlackQueens;
                case PieceType.King: return ref turn == Color.White ? ref WhiteKing : ref BlackKing;
                default: return ref turn == Color.White ? ref WhitePieces : ref BlackPieces;
            }
        }

        public void Move(Move move, PieceType pieceType, PieceType enemyPieceType, Color turn)
        {
            if (move.TypeOfMove() == MoveType.Normal)
            {
                if (enemyPieceType != PieceType.None)
                {
                    Capture(move, turn, ref Pieces(pieceType, turn), ref Pieces(enemyPieceType, turn ^ Color.Black));
                }
                else
                {
                    Move(move, turn, ref Pieces(pieceType, turn));
                }
            }
            else if (move.TypeOfMove() == MoveType.Castling)
            {
                Castle(move, turn);
            }
            else if (move.TypeOfMove() == MoveType.EnPassant)
            {
                EnPassant(move, turn);
            }
            else
            {
                Promote(move, enemyPieceType, turn);
            }
        }

        private void Move(Move move, Color turn, ref ulong pieces)
        {
            RemovePiece(ref pieces, move.From());
            AddPiece(ref pieces, turn, move.To());
        }

        private void Capture(Move move, Color turn, ref ulong pieces, ref ulong enemyPieces)
        {
            RemovePiece(ref pieces, move.From());
            RemovePiece(ref enemyPieces, move.To());
            AddPiece(ref pieces, turn, move.To());
        }

        private void Castle(Move move, Color turn)
        {
            File oldRookFile = Files.Of(move.To()) == File.C ? File.A : File.H;
            File newRookFile = Files.Of(move.To()) == File.C ? File.D : File.F;

            if (turn == Color.White)
            {
                RemovePiece(ref WhiteKing, move.From());
                RemovePiece(ref WhiteRooks, Squares.Of(oldRookFile, Rank.One));
                AddPiece(ref WhiteKing, turn, move.To());
                AddPiece(ref WhiteRooks, turn, Squares.Of(newRookFile, Rank.One));
            }
            else
            {
                RemovePiece(ref BlackKing, move.From());
                RemovePiece(ref BlackRooks, Squares.Of(oldRookFile, Rank.Eight));
                AddPiece(ref BlackKing, turn, move.To());
                AddPiece(ref BlackRooks, turn, Squares.Of(newRookFile, Rank.Eight));
            }
        }

        private void EnPassant(Move move, Color turn)
        {
            if (turn == Color.White)
            {
                RemovePiece(ref WhitePawns, move.From());
                RemovePiece(ref BlackPawns, move.To() - 8);
                AddPiece(ref WhitePawns, turn, move.To());
            }
            else
            {
                RemovePiece(ref BlackPawns, move.From());
                RemovePiece(ref WhitePawns, move.To() + 8);
                AddPiece(ref BlackPawns, turn, move.To());
            }
        }

        private void Promote(Move move, PieceType enemyPieceType, Color turn)
        {
            if (enemyPieceType == PieceType.None)
            {
                RemovePiece(ref turn == Color.White ? ref WhitePawns : ref BlackPawns, move.From());
                AddPiece(ref Pieces(move.PromotionPieceType(), turn), turn, move.To());
            }
            else
            {
                RemovePiece(ref turn == Color.White ? ref WhitePawns : ref BlackPawns, move.From());
                RemovePiece(ref Pieces(enemyPieceType, turn ^ Color.Black), move.To());
                AddPiece(ref Pieces(move.PromotionPieceType(), turn), turn, move.To());
            }
        }

        public void UndoMove(Move move, PieceType pieceType, PieceType enemyPieceType, Color turn)
        {
            if (move.TypeOfMove() == MoveType.Normal)
            {
                if (enemyPieceType != PieceType.None)
                {
                    UndoCapture(move, turn, ref Pieces(pieceType, turn), ref Pieces(enemyPieceType, turn ^ Color.Black));
                }
                else
                {
                    UndoMove(move, turn, ref Pieces(pieceType, turn));
                }
            }
            else if (move.TypeOfMove() == MoveType.Castling)
            {
                UndoCastle(move, turn);
            }
            else if (move.TypeOfMove() == MoveType.EnPassant)
            {
                UndoEnPassant(move, turn);
            }
            else if (move.TypeOfMove() == MoveType.Promotion)
            {
                UndoPromote(move, turn, enemyPieceType);
            }
        }

        private void UndoMove(Move move, Color turn, ref ulong pieces)
        {
            RemovePiece(ref pieces, move.To());
            AddPiece(ref pieces, turn, move.From());
        }

        private void UndoCapture(Move move, Color turn, ref ulong pieces, ref ulong enemyPieces)
        {
            RemovePiece(ref pieces, move.To());
            AddPiece(ref pieces, turn, move.From());
            AddPiece(ref enemyPieces, turn ^ Color.Black, move.To());

        }

        private void UndoCastle(Move move, Color turn)
        {
            File oldRookFile = Files.Of(move.To()) == File.C ? File.D : File.F;
            File newRookFile = Files.Of(move.To()) == File.C ? File.A : File.H;

            if (turn == Color.White)
            {
                RemovePiece(ref WhiteKing, move.To());
                RemovePiece(ref WhiteRooks, Squares.Of(oldRookFile, Rank.One));
                AddPiece(ref WhiteKing, turn, move.From());
                AddPiece(ref WhiteRooks, turn, Squares.Of(newRookFile, Rank.One));
            }
            else
            {
                RemovePiece(ref BlackKing, move.To());
                RemovePiece(ref BlackRooks, Squares.Of(oldRookFile, Rank.Eight));
                AddPiece(ref BlackKing, turn, move.From());
                AddPiece(ref BlackRooks, turn, Squares.Of(newRookFile, Rank.Eight));
            }
        }

        private void UndoEnPassant(Move move, Color turn)
        {
            if (turn == Color.White)
            {
                RemovePiece(ref WhitePawns, move.To());
                AddPiece(ref WhitePawns, turn, move.From());
                AddPiece(ref BlackPawns, turn ^ Color.Black, move.To() - 8);
            }
            else
            {
                RemovePiece(ref BlackPawns, move.To());
                AddPiece(ref BlackPawns, turn, move.From());
                AddPiece(ref WhitePawns, turn ^ Color.Black, move.To() + 8);
            }
        }

        private void UndoPromote(Move move, Color turn, PieceType enemyPieceType)
        {
            if (enemyPieceType == PieceType.None)
            {
                RemovePiece(ref Pieces(move.PromotionPieceType(), turn), move.To());
                AddPiece(ref turn == Color.White ? ref WhitePawns : ref BlackPawns, turn, move.From());
            }
            else
            {
                RemovePiece(ref Pieces(move.PromotionPieceType(), turn), move.To());
                AddPiece(ref turn == Color.White ? ref WhitePawns : ref BlackPawns, turn, move.From());
                AddPiece(ref Pieces(enemyPieceType, turn ^ Color.Black), turn ^ Color.Black, move.To());
            }
        }
        private void RemovePiece(ref ulong pieces, Square square)
        {
            ulong squareBitboard = Bitboards.From(square);
            pieces &= ~squareBitboard;
            WhitePieces &= ~squareBitboard;
            BlackPieces &= ~squareBitboard;
            AllPieces &= ~squareBitboard;
        }

        private void AddPiece(ref ulong pieces, Color turn, Square square)
        {
            ulong squareBitboard = Bitboards.From(square);
            pieces |= squareBitboard;
            if (turn == Color.White) WhitePieces |= squareBitboard;
            else BlackPieces |= squareBitboard;
            AllPieces |= squareBitboard;
        }

        public PieceType TypeAtSquare(Square square)
        {
            ulong bitboardSquare = Bitboards.From(square);
            if ((bitboardSquare & AllPieces) == 0)
                return PieceType.None;
            if ((bitboardSquare & (WhitePawns | BlackPawns)) != 0)
                return PieceType.Pawn;
            if ((bitboardSquare & (WhiteKnights | BlackKnights)) != 0)
                return PieceType.Knight;
            if ((bitboardSquare & (WhiteBishops | BlackBishops)) != 0)
                return PieceType.Bishop;
            if ((bitboardSquare & (WhiteRooks | BlackRooks)) != 0)
                return PieceType.Rook;
            if ((bitboardSquare & (WhiteQueens | BlackQueens)) != 0)
                return PieceType.Queen;
            return PieceType.King;
        }

        public override string ToString()
        {
            char piece;
            string output = "+---+---+---+---+---+---+---+---+\n";
            for (Rank rank = Rank.Eight; rank >= Rank.One; rank--)
            {
                for (File file = File.A; file <= File.H; file++)
                {
                    ulong mask = Bitboards.From(file, rank);
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
            output += "  a   b   c   d   e   f   g   h";
            return output;
        }
    }
}