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
    /// Reprezentuje pojedynczy ruch na szachownicy
    /// </summary>
    /// <param name="start">początek</param>
    /// <param name="end">koniec</param>
    public struct Move(Position start, Position end)
    {
        public Position Start { get; set; } = start;
        public Position End { get; set; } = end;


        /// <summary>
        /// Konstruktor z dostosowaniem do stanu obrócenia szachownicy
        /// </summary>
        /// <param name="start">początek</param>
        /// <param name="end">koniec</param>
        /// <param name="rotation">stan obrócenia szachownicy</param>
        public Move(Position start, Position end, BoardRotation rotation) : this(start, end)
        {
            AlignToRotation(rotation);
        }

        /// <summary>
        /// Konstruktor z tekstowego zapisu ruchu
        /// </summary>
        /// <param name="move">np.: 'a2a4'</param>
        public Move(string move) : this(new Position(move[..2]), new Position(move[2..4])) { }

        /// <summary>
        /// Obraca początek i koniec ruchu
        /// </summary>
        public void Rotate()
        {
            Start = Position.Rotate(Start);
            End = Position.Rotate(End);
        }

        /// <summary>
        /// Dostosowuje początek i koniec ruchu do stanu obrócenia szachownicy
        /// </summary>
        /// <param name="rotation">stan obrócenia szachownicy</param>
        public void AlignToRotation(BoardRotation rotation)
        {
            Start = Position.AlignToRotation(Start, rotation);
            End = Position.AlignToRotation(End, rotation);
        }

        /// <summary>
        /// Dekonstuuje obiekt
        /// </summary>
        public readonly void Deconstruct(out Position start, out Position end)
        {
            start = Start;
            end = End;
        }

        public readonly override string ToString()
        {
            return Start.ToString() + End.ToString();
        }
    }
}
