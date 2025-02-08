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

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    if (MoveValidator.CheckPawnMove(ref Pieces, piece, start, end)) goto __move;
                    //if (start.X == end.X && Pieces[end.Y, end.X] == null)
                    //{
                    //    if (start.Y == 6 && start.Y - end.Y == 2 && Pieces[end.Y + 1, end.X] == null)
                    //    {
                    //        goto __move;
                    //    }
                    //    else if (start.Y - end.Y == 1)
                    //    {
                    //        goto __move;
                    //    }
                    //}
                    //else if (Math.Abs(start.X - end.X) == 1 && start.Y - end.Y == 1 && Pieces[end.Y, end.X] != null)
                    //{
                    //    goto __move;
                    //}

                    //if (start.X == end.X)
                    //{
                    //    if (start.Y == 6 && end.Y - start.Y == 2)
                    //    {
                    //        goto __move;
                    //    }
                    //    else if (end.Y - start.Y == 1)
                    //    {
                    //        goto __move;
                    //    }
                    //}
                    //else if (Math.Abs(start.X - end.X) == 1 && end.Y - start.Y == 1)
                    //{
                    //    goto __move;
                    //}
                    break;
                case PieceType.Knight:
                    break;
                case PieceType.Bishop:
                    break;
                case PieceType.Rook:
                    break;
                case PieceType.Queen:
                    break;
                case PieceType.King:
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
