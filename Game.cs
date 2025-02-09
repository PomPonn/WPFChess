using System;

namespace Chess
{
    public struct GameState
    {
        public FEN.Context FENContext;
    }

    public class Game
    {
        public Piece[,] Pieces = null;
        public GameState gameState;

        public ChessBoard Board { get; set; }


        private void UpdateGameState(Position move_start, Position move_end)
        {
            Piece piece = Pieces[move_start.Y, move_start.X];

            gameState.FENContext.IsWhiteToMove = !gameState.FENContext.IsWhiteToMove;
            gameState.FENContext.FullMoveCounter++;

            if (piece.Type == PieceType.King)
            {
                if (piece.IsWhite)
                {

                    gameState.FENContext.CastlingRights.UnsetFlag(CastlingAbility.K);
                    gameState.FENContext.CastlingRights.UnsetFlag(CastlingAbility.Q);
                }
                else
                {
                    gameState.FENContext.CastlingRights.UnsetFlag(CastlingAbility.k);
                    gameState.FENContext.CastlingRights.UnsetFlag(CastlingAbility.q);
                }
            }
            else if (piece.Type == PieceType.Rook)
            {
                if (piece.IsWhite)
                {
                    if (move_start.X == 0 && move_start.Y == 0)
                        gameState.FENContext.CastlingRights.UnsetFlag(CastlingAbility.Q);
                    else if (move_start.X == 7 && move_start.Y == 0)
                        gameState.FENContext.CastlingRights.UnsetFlag(CastlingAbility.K);
                }
                else
                {
                    if (move_start.X == 0 && move_start.Y == 7)
                        gameState.FENContext.CastlingRights.UnsetFlag(CastlingAbility.q);
                    else if (move_start.X == 7 && move_start.Y == 7)
                        gameState.FENContext.CastlingRights.UnsetFlag(CastlingAbility.k);
                }
            }

            if (piece.Type == PieceType.Pawn && Math.Abs(move_start.Y - move_end.Y) == 2)
                gameState.FENContext.EnPassantTarget = new Position(move_end.X, (move_start.Y + move_end.Y) / 2);
            else
                gameState.FENContext.EnPassantTarget = new Position(-1, -1);

            if (piece.Type == PieceType.Pawn || Pieces[move_end.Y, move_end.X] != null)
                gameState.FENContext.HalfMoveClock = 0;
            else
                gameState.FENContext.HalfMoveClock++;
        }

        public Game(ChessBoard board, Piece[,] pieces = null)
        {
            Board = board;
            board.interactable = false;
            board.GameManager = this;

            gameState.FENContext = new FEN.Context
            {
                CastlingRights = new CastlingBitField(0b1111),
                IsWhiteToMove = true,
                EnPassantTarget = new Position(-1, -1),
                HalfMoveClock = 0,
                FullMoveCounter = 1
            };

            if (pieces != null)
            {
                Pieces = pieces;
                board.SetPosition(ref Pieces);
            }
        }

        public void Start()
        {
            if (Pieces == null)
                throw new InvalidOperationException("Pieces not loaded.");

            Board.interactable = true;

        }

        public void LoadFENPosition(string fen)
        {
            var res = FEN.Parse(fen);

            Pieces = res.board;

            gameState.FENContext = res.context;

            Board.SetPosition(ref Pieces);
        }

        public bool TryMove(Position start, Position end)
        {
            Piece piece = Pieces[start.Y, start.X];
            bool enPassantCapture = false;

            if (piece == null || gameState.FENContext.IsWhiteToMove != piece.IsWhite || !start.InBounds() || !end.InBounds())
                return false;

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    if (MoveValidator.CheckPawnMove(ref Pieces, gameState.FENContext.EnPassantTarget, 
                        piece.IsWhite, start, end, out enPassantCapture))
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

            if (!Board.MovePiece(start, end))
            {
                return false;
            }

            UpdateGameState(start, end);

            if (enPassantCapture)
            {
                int epY = end.Y + (piece.IsWhite ? 1 : -1);

                Board.RemovePiece(new Position(end.X, epY));
                Pieces[epY, end.X] = null;
            }

            Pieces[end.Y, end.X] = piece;
            Pieces[start.Y, start.X] = null;

            return true;
        }
    }
}
