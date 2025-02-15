using System;
using System.Linq;

namespace Chess
{
    [Flags]
    public enum CastlingAbility
    {
        K = 0b1000,
        Q = 0b0100,
        k = 0b0010,
        q = 0b0001
    }

    public struct CastlingBitField
    {
        public int Value { get; private set; }


        public CastlingBitField(int value)
        {
            Value = value;
        }

        public bool HasFlag(CastlingAbility flag)
        {
            return (Value & (int)flag) != 0;
        }

        public void SetFlag(CastlingAbility flag)
        {
            Value |= (int)flag;
        }

        public void UnsetFlag(CastlingAbility flag)
        {
            Value &= ~(int)flag;
        }
    }

    public static class FEN
    {
        public struct Context
        {
            public bool IsWhiteToMove { get; set; }
            public CastlingBitField castlingRights;
            public Position EnPassantTarget { get; set; }
            public int HalfMoveClock { get; set; }
            public int FullMoveCounter { get; set; }
        }

        public struct Result
        {
            public Piece[,] board;
            public Context context;
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

                result.context = new Context
                {
                    IsWhiteToMove = records[1] == "w",
                    castlingRights = new CastlingBitField(
                        records[2] == "-" ? 0 : records[2].Aggregate(0, (acc, c) => acc | (int)Enum.Parse(typeof(CastlingAbility), c.ToString()))
                    ),
                    EnPassantTarget = records[3] == "-" ? new Position(-1, -1) : new Position(records[3][0], int.Parse(records[3][1].ToString())),
                    HalfMoveClock = int.Parse(records[4]),
                    FullMoveCounter = int.Parse(records[5])
                };
            }

            return result;
        }
    }
}
