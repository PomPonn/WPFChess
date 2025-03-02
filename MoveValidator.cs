/*
Zadanie zaliczeniowe z c#
Imię i nazwisko ucznia: Filip Gronkowski
Data wykonania zadania: 17.02.2025 - 04.03.2025
Treść zadania: 'Szachy'
Opis funkcjonalności aplikacji: 
    Aplikacja umożliwia grę w szachy z zachowaniem wszystkich zasad gry.
    Przed rozpoczęciem gry można ją skonfigurować. Dostępne parametry to:
        - tryb gry (gra lokalna i przeciwko AI),
        - pozycja startowa (w formacie FEN) oraz jej kopiowanie/wklejanie,
        - po wybraniu trybu 'przeciwko AI':
            * kolor gracza,
            * trudność AI od 4 do 16 (wyznaczająca głębokość liczenia silnika).
    Po rozpoczęciu gry pokazuje się szachownica (skalująca się wraz z rozmiarami okna),
    oraz przyciski, umożliwiające skopiowanie pozycji, obrócenie szachownicy i powrót do lobby.
*/


using System;

namespace Chess
{
    /// <summary>
    /// Grupuje funkcje sprawdzajace poprawność ruchów i tym podobne
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>
        /// Odnajduje pozycję króla
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="isWhite">czy szukany król jest biały</param>
        /// <returns>pozycja króla lub nieprawidłowa pozycja, jeśli króla nie ma</returns>
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

        /// <summary>
        /// Dla każdego pola szachownicy wywołuje podaną funkcję,
        /// z pominięciem figur o podanym kolorze i pustych pól.
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="isWhite">kolor do zignorowania</param>
        /// <param name="checkPiece">funkcja zwrotna, wywoływana dla każdego pola</param>
        /// <returns><b>True</b>, jeżeli funkcja zwrotna zwróciła prawdę</returns>
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

        /// <summary>
        /// Sprawdza, czy roszada jest możliwa z obecym zestawem praw
        /// </summary>
        /// <param name="king">figura roszującego króla</param>
        /// <param name="gameContext">kontekst gry</param>
        /// <param name="move">ruch króla</param>
        /// <returns>Czy można zroszować</returns>
        private static bool VerifyCastlingRights(Piece king, FEN.Context gameContext, Move move)
        {
            if (king.Type != PieceType.King)
                return false;

            (Position start, Position end) = move;

            if (king.IsWhite)
            {
                if (start.X == 4 && start.Y == 7)
                    if (end.X == 6)
                        return gameContext.castlingRights.HasFlag(CastlingAbility.K);
                    else if (end.X == 2)
                        return gameContext.castlingRights.HasFlag(CastlingAbility.Q);
            }
            else
            {
                if (start.X == 4 && start.Y == 0)
                    if (end.X == 6)
                        return gameContext.castlingRights.HasFlag(CastlingAbility.k);
                    else if (end.X == 2)
                        return gameContext.castlingRights.HasFlag(CastlingAbility.q);
            }

            return false;
        }

