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

    public class Piece(PieceType type, bool isWhite)
    {
        public PieceType Type { get; } = type;
        public bool IsWhite { get; } = isWhite;
        public int Value
        {
            get
            {
                return Type switch
                {
                    PieceType.Pawn => 1,
                    PieceType.Knight or PieceType.Bishop => 3,
                    PieceType.Rook => 5,
                    PieceType.Queen => 9,
                    _ => 0,
                };
            }
        }

        public override string ToString()
        {
            return $"{(IsWhite ? "w" : "b")}_{(char)Type}";
        }
    }
}
