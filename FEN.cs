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
        q = 0b0001,
    }

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
                    res += name;
            }

            return String.IsNullOrEmpty(res) ? "-" : res;
        }

        public readonly bool HasFlag(CastlingAbility flag)
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

    // klasa do pracy z FEN (Forsyth–Edwards Notation - format do zapisu pozycji szachowych)
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
