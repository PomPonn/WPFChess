using System;

namespace Chess
{
    public static class MoveValidator
    {
        public static bool CheckPawnMove(ref Piece[,] pieces, bool isWhite, Position from, Position to)
        {
            if (from.X == to.X && pieces[to.Y, to.X] == null)
            {
                if (from.Y == (isWhite ? 6 : 1)
                    && from.Y - to.Y == (isWhite ? 2 : -2)
                    && pieces[to.Y + (isWhite ? 1 : -1), to.X] == null)
                {
                    return true;
                }
                else if (from.Y - to.Y == (isWhite ? 1 : -1))
                {
                    return true;
                }
            }
            else if (Math.Abs(from.X - to.X) == 1 && from.Y - to.Y == (isWhite ? 1 : -1) && pieces[to.Y, to.X] != null)
            {
                if (pieces[to.Y, to.X].IsWhite != isWhite)
                    return true;
            }

            return false;
        }

        public static bool CheckKnightMove(ref Piece[,] pieces, Position from, Position to)
        {
            if (pieces[to.Y, to.X] != null && pieces[from.Y, from.X].IsWhite == pieces[to.Y, to.X].IsWhite)
                return false;

            if (Math.Abs(from.X - to.X) == 2 && Math.Abs(from.Y - to.Y) == 1)
                return true;

            else if (Math.Abs(from.X - to.X) == 1 && Math.Abs(from.Y - to.Y) == 2)
                return true;

            return false;
        }

        public static bool CheckBishopMove(ref Piece[,] pieces, Position from, Position to)
        {
            if (pieces[to.Y, to.X] != null && pieces[from.Y, from.X].IsWhite == pieces[to.Y, to.X].IsWhite)
                return false;

            if (Math.Abs(from.X - to.X) == Math.Abs(from.Y - to.Y))
            {
                int xDir = Math.Sign(to.X - from.X);
                int yDir = Math.Sign(to.Y - from.Y);
                for (int i = 1; i < Math.Abs(from.X - to.X); i++)
                {
                    if (pieces[from.Y + i * yDir, from.X + i * xDir] != null)
                        return false;
                }
                return true;
            }

            return false;
        }

        public static bool CheckRookMove(ref Piece[,] pieces, Position from, Position to)
        {
            if (pieces[to.Y, to.X] != null && pieces[from.Y, from.X].IsWhite == pieces[to.Y, to.X].IsWhite)
                return false;

            if (from.X == to.X)
            {
                int dir = Math.Sign(to.Y - from.Y);
                for (int i = 1; i < Math.Abs(from.Y - to.Y); i++)
                {
                    if (pieces[from.Y + i * dir, from.X] != null)
                        return false;
                }

                return true;
            }
            else if (from.Y == to.Y)
            {
                int dir = Math.Sign(to.X - from.X);
                for (int i = 1; i < Math.Abs(from.X - to.X); i++)
                {
                    if (pieces[from.Y, from.X + i * dir] != null)
                        return false;
                }

                return true;
            }

            return false;
        }

        public static bool CheckQueenMove(ref Piece[,] pieces, Position from, Position to)
        {
            return CheckBishopMove(ref pieces, from, to) || CheckRookMove(ref pieces, from, to);
        }

        public static bool CheckKingMove(ref Piece[,] pieces, Position from, Position to)
        {
            if (pieces[to.Y, to.X] != null && pieces[from.Y, from.X].IsWhite == pieces[to.Y, to.X].IsWhite)
                return false;

            if (Math.Abs(from.X - to.X) <= 1 && Math.Abs(from.Y - to.Y) <= 1)
                return true;

            return false;
        }
    }
}
