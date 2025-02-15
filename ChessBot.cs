using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Chess.FEN;

namespace Chess
{
    public class ChessEngine
    {
        static readonly string APIEndpoint = "wss://chess-api.com/v1";
        static readonly int responseBufferSize = 1024;

        readonly ClientWebSocket client = new ClientWebSocket();
        readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        public struct SendOptions
        {
            public string FEN { get; set; }
            public int Variants { get; set; }
            public int Depth { get; set; }
            public int MaxThinkingTime { get; set; }
            public string SearchMoves { get; set; }

            public SendOptions(string fen)
            {
                FEN = fen;
                Variants = 1;
                Depth = 12;
                MaxThinkingTime = 50;
                SearchMoves = string.Empty;
            }
        }

        public struct Response
        {
            public string Type { get; set; }
            public int Eval { get; set; }
            public int Depth { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public string Promotion { get; set; }
            public bool IsCastling { get; set; }
            public string Move { get; set; }
            public string[] ContinuationArr { get; set; }
        }

        public Task Connect()
        {
            Uri uri = new Uri(APIEndpoint);

            return client.ConnectAsync(uri, tokenSource.Token);
        }

        public Task SendPosition(SendOptions options)
        {
            if (client.State == WebSocketState.Open)
            {
                MemoryStream stream = new MemoryStream();

                JsonSerializerOptions serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                JsonSerializer.Serialize(stream, options, serializerOptions);
                ArraySegment<byte> data = new ArraySegment<byte>(stream.ToArray());
                string responseJson = Encoding.UTF8.GetString(data.Array, 0, data.Count);

                return client.SendAsync(data, WebSocketMessageType.Text, false, tokenSource.Token);
            }

            return Task.FromResult(0);
        }

        public async void GetResponse()
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[responseBufferSize]);

            while (client.State == WebSocketState.Open)
            {
                var result = await client.ReceiveAsync(buffer, tokenSource.Token);

                string responseJson = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                Response response = JsonSerializer.Deserialize<Response>(responseJson, options);

                var test = 5;
            }
        }

        public Task CloseConnection(string status)
        {
            return client.CloseAsync(WebSocketCloseStatus.NormalClosure, status, tokenSource.Token);
        }
    }
}
