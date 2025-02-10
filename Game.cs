using System;
using System.Windows;

namespace Chess
{
    public struct GameState
    {
        public FEN.Context FENContext;
    }

    public enum GameResult
    {
        WhiteWin,
        BlackWin,
        Draw
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

        private Position FindKing(bool isWhite)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Pieces[i, j] != null && Pieces[i, j].Type == PieceType.King && Pieces[i, j].IsWhite == isWhite)
                    {
                        return new Position(j, i);
                    }
                }
            }

            return new Position(-1, -1);
        }

        private bool KingChecked(Position kingPos)
        {
            if (!kingPos.InBounds()) return false;

            bool isWhite = Pieces[kingPos.Y, kingPos.X].IsWhite;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Pieces[i, j] != null && Pieces[i, j].IsWhite != isWhite)
                    {
                        if (MoveValidator.CheckMove(ref Pieces, new Position(j, i), kingPos, gameState.FENContext.EnPassantTarget, out _))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool KingChecked(bool isWhite)
        {
            Position kingPos = FindKing(isWhite);

            return KingChecked(kingPos);
        }

        private bool KingMated(bool isWhite)
        {
            Position kingPos = FindKing(isWhite);

            if (!KingChecked(kingPos))
                return false;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Pieces[i, j] == null || Pieces[i, j].IsWhite != isWhite) continue;

                    Position startPos = new Position(j, i);

                    for (int k = 0; k < 8; k++)
                    {
                        for (int l = 0; l < 8; l++)
                        {
                            Position testPos = new Position(l, k);

                            if (MoveValidator.CheckMove(ref Pieces, startPos, testPos, gameState.FENContext.EnPassantTarget, out _))
                            {
                                Piece piece = Pieces[k, l];

                                Pieces[k, l] = Pieces[i, j];
                                Pieces[i, j] = null;

                                bool isChecked = startPos == kingPos ? KingChecked(testPos) : KingChecked(kingPos);

                                Pieces[i, j] = Pieces[k, l];
                                Pieces[k, l] = piece;

                                if (!isChecked) return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private void GameOver(GameResult isWhite)
        {
            Board.interactable = false;

            string message = string.Empty;

            if (isWhite == GameResult.Draw)
                message = "Draw!";
            else
                message = "Checkmate! " + (isWhite == GameResult.WhiteWin ? "White" : "Black") + " wins!";

            MessageBox.Show(message, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
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

            if (piece == null || gameState.FENContext.IsWhiteToMove != piece.IsWhite || !start.InBounds() || !end.InBounds())
                return false;

            if (!MoveValidator.CheckMove(ref Pieces, start, end, gameState.FENContext.EnPassantTarget, out bool enPassantCapture))
                return false;

            Pieces[end.Y, end.X] = piece;
            Pieces[start.Y, start.X] = null;

            // check if allied king is checked after the move
            bool isKingChecked = KingChecked(piece.IsWhite);

            // revert move, to update game state correctly
            Pieces[end.Y, end.X] = null;
            Pieces[start.Y, start.X] = piece;

            if (isKingChecked)
                return false;

            if (!Board.MovePiece(start, end))
            {
                // board and gameController are desynced
                // sync it back?
                throw new Exception("MovePiece failed.");
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

            if (KingChecked(!piece.IsWhite) && KingMated(!piece.IsWhite))
                GameOver(piece.IsWhite ? GameResult.WhiteWin : GameResult.BlackWin);

            return true;
        }
    }
}
