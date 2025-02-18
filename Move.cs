namespace Chess
{
    public struct Move(Position start, Position end)
    {
        public Position Start { get; set; } = start;
        public Position End { get; set; } = end;

        public Move(string move) : this(new Position(move[..2]), new Position(move[2..4])) { }

        public readonly void Deconstruct(out Position start, out Position end)
        {
            start = Start;
            end = End;
        }

        public readonly override string ToString()
        {
            return Start.ToString() + End.ToString();
        }

        public void ApplyRotation(BoardRotation rotation)
        {
            Start = Position.ApplyRotation(Start, rotation);
            End = Position.ApplyRotation(End, rotation);
        }
    }
}
