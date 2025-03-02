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


namespace Chess
{
    /// <summary>
    /// Reprezentuje rodzaje figur 
    /// oraz ich znakowe odpowiedniki w angielskiej notacji algebraicznej
    /// </summary>
    public enum PieceType
    {
        Pawn = 'p',
        King = 'k',
        Queen = 'q',
        Knight = 'n',
        Bishop = 'b',
        Rook = 'r'
    }

    /// <summary>
    /// Klasa Reprezentująca Figurę
    /// </summary>
    /// <param name="type">typ figury</param>
    /// <param name="isWhite">czy figura jest biała</param>
    public class Piece(PieceType type, bool isWhite)
    {
        public PieceType Type { get; } = type;
        public bool IsWhite { get; } = isWhite;

        /// <summary>
        /// Zwraca wartość figury, w zależności od jej typu
        /// </summary>
        public int Value
        {
            get
            {
                return Type switch
                {
                    PieceType.Pawn => 1,
                    PieceType.Knight or PieceType.Bishop => 3,
                    PieceType.Rook => 5,
                    PieceType.Queen => 9,
                    _ => 0,
                };
            }
        }

        public override string ToString()
        {
            return $"{(IsWhite ? "w" : "b")}_{(char)Type}";
        }
    }
}
