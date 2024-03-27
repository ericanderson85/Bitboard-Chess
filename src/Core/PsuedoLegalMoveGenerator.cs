using Types;

namespace Core
{
    public static class PseudoLegalMoveGenerator
    {
        public static ulong PawnAttacks(Position position, Color turn)
        {
            ulong pawnAttacks;
            Board board = position.Board;
            if (turn == Color.White)
                pawnAttacks = board.WhitePawns & ~Files.BitboardA << 7 | board.WhitePawns & ~Files.BitboardH << 9;
            else
                pawnAttacks = board.BlackPawns & ~Files.BitboardA >> 9 | board.BlackPawns & ~Files.BitboardH >> 7;
            return pawnAttacks;
        }

        public static List<MoveWrapper> AllMoves(Position position)
        {
            return PawnMoves(position).Concat(KnightMoves(position)).Concat(BishopMoves(position)).Concat(RookMoves(position)).Concat(QueenMoves(position)).Concat(KingMoves(position)).ToList();
        }

        public static List<MoveWrapper> LoudMoves(Position position)
        {
            return LoudPawnMoves(position).Concat(KnightCaptures(position)).Concat(BishopCaptures(position)).Concat(RookCaptures(position)).Concat(QueenCaptures(position)).Concat(KingCaptures(position)).ToList();
        }

        public static IEnumerable<MoveWrapper> PawnMoves(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            return state.Turn == Color.White ? WhitePawnMoves(position).Concat(LoudWhitePawnMoves(position)) : BlackPawnMoves(position).Concat(LoudBlackPawnMoves(position));
        }

        public static IEnumerable<MoveWrapper> WhitePawnMoves(Position position)
        {
            Board board = position.Board;
            List<MoveWrapper> pawnMoves = new();

            ulong singlePush = ((board.WhitePawns & ~Ranks.BitboardSeven) << 8) & ~board.AllPieces;
            foreach (Square to in Bitboards.ToSquares(singlePush))
                pawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to - 8, to));

