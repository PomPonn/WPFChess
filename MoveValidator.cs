using System;

namespace Chess
{
    public static class MoveValidator
    {
        public static bool CheckPawnMove(ref Piece[,] pieces, Piece piece, Position from, Position to)
        {
            if (from.X == to.X && pieces[to.Y, to.X] == null)
            {
                if (from.Y == (piece.IsWhite ? 6 : 1) 
                    && from.Y - to.Y == (piece.IsWhite ? 2 : -2)
                    && pieces[to.Y + (piece.IsWhite ? 1 : -1), to.X] == null)
                {
                    goto __move;
                }
                else if (from.Y - to.Y == (piece.IsWhite ? 1 : -1))
                {
                    goto __move;
                }
            }
            else if (Math.Abs(from.X - to.X) == 1 && from.Y - to.Y == (piece.IsWhite ? 1 : -1) && pieces[to.Y, to.X] != null)
            {
                if (pieces[to.Y, to.X].IsWhite != piece.IsWhite)
                    goto __move;
            }

            return false;

        __move:

            return true;
        }
    }
}
