namespace Chess
{
    public struct Move(Position start, Position end)
    {
        public Position Start { get; set; } = start;
        public Position End { get; set; } = end;


        public Move(Position start, Position end, BoardRotation rotation) : this(start, end)
        {
            AlignToRotation(rotation);
        }

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

        public void Rotate()
        {
            Start = Position.Rotate(Start);
            End = Position.Rotate(End);
        }

        public void AlignToRotation(BoardRotation rotation)
        {
            Start = Position.AlignToRotation(Start, rotation);
            End = Position.AlignToRotation(End, rotation);
        }
    }
}
