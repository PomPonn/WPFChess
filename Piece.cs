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
