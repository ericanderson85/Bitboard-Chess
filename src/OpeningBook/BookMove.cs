namespace Book
{
    public readonly struct BookMove
    {
        public readonly string MoveString;
        public readonly uint Weight;

        public BookMove(string moveString, uint weight)
        {
            MoveString = moveString;
            Weight = weight;
        }
    }

}