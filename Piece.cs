namespace Chess
{
    public enum PieceType
    {
        Pawn = 'p',
        King = 'k',
        Queen = 'q',
        Knight = 'n',
        Bishop = 'b',
        Rook = 'r'
    }

    public class Piece
    {
        public PieceType Type { get; }
        public bool IsWhite { get; }
        public int Value
        {
            get
            {
                switch (Type)
                {
                    case PieceType.Pawn:
                        return 1;
                    case PieceType.Knight:
                    case PieceType.Bishop:
                        return 3;
                    case PieceType.Rook:
                        return 5;
                    case PieceType.Queen:
                        return 9;
                    case PieceType.King:
                    default:
                        return 0;
                }
            }
        }

        public Piece(PieceType type, bool isWhite)
        {
            Type = type;
            IsWhite = isWhite;
        }

        public override string ToString()
        {
            return $"{(IsWhite ? "w" : "b")}_{(char)Type}";
        }
    }
}
