namespace Chess
{
    public struct GameInfo
    {
        public bool IsWhiteToMove { get; set; }
        public int CastlingRights { get; set; }
        public Position EnPassantTarget { get; set; }
        public int HalfMoveClock { get; set; }
        public int FullMoveCounter { get; set; }
    }

    public class Game
    {
        public ChessBoard Board { get; set; }
        public GameInfo GameState { get; set; }
        public Piece[,] Pieces;

        public Game(ChessBoard board)
        {
            Board = board;
            board.GameManager = this;
        }

        public void LoadFENPosition(string fen)
        {
            var res = FEN.Parse(fen);

            Pieces = res.board;
            GameState = res.info;

            Board.SetPosition(ref Pieces);
        }

        public bool TryMove(Position start, Position end)
        {
            Piece piece = Pieces[start.Y, start.X];

            if (!start.InBounds() || !end.InBounds() || piece == null)
                return false;

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    if (MoveValidator.CheckPawnMove(ref Pieces, piece.IsWhite, start, end))
                        goto __move;
                    break;
                case PieceType.Knight:
                    if (MoveValidator.CheckKnightMove(ref Pieces, start, end))
                        goto __move;
                    break;
                case PieceType.Bishop:
                    if (MoveValidator.CheckBishopMove(ref Pieces, start, end))
                        goto __move;
                    break;
                case PieceType.Rook:
                    if (MoveValidator.CheckRookMove(ref Pieces, start, end))
                        goto __move;
                    break;
                case PieceType.Queen:
                    if (MoveValidator.CheckQueenMove(ref Pieces, start, end))
                        goto __move;
                    break;
                case PieceType.King:
                    if (MoveValidator.CheckKingMove(ref Pieces, start, end))
                        goto __move;
                    break;
            }

            return false;

        __move:

            Pieces[end.Y, end.X] = piece;
            Pieces[start.Y, start.X] = null;

            Board.MovePiece(start, end);

            return true;
        }
    }
}