            ulong doublePush = ((singlePush & Ranks.BitboardThree) << 8) & ~board.AllPieces;
            foreach (Square to in Bitboards.ToSquares(doublePush))
                pawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to - 16, to, enPassantSquare: to - 8));

            return pawnMoves;
        }

        public static IEnumerable<MoveWrapper> BlackPawnMoves(Position position)
        {
            Board board = position.Board;
            List<MoveWrapper> pawnMoves = new();

            ulong singlePush = ((board.BlackPawns & ~Ranks.BitboardTwo) >> 8) & ~board.AllPieces;
            foreach (Square to in Bitboards.ToSquares(singlePush))
                pawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to + 8, to));

            ulong doublePush = ((singlePush & Ranks.BitboardSix) >> 8) & ~board.AllPieces;
            foreach (Square to in Bitboards.ToSquares(doublePush))
                pawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to + 16, to, enPassantSquare: to + 8));

            return pawnMoves;
        }


        public static IEnumerable<MoveWrapper> LoudPawnMoves(Position position)
        {
            State state = position.State;
            return state.Turn == Color.White ? LoudWhitePawnMoves(position) : LoudBlackPawnMoves(position);
        }

        public static IEnumerable<MoveWrapper> LoudWhitePawnMoves(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> loudWhiteMoves = new();


            ulong promotionPush = ((board.WhitePawns & Ranks.BitboardSeven) << 8) & ~board.AllPieces;
            foreach (Square to in Bitboards.ToSquares(promotionPush))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudWhiteMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to - 8, to, MoveType.Promotion, promotionPieceType));

            ulong captureWest = ((board.WhitePawns & ~Files.BitboardA & ~Ranks.BitboardSeven) << 7) & board.BlackPieces;
            foreach (Square to in Bitboards.ToSquares(captureWest))
                loudWhiteMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to - 7, to));

            ulong promotionCaptureWest = ((board.WhitePawns & ~Files.BitboardA & Ranks.BitboardSeven) << 7) & board.BlackPieces;
            foreach (Square to in Bitboards.ToSquares(promotionCaptureWest))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudWhiteMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to - 7, to, MoveType.Promotion, promotionPieceType));

            ulong captureEast = ((board.WhitePawns & ~Files.BitboardH & ~Ranks.BitboardSeven) << 9) & board.BlackPieces;
            foreach (Square to in Bitboards.ToSquares(captureEast))
                loudWhiteMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to - 9, to));

            ulong promotionCaptureEast = ((board.WhitePawns & ~Files.BitboardH & Ranks.BitboardSeven) << 9) & board.BlackPieces;
            foreach (Square to in Bitboards.ToSquares(promotionCaptureEast))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudWhiteMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to - 9, to, MoveType.Promotion, promotionPieceType));

            if (state.EnPassantSquare != null)
            {
                ulong enPassant = Bitboards.From((Square)state.EnPassantSquare);

                if ((((board.WhitePawns & ~Files.BitboardA) << 7) & enPassant) != 0)
                    loudWhiteMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, (Square)state.EnPassantSquare - 7, (Square)state.EnPassantSquare, MoveType.EnPassant));

                if ((((board.WhitePawns & ~Files.BitboardH) << 9) & enPassant) != 0)
                    loudWhiteMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, (Square)state.EnPassantSquare - 9, (Square)state.EnPassantSquare, MoveType.EnPassant));
            }


            return loudWhiteMoves;
        }

        public static IEnumerable<MoveWrapper> LoudBlackPawnMoves(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> loudPawnMoves = new();

            ulong promotionPush = ((board.BlackPawns & Ranks.BitboardTwo) >> 8) & ~board.AllPieces;
            foreach (Square to in Bitboards.ToSquares(promotionPush))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudPawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to + 8, to, MoveType.Promotion, promotionPieceType));

            ulong captureWest = ((board.BlackPawns & ~Files.BitboardH & ~Ranks.BitboardTwo) >> 7) & board.WhitePieces;
            foreach (Square to in Bitboards.ToSquares(captureWest))
                loudPawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to + 7, to));

            ulong promotionCaptureEast = ((board.BlackPawns & ~Files.BitboardH & Ranks.BitboardTwo) >> 7) & board.WhitePieces;
            foreach (Square to in Bitboards.ToSquares(promotionCaptureEast))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudPawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to + 7, to, MoveType.Promotion, promotionPieceType));

            ulong captureEast = ((board.BlackPawns & ~Files.BitboardA & ~Ranks.BitboardTwo) >> 9) & board.WhitePieces;
            foreach (Square to in Bitboards.ToSquares(captureEast))
                loudPawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to + 9, to));

            ulong promotionCaptureWest = ((board.BlackPawns & ~Files.BitboardA & Ranks.BitboardTwo) >> 9) & board.WhitePieces;
            foreach (Square to in Bitboards.ToSquares(promotionCaptureWest))
                for (PieceType promotionPieceType = PieceType.Knight; promotionPieceType <= PieceType.Queen; promotionPieceType++)
                    loudPawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, to + 9, to, MoveType.Promotion, promotionPieceType));

            if (state.EnPassantSquare != null)
            {
                ulong enPassant = Bitboards.From((Square)state.EnPassantSquare);

                if ((((board.BlackPawns & ~Files.BitboardA) >> 9) & enPassant) != 0)
                    loudPawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, (Square)state.EnPassantSquare + 9, (Square)state.EnPassantSquare, MoveType.EnPassant));

                if ((((board.BlackPawns & ~Files.BitboardH) >> 7) & enPassant) != 0)
                    loudPawnMoves.Add(MoveWrapper.CreateMove(position, PieceType.Pawn, (Square)state.EnPassantSquare + 7, (Square)state.EnPassantSquare, MoveType.EnPassant));
            }


            return loudPawnMoves;
        }

        public static IEnumerable<MoveWrapper> KnightMoves(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> knightMoves = new();
            ulong knights;
            ulong friendlyPieces;
            if (state.Turn == Color.White)
            {
                knights = board.WhiteKnights;
                friendlyPieces = board.WhitePieces;
            }
            else
            {
                knights = board.BlackKnights;
                friendlyPieces = board.BlackPieces;
            }

            foreach (Square from in Bitboards.ToSquares(knights))
            {
                IEnumerable<Square> potentialSquares = Bitboards.ToSquares(AttackTables.KnightAttacks[(int)from]);
                foreach (Square to in potentialSquares)
                    if ((Bitboards.From(to) & friendlyPieces) == 0)
                        knightMoves.Add(MoveWrapper.CreateMove(position, PieceType.Knight, from, to));
            }

            return knightMoves;
        }

        public static IEnumerable<MoveWrapper> KnightCaptures(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> knightCaptures = new();
            ulong knights;
            ulong enemyPieces;
            if (state.Turn == Color.White)
            {
                knights = board.WhiteKnights;
                enemyPieces = board.BlackPieces;
            }
            else
            {
                knights = board.BlackKnights;
                enemyPieces = board.WhitePieces;
            }

            foreach (Square from in Bitboards.ToSquares(knights))
            {
                IEnumerable<Square> potentialSquares = Bitboards.ToSquares(AttackTables.KnightAttacks[(int)from]);
                foreach (Square to in potentialSquares)
                    if ((Bitboards.From(to) & enemyPieces) != 0)
                        knightCaptures.Add(MoveWrapper.CreateMove(position, PieceType.Knight, from, to));
            }

            return knightCaptures;
        }

        public static IEnumerable<MoveWrapper> BishopMoves(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> bishopMoves = new();
            ulong bishops;
            ulong friendlyPieces;
            if (state.Turn == Color.White)
            {
                bishops = board.WhiteBishops;
                friendlyPieces = board.WhitePieces;
            }
            else
            {
                bishops = board.BlackBishops;
                friendlyPieces = board.BlackPieces;
            }

            foreach (Square from in Bitboards.ToSquares(bishops))
            {
                ulong destinations = Magic.GetBishopMoves(from, board.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations))
                    bishopMoves.Add(MoveWrapper.CreateMove(position, PieceType.Bishop, from, to));
            }

            return bishopMoves;
        }

        public static IEnumerable<MoveWrapper> BishopCaptures(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> bishopCaptures = new();
            ulong bishops;
            ulong friendlyPieces;
            ulong enemyPieces;
            if (state.Turn == Color.White)
            {
                bishops = board.WhiteBishops;
                friendlyPieces = board.WhitePieces;
                enemyPieces = board.BlackPieces;
            }
            else
            {
                bishops = board.BlackBishops;
                friendlyPieces = board.BlackPieces;
                enemyPieces = board.WhitePieces;
            }

            foreach (Square from in Bitboards.ToSquares(bishops))
            {
                ulong destinations = Magic.GetBishopMoves(from, board.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations & enemyPieces))
                    bishopCaptures.Add(MoveWrapper.CreateMove(position, PieceType.Bishop, from, to));
            }

            return bishopCaptures;
        }

        public static IEnumerable<MoveWrapper> RookMoves(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> rookMoves = new();
            ulong rooks;
            ulong friendlyPieces;
            if (state.Turn == Color.White)
            {
                rooks = board.WhiteRooks;
                friendlyPieces = board.WhitePieces;
            }
            else
            {
                rooks = board.BlackRooks;
                friendlyPieces = board.BlackPieces;
            }

            foreach (Square from in Bitboards.ToSquares(rooks))
            {
                ulong destinations = Magic.GetRookMoves(from, board.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations))
                    rookMoves.Add(MoveWrapper.CreateMove(position, PieceType.Rook, from, to));
            }

            return rookMoves;
        }

        public static IEnumerable<MoveWrapper> RookCaptures(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> rookCaptures = new();
            ulong rooks;
            ulong friendlyPieces;
            ulong enemyPieces;
            if (state.Turn == Color.White)
            {
                rooks = board.WhiteRooks;
                friendlyPieces = board.WhitePieces;
                enemyPieces = board.BlackPieces;
            }
            else
            {
                rooks = board.BlackRooks;
                friendlyPieces = board.BlackPieces;
                enemyPieces = board.WhitePieces;
            }

            foreach (Square from in Bitboards.ToSquares(rooks))
            {
                ulong destinations = Magic.GetRookMoves(from, board.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations & enemyPieces))
                    rookCaptures.Add(MoveWrapper.CreateMove(position, PieceType.Rook, from, to));
            }

            return rookCaptures;
        }

        public static IEnumerable<MoveWrapper> QueenMoves(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> queenMoves = new();
            ulong queens;
            ulong friendlyPieces;
            if (state.Turn == Color.White)
            {
                queens = board.WhiteQueens;
                friendlyPieces = board.WhitePieces;
            }
            else
            {
                queens = board.BlackQueens;
                friendlyPieces = board.BlackPieces;
            }

            foreach (Square from in Bitboards.ToSquares(queens))
            {
                ulong destinations = Magic.GetRookMoves(from, board.AllPieces, friendlyPieces) | Magic.GetBishopMoves(from, board.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations))
                    queenMoves.Add(MoveWrapper.CreateMove(position, PieceType.Queen, from, to));
            }

            return queenMoves;
        }

        public static IEnumerable<MoveWrapper> QueenCaptures(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> queenCaptures = new();
            ulong queens;
            ulong friendlyPieces;
            ulong enemyPieces;
            if (state.Turn == Color.White)
            {
                queens = board.WhiteQueens;
                friendlyPieces = board.WhitePieces;
                enemyPieces = board.BlackPieces;
            }
            else
            {
                queens = board.BlackQueens;
                friendlyPieces = board.BlackPieces;
                enemyPieces = board.WhitePieces;
            }

            foreach (Square from in Bitboards.ToSquares(queens))
            {
                ulong destinations = Magic.GetRookMoves(from, board.AllPieces, friendlyPieces) | Magic.GetBishopMoves(from, board.AllPieces, friendlyPieces);
                foreach (Square to in Bitboards.ToSquares(destinations & enemyPieces))
                    queenCaptures.Add(MoveWrapper.CreateMove(position, PieceType.Queen, from, to));
            }

            return queenCaptures;
        }

        public static IEnumerable<MoveWrapper> KingMoves(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> kingMoves = new();
            ulong king;
            ulong friendlyPieces;
            if (state.Turn == Color.White)
            {
                king = board.WhiteKing;
                friendlyPieces = board.WhitePieces;
            }
            else
            {
                king = board.BlackKing;
                friendlyPieces = board.BlackPieces;
            }

            Square kingSquare = Bitboards.LSB(king);
            IEnumerable<Square> potentialSquares = Bitboards.ToSquares(AttackTables.KingAttacks[(int)kingSquare]);
            foreach (Square to in potentialSquares)
                if ((Bitboards.From(to) & friendlyPieces) == 0)
                    kingMoves.Add(MoveWrapper.CreateMove(position, PieceType.King, kingSquare, to));

            kingMoves.AddRange(CastlingMoves(position));

            return kingMoves;
        }

        public static IEnumerable<MoveWrapper> KingCaptures(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> kingCaptures = new();
            ulong king;
            ulong enemyPieces;
            if (state.Turn == Color.White)
            {
                king = board.WhiteKing;
                enemyPieces = board.BlackPieces;
            }
            else
            {
                king = board.BlackKing;
                enemyPieces = board.WhitePieces;
            }

            Square kingSquare = Bitboards.LSB(king);
            IEnumerable<Square> potentialSquares = Bitboards.ToSquares(AttackTables.KingAttacks[(int)kingSquare]);
            foreach (Square to in potentialSquares)
                if ((Bitboards.From(to) & enemyPieces) != 0)
                    kingCaptures.Add(MoveWrapper.CreateMove(position, PieceType.King, kingSquare, to));

            kingCaptures.AddRange(CastlingMoves(position));

            return kingCaptures;
        }

        private static IEnumerable<MoveWrapper> CastlingMoves(Position position)
        {
            State state = position.State;
            Board board = position.Board;
            List<MoveWrapper> castlingMoves = new();

            if (state.Turn == Color.White)
            {
                if (state.CastlingRights.HasFlag(CastlingRights.WhiteKingSide) && (board.AllPieces & 0x60UL) == 0)
                    castlingMoves.Add(MoveWrapper.CreateMove(position, PieceType.King, Square.E1, Square.G1, MoveType.Castling));

                if (state.CastlingRights.HasFlag(CastlingRights.WhiteQueenSide) && (board.AllPieces & 0xEUL) == 0)
                    castlingMoves.Add(MoveWrapper.CreateMove(position, PieceType.King, Square.E1, Square.C1, MoveType.Castling));
            }
            else
            {
                if (state.CastlingRights.HasFlag(CastlingRights.BlackKingSide) && (board.AllPieces & 0x6000000000000000UL) == 0)
                    castlingMoves.Add(MoveWrapper.CreateMove(position, PieceType.King, Square.E8, Square.G8, MoveType.Castling));

                if (state.CastlingRights.HasFlag(CastlingRights.BlackQueenSide) && (board.AllPieces & 0xE00000000000000UL) == 0)
                    castlingMoves.Add(MoveWrapper.CreateMove(position, PieceType.King, Square.E8, Square.C8, MoveType.Castling));
            }

            return castlingMoves;
        }
    }
}