        /// <summary>
        /// Sprawdza poprawność ruchu pionka
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="move">ruch do sprawdzenia</param>
        /// <param name="enPassantTarget">opcjonalna pozycja jako cel do zbicia po ruchu en passant przeciwnika</param>
        /// <param name="enPassantCapture">czy ruch jest en passant (w przelocie)</param>
        /// <returns>czy ruch jest poprawny</returns>
        private static bool CheckPawnMove(Piece[,] pieces, Move move, Position? enPassantTarget, out bool enPassantCapture)
        {
            (Position from, Position to) = move;

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

        /// <summary>
        /// Sprawdza poprawność ruchu skoczka
        /// </summary>
        /// <param name="move">ruch do sprawdzenia</param>
        /// <returns>czy ruch jest poprawny</returns>
        private static bool CheckKnightMove(Move move)
        {
            (Position from, Position to) = move;

            if (Math.Abs(from.X - to.X) == 2 && Math.Abs(from.Y - to.Y) == 1)
                return true;

            else if (Math.Abs(from.X - to.X) == 1 && Math.Abs(from.Y - to.Y) == 2)
                return true;

            return false;
        }

        /// <summary>
        /// Sprawdza poprawność ruchu gońca
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="move">ruch do sprawdzenia</param>
        /// <returns>czy ruch jest poprawny</returns>
        private static bool CheckBishopMove(Piece[,] pieces, Move move)
        {
            (Position from, Position to) = move;

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

        /// <summary>
        /// Sprawdza poprawność ruchu wieży
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="move">ruch do sprawdzenia</param>
        /// <returns>czy ruch jest poprawny</returns>
        private static bool CheckRookMove(Piece[,] pieces, Move move)
        {
            (Position from, Position to) = move;

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

        /// <summary>
        /// Sprawdza poprawność ruchu hetmana
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="move">ruch do sprawdzenia</param>
        /// <returns>czy ruch jest poprawny</returns>
        private static bool CheckQueenMove(Piece[,] pieces, Move move)
        {
            return CheckBishopMove(pieces, move) || CheckRookMove(pieces, move);
        }

        /// <summary>
        /// Sprawdza poprawność ruchu króla
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="gameContext">kontekst gry</param>
        /// <param name="move">ruch do sprawdzenia</param>
        /// <param name="castled">czy ruch wiąże sie z roszadą</param>
        /// <returns>czy ruch jest poprawny</returns>
        private static bool CheckKingMove(Piece[,] pieces, FEN.Context gameContext, Move move, out bool castled)
        {
            castled = false;
            (Position from, Position to) = move;

            // normal move
            if (Math.Abs(from.X - to.X) <= 1 && Math.Abs(from.Y - to.Y) <= 1)
            {
                return true;
            }
            // castling
            else if (Math.Abs(from.X - to.X) == 2 && from.Y == to.Y)
            {
                Piece king = pieces[from.Y, from.X];

                if (!VerifyCastlingRights(king, gameContext, move))
                    return false;

                int dir = Math.Sign(to.X - from.X);
                Piece castleRook;

                if (dir == 1)
                    castleRook = pieces[from.Y, 7];
                else
                    castleRook = pieces[from.Y, 0];

                if (castleRook != null && castleRook.Type == PieceType.Rook && castleRook.IsWhite == king.IsWhite)
                {
                    for (int x = from.X + dir; x != to.X; x += dir)
                    {
                        if (pieces[from.Y, x] != null)
                            return false;

                        pieces[from.Y, x] = king;

                        bool isChecked = KingChecked(pieces, gameContext, new Position(x, from.Y));

                        pieces[from.Y, x] = null;

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

        /// <summary>
        /// Sprawdza poprawność podanego ruchu
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="move">ruch do sprawdzenia</param>
        /// <param name="gameContext">kontekst gry</param>
        /// <param name="specialMove">informuje, czy podany ruch wiąże się z dodatkowymi ruchami</param>
        /// <returns>czy ruch jest poprawny</returns>
        public static bool CheckMove(Piece[,] pieces, Move move, FEN.Context gameContext, out bool specialMove)
        {
            specialMove = false;
            (Position from, Position to) = move;

            if (!from.InBounds() || !to.InBounds() ||
                (pieces[to.Y, to.X] != null && pieces[from.Y, from.X].IsWhite == pieces[to.Y, to.X].IsWhite))
                return false;

            switch (pieces[from.Y, from.X].Type)
            {
                case PieceType.Pawn:
                    return CheckPawnMove(pieces, move, gameContext.EnPassantTarget, out specialMove);
                case PieceType.Knight:
                    return CheckKnightMove(move);
                case PieceType.Bishop:
                    return CheckBishopMove(pieces, move);
                case PieceType.Rook:
                    return CheckRookMove(pieces, move);
                case PieceType.Queen:
                    return CheckQueenMove(pieces, move);
                case PieceType.King:
                    return CheckKingMove(pieces, gameContext, move, out specialMove);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Sprawdza, czy król jest szachowany
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="gameContext">kontekst gry</param>
        /// <param name="kingPos">pozycja króla</param>
        /// <returns>czy król jest szachowany</returns>
        public static bool KingChecked(Piece[,] pieces, FEN.Context gameContext, Position kingPos)
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
                        if (CheckMove(pieces, new Move(new Position(i, j), kingPos), gameContext, out _))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sprawdza, czy król jest szachowany
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="gameContext">kontekst gry</param>
        /// <param name="isWhite">kolor króla</param>
        /// <returns>czy król jest szachowany</returns>
        public static bool KingChecked(Piece[,] pieces, FEN.Context gameContext, bool isWhite)
        {
            Position kingPos = FindKing(pieces, isWhite);

            return KingChecked(pieces, gameContext, kingPos);
        }

        /// <summary>
        /// Sprawdza, czy wystąpił pat
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="gameContext">kontekst gry</param>
        /// <param name="isWhite">kolor do sprawdzenia</param>
        /// <returns>czy jest pat</returns>
        public static bool Stalemate(Piece[,] pieces, FEN.Context gameContext, bool isWhite)
        {
            if (KingChecked(pieces, gameContext, isWhite))
                return false;

            return !CheckTilesForEachPiece(pieces, isWhite, (startPos, testPos) =>
            {
                bool moveValid = CheckMove(pieces, new Move(startPos, testPos), gameContext, out _);

                if (pieces[startPos.Y, startPos.X].Type == PieceType.King)
                {
                    if (moveValid)
                    {
                        (int sX, int sY) = startPos;
                        (int tX, int tY) = testPos;

                        Piece piece = pieces[tY, tX];

                        pieces[tY, tX] = pieces[sY, sX];
                        pieces[sY, sX] = null;

                        moveValid = !KingChecked(pieces, gameContext, testPos);

                        pieces[sY, sX] = pieces[tY, tX];
                        pieces[tY, tX] = piece;

                    }

                    return moveValid;
                }

                return !moveValid;
            });
        }

        /// <summary>
        /// Sprawdza, czy wystąpił mat
        /// </summary>
        /// <param name="pieces">szachownica jako dwuwymiarowa tablica</param>
        /// <param name="gameContext">kontekst gry</param>
        /// <param name="isWhite">kolor do sprawdzenia</param>
        /// <returns>czy jest mat</returns>
        public static bool KingMated(Piece[,] pieces, FEN.Context gameContext, bool isWhite)
        {
            Position kingPos = FindKing(pieces, isWhite);

            if (!KingChecked(pieces, gameContext, kingPos))
                return false;

            return !CheckTilesForEachPiece(pieces, isWhite, (startPos, testPos) =>
            {
                if (CheckMove(pieces, new Move(startPos, testPos), gameContext, out _))
                {
                    (int sX, int sY) = startPos;
                    (int tX, int tY) = testPos;

                    Piece piece = pieces[tY, tX];

                    pieces[tY, tX] = pieces[sY, sX];
                    pieces[sY, sX] = null;

                    bool isChecked = startPos == kingPos ?
                        KingChecked(pieces, gameContext, testPos) :
                        KingChecked(pieces, gameContext, kingPos);

                    pieces[sY, sX] = pieces[tY, tX];
                    pieces[tY, tX] = piece;

                    return !isChecked;
                }

                return false;
            });
        }
    }
}
