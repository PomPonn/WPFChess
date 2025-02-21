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
            X = file - 'a';
            Y = 8 - rank;
        }

        public Position(string position)
        {
            X = position[0] - 'a';
            Y = 8 - int.Parse(position[1].ToString());
        }

        public readonly void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public readonly bool InBounds()
        {
            return X >= 0 && X < 8 && Y >= 0 && Y < 8;
        }

        public void Rotate()
        {
            X = 7 - X;
            Y = 7 - Y;
        }

        public static Position Rotate(Position pos)
        {
            return new Position(7 - pos.X, 7 - pos.Y);
        }

        public void AlignToRotation(BoardRotation rotation)
        {
            if (rotation == BoardRotation.WhiteBottom) return;

            Rotate();
        }

        public static int AlignToRotation(int value, BoardRotation rotation)
        {
            return rotation == BoardRotation.WhiteBottom ? value : 7 - value;
        }

        public static Position AlignToRotation(Position pos, BoardRotation rotation)
        {
            if (rotation == BoardRotation.WhiteBottom) return pos;

            return Rotate(pos);
        }

        public static Position From(Point point, int tileSize)
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

        public override readonly bool Equals(object obj)
        {
            if (!(obj is Position))
                return false;

            return this == (Position)obj;
        }

        public override readonly int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override readonly string ToString()
        {
            return $"{(char)(X + 'a')}{8 - Y}";
        }
    }
}
