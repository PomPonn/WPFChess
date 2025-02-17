namespace Chess
{
    public struct Move(Position start, Position end)
    {
        public Position Start { get; set; } = start;
        public Position End { get; set; } = end;


        public readonly void Deconstruct(out Position start, out Position end)
        {
            start = Start;
            end = End;
        }

        public static Move FromString(string move)
        {
            return new Move
            {
                Start = Position.From(move[..2]),
                End = Position.From(move[2..^3])
            };
        }
    }
}
