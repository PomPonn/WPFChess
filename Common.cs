using System;

namespace Chess
{
    public struct Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position(char file, int rank)
        {
            X = file - 'a';
            Y = 8 - rank;
        }

        public override string ToString()
        {
            return $"{(char)(X + 'a')}{8 - Y}";
        }
    }

    [Flags]
    public enum CastlingAbility
    {
        K = 0b1000,
        Q = 0b0100,
        k = 0b0010,
        q = 0b0001
    }
}
