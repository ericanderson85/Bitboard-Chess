using Core;
using Types;
using File = Types.File;

Position p = new();
p.StartingPosition();

for (int i = 0; i < 64; i++)
{
    Bitboards.Print(AttackTables.KnightAttacks[i]);
}