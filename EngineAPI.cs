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

        public static async Task<APIResponse> Request(string fen, int depth = 12)
        {
            using HttpResponseMessage response = await client.GetAsync(
                Path.Combine(APIEndpoint, $"?fen={fen}&depth={depth}")
            );
            response.EnsureSuccessStatusCode();

            var apiAnswer = await response.Content.ReadFromJsonAsync<APIResponse>();
            if (!apiAnswer.Success)
                throw new HttpRequestException($"Invalid chess engine api request - {apiAnswer.Error}");

            return apiAnswer;
        }
    }
}
