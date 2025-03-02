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
using System.Windows;

namespace Chess
{
    /// <summary>
    /// Reprezentuje pozycję na szachownicy
    /// </summary>
    public struct Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Konstruktor główny
        /// </summary>
        /// <param name="x">pozycja na osi x (0 po lewej)</param>
        /// <param name="y">pozycja na osi y (0 u góry)</param>
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Konstruktor korzystający z pozycjiw formie tekstu'
        /// </summary>
        /// <param name="position">w formacie '[linia (od a do h)][wiersz (od 1 do 8)]'</param>
        public Position(string position)
        {
            X = position[0] - 'a';
            Y = 8 - int.Parse(position[1].ToString());
        }

        /// <summary>
        /// Dekonstruuje obiekt
        /// </summary>
        public readonly void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        /// <returns>Czy pozycja znajduje się w obrębie szachownicy (czy jest prawidłowa)</returns>
        public readonly bool InBounds()
        {
            return X >= 0 && X < 8 && Y >= 0 && Y < 8;
        }

        /// <summary>
        /// Obraca pozycję
        /// </summary>
        public void Rotate()
        {
            X = 7 - X;
            Y = 7 - Y;
        }

        /// <summary>
        /// Obraca pozycję
        /// </summary>
        /// <param name="pos">pozycja do obrócenia</param>
        /// <returns>obrócona pozycja</returns>
        public static Position Rotate(Position pos)
        {
            return new Position(7 - pos.X, 7 - pos.Y);
        }

        /// <summary>
        /// Dostosowuje komponent do stanu obrócenia szachownicy
        /// </summary>
        /// <param name="value">komponent do obrócenia</param>
        /// <param name="rotation">stan obrócenia szachownicy</param>
        /// <returns>obrócony komponent</returns>
        public static int AlignToRotation(int value, BoardRotation rotation)
        {
            return rotation == BoardRotation.WhiteBottom ? value : 7 - value;
        }

        /// <summary>
        /// Dostosowuje pozycję do stanu obrócenia szachownicy
        /// </summary>
        /// <param name="pos">pozycja do obrócenia</param>
        /// <param name="rotation">stan obrócenia szachownicy</param>
        /// <returns>obrócona pozycja</returns>
        public static Position AlignToRotation(Position pos, BoardRotation rotation)
        {
            if (rotation == BoardRotation.WhiteBottom) return pos;

            return Rotate(pos);
        }

        /// <summary>
        /// Konstruuje pozycję z punktu wskazanego przez myszkę,
        /// korzystając z wizualnej wielkości pojedynczego pola szachownicy
        /// </summary>
        /// <param name="point">punkt wskazany przez myszkę</param>
        /// <param name="tileSize">wizualna wielkość pojedynczego pola szachownicy</param>
        /// <returns>nowa skonstruowania pozycja</returns>
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
