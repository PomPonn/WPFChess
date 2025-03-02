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
using System.Linq;

namespace Chess
{
    /// <summary>
    /// pojedyncze zdolności do roszady jako flagi
    /// </summary>
    [Flags]
    public enum CastlingAbility
    {
        K = 0b1000,
        Q = 0b0100,
        k = 0b0010,
        q = 0b0001,
    }

    /// <summary>
    /// siatka bitów reprezentująca zdolności do roszady obu kolorów
    /// </summary>
    /// <param name="value">początkowa wartość</param>
    public struct CastlingBitField(int value)
    {
        public int Value { get; private set; } = value;

        public override readonly string ToString()
        {
            string res = "";

            // sprawdzenie obecności każdego prawa do roszady
            // i dodanie go jako string jeśli jest obecne

            foreach (string name in Enum.GetNames(typeof(CastlingAbility)))
            {
                CastlingAbility ability = (CastlingAbility)Enum.Parse(typeof(CastlingAbility), name);

                if (HasFlag(ability))
                    res = name + res;
            }

            return String.IsNullOrEmpty(res) ? "-" : res;
        }

        /// <summary>
        /// Sprawdza, czy flaga jest obecna
        /// </summary>
        /// <param name="flag">flaga do sprawdzenia</param>
        public readonly bool HasFlag(CastlingAbility flag)
        {
            return (Value & (int)flag) != 0;
        }

        /// <summary>
        /// Ustawia daną flagę
        /// </summary>
        /// <param name="flag">flaga do ustawienia</param>
        public void SetFlag(CastlingAbility flag)
        {
            Value |= (int)flag;
        }

        /// <summary>
        /// Deaktywuje daną flagę
        /// </summary>
        /// <param name="flag">flaga do dezaktywacji</param>
        public void UnsetFlag(CastlingAbility flag)
        {
            Value &= ~(int)flag;
        }
    }

    /// <summary>
    /// klasa do pracy z FEN (Forsyth–Edwards Notation - format do zapisu pozycji szachowych)
    /// </summary>
    public static class FEN
    {
        public struct Context
        {
            public bool IsWhiteToMove { get; set; }
            public CastlingBitField castlingRights;
            public Position? EnPassantTarget { get; set; }
            public int HalfMoveClock { get; set; }
            public int FullMoveCounter { get; set; }
        }

        public struct Result
        {
            public Piece[,] board;
            public Context context;
        }

        /// <summary>
        /// Parsuje pojedynczy wiersz
        /// </summary>
        /// <param name="table">tablica figur, do której wpisać wiersz</param>
        /// <param name="rowNumber">numer wiersza</param>
        /// <param name="placementStr">string do sparsowania</param>
        private static void ParseRowPlacement(Piece[,] table, int rowNumber, string placementStr)
        {
            int i = 0;

            foreach (char c in placementStr)
            {
                if (char.IsDigit(c))
                {
                    i += int.Parse(c.ToString());
                }
                else
                {
                    table[rowNumber, i] = new Piece((PieceType)char.ToLower(c), char.IsUpper(c));
                    i++;
                }
            }
        }

        /// <summary>
        /// Buduje nowy string FEN z podej tablicy figur i kontekstu
        /// </summary>
        /// <param name="table">dwuwymiarowa tablica figur reprezentująca szachownicę</param>
        /// <param name="context">kontekst gry</param>
        /// <returns>string FEN</returns>
        public static string Build(Piece[,] table, Context context)
        {
            // board

            string output = "";
            int skipCount = 0;

            void ApplySkip()
            {
                if (skipCount == 0) return;

                output += skipCount;
                skipCount = 0;
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Piece piece = table[i, j];

                    if (piece != null)
                    {
                        ApplySkip();
                        output += piece.IsWhite ? char.ToUpper((char)piece.Type) : (char)piece.Type;
                    }
                    else
                    {
                        skipCount++;
                    }
                }

                ApplySkip();

                if (i != 7)
                    output += '/';
            }
            output += ' ';

            //"rn1q1rk1/pp2b1pp/2p2n2/3p1pB1/3P4/1QP2N2/PP1N1PPP/R4RK1 b - - 1 11";
            //"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            // side to move
            output += context.IsWhiteToMove ? "w " : "b ";

            // castling rights
            output += context.castlingRights.ToString() + " ";

            // en passant target
            output += context.EnPassantTarget != null ? context.EnPassantTarget + " " : "- ";

            // halfmove clock
            output += context.HalfMoveClock + " ";

            // fullmove clock
            output += context.FullMoveCounter;

            return output;
        }

        /// <summary>
        /// parsuje string FEN
        /// </summary>
        /// <param name="input">string FEN do sparsowania</param>
        /// <returns>sparsowana pozycja FEN</returns>
        /// <exception cref="ArgumentException">nieprawidłowy string FEN</exception>
        public static Result Parse(string input)
        {
            Result result = new();

            for (int i = 0; i < input.Length; i++)
            {
                string[] records = input.Split(' ');

                if (records.Length != 6)
                {
                    throw new ArgumentException("Invalid FEN string.");
                }

                string[] placement = records[0].Split('/');

                if (placement.Length != 8)
                {
                    throw new ArgumentException("Invalid FEN string.");
                }

                result.board = new Piece[8, 8];

                for (int j = 0; j < placement.Length; j++)
                {
                    ParseRowPlacement(result.board, j, placement[j]);
                }

                result.context = new Context
                {
                    IsWhiteToMove = records[1] == "w",
                    castlingRights = new CastlingBitField(
                        records[2] == "-" ? 0 : records[2].Aggregate(0, (acc, c) => acc | (int)Enum.Parse(typeof(CastlingAbility), c.ToString()))
                    ),
                    EnPassantTarget = records[3] == "-" ? null : new Position(records[3][0], int.Parse(records[3][1].ToString())),
                    HalfMoveClock = int.Parse(records[4]),
                    FullMoveCounter = int.Parse(records[5])
                };
            }

            return result;
        }
    }
}
