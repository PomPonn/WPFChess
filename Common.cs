using System;
using System.Windows;

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
            X = file - 'h';
            Y = 8 - rank;
        }

        public static Position FromPoint(Point point, int tileSize)
        {
            return new Position((int)Math.Floor(point.X / tileSize), (int)Math.Floor(point.Y / tileSize));
        }

        public static bool operator ==(Position a, Position b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Position a, Position b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Position))
                return false;

            Position other = (Position)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
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
