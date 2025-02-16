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

        public struct APIRequestParams
        {
            public string Fen { get; set; }
            public int Depth { get; set; }
        }

        public struct APIResponse
        {
            public bool Success { get; set; }
            public string Data { get; set; }
            public string BestMove { get; set; }
            public int? Eval { get; set; }
            public int? Mate { get; set; }
            public string Continuation { get; set; }
        }

        public static async Task<APIResponse?> Request(string fen, int depth = 12)
        {
            try
            {
                //using HttpResponseMessage response = await client.GetAsync(
                //    Path.Combine(APIEndpoint, $"?fen={fen}&depth={depth}")
                //);
                using HttpResponseMessage response = await client.GetAsync("https://google.com");


                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<APIResponse>();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
