using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess
{
    public static class FEN
    {
        public readonly struct Square
        {
            public char File { get; }
            public int Rank { get; }
            
            public Square(char file, int rank)
            {
                File = file;
                Rank = rank;
            }
         
            public override string ToString()
            {
                return $"{File}{Rank}";
            }
        }

        [Flags]
        public enum CastlingAbility
        {
            K =    0b1000,
            Q =    0b0100,
            k =    0b0010,
            q =    0b0001
        }

        public readonly struct CastlingBitField
        {
            private readonly int bitfield;

            public CastlingBitField(int bitfield)
            {
                this.bitfield = bitfield;
            }

            public bool HasFlag(CastlingAbility ability)
            {
                return (bitfield & (int)ability) != 0;
            }
        }

            public struct FENMetaInfo
            {
                public bool IsWhiteToMove { get; set; }
                public int CastlingRights { get; set; }
                public Square EnPassantTarget { get; set; }
                public int HalfMoveClock { get; set; }
                public int FullMoveCounter{ get; set; }
            } 

            public struct FENResult
            {
                public Piece[,] piecePlacement { get; set; }
                public FENMetaInfo Meta { get; set; }
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
                    table[rowNumber, i] = new Piece((PieceType)c, char.IsUpper(c));
                    i++;
                }
            }
        }

        public static FENResult Parse(string input)
        {
            FENResult result = new FENResult();

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

                result.piecePlacement = new Piece[8, 8];

                for (int j = 0; j < placement.Length; j++)
                {
                    ParseRowPlacement(result.piecePlacement, j, placement[j]);
                }

                result.Meta = new FENMetaInfo
                {
                    IsWhiteToMove = records[1] == "w",
                    CastlingRights = records[2] == "-" ? 0 : records[2].Aggregate(0, (acc, c) => acc | (int)Enum.Parse(typeof(CastlingAbility), c.ToString())),
                    EnPassantTarget = records[3] == "-" ? new Square('a', 0) : new Square(records[3][0], int.Parse(records[3][1].ToString())),
                    HalfMoveClock = int.Parse(records[4]),
                    FullMoveCounter = int.Parse(records[5])
                };
            }

            return result;
        }
    }
}
