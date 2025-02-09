using System;
using System.Linq;

namespace Chess
{
    public static class FEN
    {
        [Flags]
        public enum CastlingAbility
        {
            K = 0b1000,
            Q = 0b0100,
            k = 0b0010,
            q = 0b0001
        }

        public struct Result
        {
            public Piece[,] board;
            public GameInfo info;
        }

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

        public static Result Parse(string input)
        {
            Result result = new Result();

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

                result.info = new GameInfo
                {
                    IsWhiteToMove = records[1] == "w",
                    CastlingRights = records[2] == "-" ? 0 : records[2].Aggregate(0, (acc, c) => acc | (int)Enum.Parse(typeof(CastlingAbility), c.ToString())),
                    EnPassantTarget = records[3] == "-" ? new Position(0, 0)
                    : new Position(int.Parse(records[3][0].ToString()) - 'a', int.Parse(records[3][1].ToString())),
                    HalfMoveClock = int.Parse(records[4]),
                    FullMoveCounter = int.Parse(records[5])
                };
            }

            return result;
        }
    }
}
