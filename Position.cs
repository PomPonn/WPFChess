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

        public bool InBounds()
        {
            return X >= 0 && X < 8 && Y >= 0 && Y < 8;
        }

        public void ApplyRotation(BoardRotation rotation)
        {
            if (rotation == BoardRotation.WhiteBottom) return;

            X = 7 - X;
            Y = 7 - Y;
        }

        public static int ApplyRotation(int value, BoardRotation rotation)
        {
            return rotation == BoardRotation.WhiteBottom ? value : 7 - value;
        }

        public static Position ApplyRotation(Position pos, BoardRotation rotation)
        {
            if (rotation == BoardRotation.WhiteBottom) return pos;

            return new Position(7 - pos.X, 7 - pos.Y);
        }

        public static Position ApplyRotation(int x, int y, BoardRotation rotation)
        {
            if (rotation == BoardRotation.WhiteBottom) return new Position(x, y);

            return new Position(7 - x, 7 - y);
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
}
