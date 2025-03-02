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
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Chess
{
    public static class EngineAPICLient
    {
        static readonly HttpClient client = new();
        static readonly string APIEndpoint = "https://stockfish.online/api/s/v2.php";


        /// <summary>
        /// Reprezentuje odpowiedź API
        /// </summary>
        public struct APIResponse
        {
            public bool Success { get; set; }
            public string Error { get; set; }
            public string BestMove { get; set; }
            public string Continuation { get; set; }
            public float? Evaluation { get; set; }
            public int? Mate { get; set; }

            public readonly (Move bestMove, Move? ponder) ParseBestMove()
            {
                string[] parts = BestMove.Split(" ");
                return (new Move(parts[1]), parts.Length == 4 ? new Move(parts[3]) : null);
            }
        }

        /// <summary>
        /// Asynchronicznie prosi API o podanie ruchu, wysyłając przy tym pozycję fen i glębokość silnika
        /// </summary>
        /// <param name="fen">pozycja FEN</param>
        /// <param name="depth">głębokość liczenia silnika</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">API odrzuciło dane</exception>
        public static async Task<APIResponse> Request(string fen, int depth = 12)
        {
            // asynchroniczne pobranie odpowiedzi API,
            // podającac pozycję FEN i głębokość liczenia
            // jako parametry URL
            using HttpResponseMessage response = await client.GetAsync(
                Path.Combine(APIEndpoint, $"?fen={fen}&depth={depth}")
            );
            response.EnsureSuccessStatusCode();

            // przekonwertowanie ciała odpowiedzi na JSON
            var apiAnswer = await response.Content.ReadFromJsonAsync<APIResponse>();

            if (!apiAnswer.Success)
                throw new ArgumentException($"Invalid chess engine api request - {apiAnswer.Error}");

            return apiAnswer;
        }
    }
}
