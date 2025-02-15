using System;

namespace Chess
{
    public static class MoveValidator
    {
        private static Position FindKing(Piece[,] pieces, bool isWhite)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pieces[j, i] != null && pieces[j, i].Type == PieceType.King && pieces[j, i].IsWhite == isWhite)
                    {
                        return new Position(i, j);
                    }
                }
            }

            return new Position(-1, -1);
        }

        private static bool CheckTilesForEachPiece(Piece[,] pieces, bool isWhite, Func<Position, Position, bool> checkPiece)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pieces[j, i] == null || pieces[j, i].IsWhite != isWhite) continue;

                    Position startPos = new Position(i, j);

                    for (int k = 0; k < 8; k++)
                    {
                        for (int l = 0; l < 8; l++)
                        {
                            Position testPos = new Position(k, l);

                            if (startPos == testPos) continue;

                            if (checkPiece(startPos, testPos)) return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool VerifyCastlingRights(Piece king, GameState gameState, Position start, Position end)
        {
            if (king.Type != PieceType.King)
                return false;

            if (king.IsWhite)
            {
                if (start.X == 4 && start.Y == 7)
                    if (end.X == 6)
                        return gameState.FENContext.castlingRights.HasFlag(CastlingAbility.K);
                    else if (end.X == 2)
                        return gameState.FENContext.castlingRights.HasFlag(CastlingAbility.Q);
            }
            else
            {
                if (start.X == 4 && start.Y == 0)
                    if (end.X == 6)
                        return gameState.FENContext.castlingRights.HasFlag(CastlingAbility.k);
                    else if (end.X == 2)
                        return gameState.FENContext.castlingRights.HasFlag(CastlingAbility.q);
            }

            return false;
        }

        public static bool CheckMove(Piece[,] pieces, Position from, Position to, GameState gameState, out bool specialMove)
        {
            specialMove = false;

            if (!from.InBounds() || !to.InBounds() ||
                (pieces[to.Y, to.X] != null && pieces[from.Y, from.X].IsWhite == pieces[to.Y, to.X].IsWhite))
                return false;

            switch (pieces[from.Y, from.X].Type)
            {
                case PieceType.Pawn:
                    return CheckPawnMove(pieces, from, to, gameState.FENContext.EnPassantTarget, out specialMove);
                case PieceType.Knight:
                    return CheckKnightMove(from, to);
                case PieceType.Bishop:
                    return CheckBishopMove(pieces, from, to);
                case PieceType.Rook:
                    return CheckRookMove(pieces, from, to);
                case PieceType.Queen:
                    return CheckQueenMove(pieces, from, to);
                case PieceType.King:
                    return CheckKingMove(pieces, gameState, from, to, out specialMove);
                default:
                    return false;
            }
        }

        public static bool CheckPawnMove(Piece[,] pieces, Position from, Position to, Position enPassantTarget, out bool enPassantCapture)
        {
            bool isWhite = pieces[from.Y, from.X].IsWhite;
            enPassantCapture = false;

            int direction = isWhite ? -1 : 1;
            int startRow = isWhite ? 6 : 1;
            int enPassantRow = isWhite ? 3 : 4;

            // Normal move
            if (from.X == to.X && pieces[to.Y, to.X] == null)
            {
                // Double move from starting position
                if (from.Y == startRow && to.Y == from.Y + (2 * direction) && pieces[from.Y + direction, from.X] == null)
                {
                    return true;
                }
                // Single move
                else if (to.Y == from.Y + direction)
                {
                    return true;
                }
            }
            // Capture move
            else if (Math.Abs(from.X - to.X) == 1 && to.Y == from.Y + direction)
            {
                // Normal capture
                if (pieces[to.Y, to.X] != null && pieces[to.Y, to.X].IsWhite != isWhite)
                {
                    return true;
                }
                // En passant capture
                else if (to == enPassantTarget && from.Y == enPassantRow)
                {
                    enPassantCapture = true;
                    return true;
                }
            }

            return false;
        }

        public static bool CheckKnightMove(Position from, Position to)
        {
            if (Math.Abs(from.X - to.X) == 2 && Math.Abs(from.Y - to.Y) == 1)
                return true;

            else if (Math.Abs(from.X - to.X) == 1 && Math.Abs(from.Y - to.Y) == 2)
                return true;

            return false;
        }

        public static bool CheckBishopMove(Piece[,] pieces, Position from, Position to)
        {
            if (Math.Abs(from.X - to.X) == Math.Abs(from.Y - to.Y))
            {
                int xDir = Math.Sign(to.X - from.X);
                int yDir = Math.Sign(to.Y - from.Y);
                for (int i = 1; i < Math.Abs(from.X - to.X); i++)
                {
                    if (pieces[from.Y + (i * yDir), from.X + (i * xDir)] != null)
                        return false;
                }
                return true;
            }

            return false;
        }

        public static bool CheckRookMove(Piece[,] pieces, Position from, Position to)
        {
            if (from.X == to.X)
            {
                int dir = Math.Sign(to.Y - from.Y);
                for (int i = 1; i < Math.Abs(from.Y - to.Y); i++)
                {
                    if (pieces[from.Y + (i * dir), from.X] != null)
                        return false;
                }

                return true;
            }
            else if (from.Y == to.Y)
            {
                int dir = Math.Sign(to.X - from.X);
                for (int i = 1; i < Math.Abs(from.X - to.X); i++)
                {
                    if (pieces[from.Y, from.X + (i * dir)] != null)
                        return false;
                }

                return true;
            }

            return false;
        }

        public static bool CheckQueenMove(Piece[,] pieces, Position from, Position to)
        {
            return CheckBishopMove(pieces, from, to) || CheckRookMove(pieces, from, to);
        }

        public static bool CheckKingMove(Piece[,] pieces, GameState gameState, Position from, Position to, out bool castled)
        {
            castled = false;

            // normal move
            if (Math.Abs(from.X - to.X) <= 1 && Math.Abs(from.Y - to.Y) <= 1)
            {
                return true;
            }
            // castling
            else if (Math.Abs(from.X - to.X) == 2 && from.Y == to.Y)
            {
                Piece king = pieces[from.Y, from.X];

                if (!VerifyCastlingRights(king, gameState, from, to))
                    return false;

                int dir = Math.Sign(to.X - from.X);
                Piece castleRook;

                if (dir == 1)
                    castleRook = pieces[from.Y, 7];
                else
                    castleRook = pieces[from.Y, 0];

                if (castleRook != null && castleRook.Type == PieceType.Rook && castleRook.IsWhite == king.IsWhite)
                {
                    for (int i = from.X + dir; i != to.X; i += dir)
                    {
                        if (pieces[from.Y, i] != null)
                            return false;

                        pieces[from.Y, i] = king;

                        bool isChecked = KingChecked(pieces, gameState, new Position(from.Y, i));

                        pieces[from.Y, i] = null;

                        if (isChecked)
                        {
                            return false;
                        }
                    }

                    castled = true;
                    return true;
                }
            }

            return false;
        }

        public static bool KingChecked(Piece[,] pieces, GameState gameState, Position kingPos)
        {
            Piece king = pieces[kingPos.Y, kingPos.X];

            if (!kingPos.InBounds() || king == null || king.Type != PieceType.King) return false;

            bool isWhite = pieces[kingPos.Y, kingPos.X].IsWhite;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pieces[j, i] != null && pieces[j, i].IsWhite != isWhite)
                    {
                        if (CheckMove(pieces, new Position(i, j), kingPos, gameState, out _))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool KingChecked(Piece[,] pieces, GameState gameState, bool isWhite)
        {
            Position kingPos = FindKing(pieces, isWhite);

            return KingChecked(pieces, gameState, kingPos);
        }

        public static bool Stalemate(Piece[,] pieces, GameState gameState, bool isWhite)
        {
            if (KingChecked(pieces, gameState, isWhite))
                return false;

            return !CheckTilesForEachPiece(pieces, isWhite, (startPos, testPos) =>
            {
                bool moveValid = CheckMove(pieces, startPos, testPos, gameState, out _);

                if (pieces[startPos.Y, startPos.X].Type == PieceType.King)
                {
                    if (moveValid)
                    {
                        (int sX, int sY) = startPos;
                        (int tX, int tY) = testPos;

                        Piece piece = pieces[tY, tX];

                        pieces[tY, tX] = pieces[sY, sX];
                        pieces[sY, sX] = null;

                        moveValid = !KingChecked(pieces, gameState, testPos);

                        pieces[sY, sX] = pieces[tY, tX];
                        pieces[tY, tX] = piece;

                    }

                    return moveValid;
                }

                return !moveValid;
            });
        }

        public static bool KingMated(Piece[,] pieces, GameState gameState, bool isWhite)
        {
            Position kingPos = FindKing(pieces, isWhite);

            if (!KingChecked(pieces, gameState, kingPos))
                return false;

            return !CheckTilesForEachPiece(pieces, isWhite, (startPos, testPos) =>
            {
                if (CheckMove(pieces, startPos, testPos, gameState, out _))
                {
                    (int sX, int sY) = startPos;
                    (int tX, int tY) = testPos;

                    Piece piece = pieces[tY, tX];

                    pieces[tY, tX] = pieces[sY, sX];
                    pieces[sY, sX] = null;

                    bool isChecked = startPos == kingPos ?
                        KingChecked(pieces, gameState, testPos) :
                        KingChecked(pieces, gameState, kingPos);

                    pieces[sY, sX] = pieces[tY, tX];
                    pieces[tY, tX] = piece;

                    return !isChecked;
                }

                return false;
            });
        }
    }
}
