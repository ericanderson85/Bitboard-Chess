using Core;
using Types;

Position p = new();
Random r = new();
Console.WriteLine(p.CurrentBoard);
for (int i = 0; i < 50; i++)
{
    List<(Move, PieceType)> allMoves = new();
    foreach (Move pawnMove in p.PawnMoves()) allMoves.Add((pawnMove, PieceType.Pawn));
    foreach (Move knightMove in p.KnightMoves()) allMoves.Add((knightMove, PieceType.Knight));
    foreach (Move bishopMove in p.BishopMoves()) allMoves.Add((bishopMove, PieceType.Bishop));
    foreach (Move rookMove in p.RookMoves()) allMoves.Add((rookMove, PieceType.Rook));
    foreach (Move queenMove in p.QueenMoves()) allMoves.Add((queenMove, PieceType.Queen));
    foreach (Move kingMove in p.KingMoves()) allMoves.Add((kingMove, PieceType.King));

    int index = r.Next(allMoves.Count);
    Move move = allMoves[index].Item1;
    PieceType pieceType = allMoves[index].Item2;
    p.PerformMove(move, pieceType);
    Console.WriteLine(move);
    Console.WriteLine(p.CurrentBoard);
}

// using Core;
// using Types;

// int[] RBits = new int[]{
//   12, 11, 11, 11, 11, 11, 11, 12,
//   11, 10, 10, 10, 10, 10, 10, 11,
//   11, 10, 10, 10, 10, 10, 10, 11,
//   11, 10, 10, 10, 10, 10, 10, 11,
//   11, 10, 10, 10, 10, 10, 10, 11,
//   11, 10, 10, 10, 10, 10, 10, 11,
//   11, 10, 10, 10, 10, 10, 10, 11,
//   12, 11, 11, 11, 11, 11, 11, 12
// };

// int[] BBits = new int[]{
//   6, 5, 5, 5, 5, 5, 5, 6,
//   5, 5, 5, 5, 5, 5, 5, 5,
//   5, 5, 7, 7, 7, 7, 5, 5,
//   5, 5, 7, 9, 9, 7, 5, 5,
//   5, 5, 7, 9, 9, 7, 5, 5,
//   5, 5, 7, 7, 7, 7, 5, 5,
//   5, 5, 5, 5, 5, 5, 5, 5,
//   6, 5, 5, 5, 5, 5, 5, 6
// };

// List<MagicEntry> values = new();
// Random r = new();
// Square square = Square.A1;
// foreach (int bits in RBits)
// {
//     values.Add(Magic.FindRookMagic(square, bits, r));
//     square++;
// }

// Console.WriteLine();
// Console.WriteLine();
// Console.WriteLine();
// Console.WriteLine();
// Console.WriteLine();

// foreach (MagicEntry entry in values)
// {
//     Console.WriteLine(-(entry._indexShift - 64) + " " + entry._magic.ToString("X"));
// }