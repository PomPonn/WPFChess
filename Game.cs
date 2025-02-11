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
        private Position lastOddBlackMovePos;
        private Position lastOddWhiteMovePos;
        private int repetitionCounter;

        public Piece[,] Pieces = null;
        public GameState gameState;

        public ChessBoard Board { get; set; }


        private void UpdateGameState(Position moveStart, Position moveEnd)
        {
            Piece piece = Pieces[moveStart.Y, moveStart.X];

            gameState.FENContext.IsWhiteToMove = !gameState.FENContext.IsWhiteToMove;

            // increase move counters
            if (gameState.FENContext.IsWhiteToMove)
            {
                gameState.FENContext.FullMoveCounter++;
                gameState.FENContext.HalfMoveClock++;
            }

            // reset half move clock
            if (piece.Type == PieceType.Pawn || Pieces[moveEnd.Y, moveEnd.X] != null)
                gameState.FENContext.HalfMoveClock = 0;

            // refresh castling rights
            if (piece.Type == PieceType.King)
            {
                if (piece.IsWhite)
                {
                    gameState.FENContext.castlingRights.UnsetFlag(CastlingAbility.K);
                    gameState.FENContext.castlingRights.UnsetFlag(CastlingAbility.Q);
                }
                else
                {
                    gameState.FENContext.castlingRights.UnsetFlag(CastlingAbility.k);
                    gameState.FENContext.castlingRights.UnsetFlag(CastlingAbility.q);
                }
            }
            else if (piece.Type == PieceType.Rook)
            {
                if (piece.IsWhite && moveStart.Y == 7)
                {
                    if (moveStart.X == 0)
                        gameState.FENContext.castlingRights.UnsetFlag(CastlingAbility.Q);
                    else if (moveStart.X == 7)
                        gameState.FENContext.castlingRights.UnsetFlag(CastlingAbility.K);
                }
                else if (moveStart.Y == 0)
                {
                    if (moveStart.X == 0)
                        gameState.FENContext.castlingRights.UnsetFlag(CastlingAbility.q);
                    else if (moveStart.X == 7)
                        gameState.FENContext.castlingRights.UnsetFlag(CastlingAbility.k);
                }
            }

            // set en passant target
            if (piece.Type == PieceType.Pawn && Math.Abs(moveStart.Y - moveEnd.Y) == 2)
                gameState.FENContext.EnPassantTarget = new Position(moveEnd.X, (moveStart.Y + moveEnd.Y) / 2);
            else
                gameState.FENContext.EnPassantTarget = new Position(-1, -1);
        }

        private void GameOver(GameResult isWhite)
        {
            Board.interactable = false;

            string message;

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
                castlingRights = new CastlingBitField(0b1111),
                IsWhiteToMove = true,
                EnPassantTarget = new Position(-1, -1),
                HalfMoveClock = 0,
                FullMoveCounter = 1
            };

            if (pieces != null)
            {
                Pieces = pieces;
                board.SetPosition(Pieces);
            }
        }

        public void Start()
        {
            if (Pieces == null)
                throw new InvalidOperationException("Pieces not loaded.");

            lastOddBlackMovePos = new Position(-1, -1);
            lastOddWhiteMovePos = new Position(-1, -1);

            repetitionCounter = 0;

            Board.interactable = true;
        }

        public void LoadFENPosition(string fen)
        {
            var res = FEN.Parse(fen);

            Pieces = res.board;

            gameState.FENContext = res.context;

            Board.SetPosition(Pieces);
        }

        public bool TryMove(Position start, Position end)
        {
            Piece piece = Pieces[start.Y, start.X];

            if (piece == null || gameState.FENContext.IsWhiteToMove != piece.IsWhite)
                return false;

            if (!MoveValidator.CheckMove(Pieces, start, end, gameState, out bool specialMove))
                return false;

            Pieces[end.Y, end.X] = piece;
            Pieces[start.Y, start.X] = null;

            // check if allied king is checked after the move
            bool isKingChecked = MoveValidator.KingChecked(Pieces, gameState, piece.IsWhite);

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

            // en passant capture
            if (piece.Type == PieceType.Pawn && specialMove)
            {
                int epY = end.Y + (piece.IsWhite ? 1 : -1);

                Board.RemovePiece(new Position(end.X, epY));
                Pieces[epY, end.X] = null;
            }
            // castling
            else if (piece.Type == PieceType.King && specialMove)
            {
                int rookX = end.X == 6 ? 7 : 0;
                int rookY = piece.IsWhite ? 7 : 0;
                Piece rook = Pieces[rookY, rookX];

                int castledRookX = end.X - (end.X == 6 ? 1 : -1);
                Pieces[rookY, rookX] = null;
                Pieces[rookY, castledRookX] = rook;

                Board.MovePiece(new Position(rookX, rookY), new Position(castledRookX, rookY));
            }

            Pieces[end.Y, end.X] = piece;
            Pieces[start.Y, start.X] = null;

            // pawn succesion
            if (piece.Type == PieceType.Pawn && (end.Y == 0 || end.Y == 7))
            {
                Pieces[end.Y, end.X] = new Piece(PieceType.Queen, piece.IsWhite);
                Board.ReplacePiece(end, Pieces[end.Y, end.X]);
            }

            // update repetition counter
            if (gameState.FENContext.FullMoveCounter % 2 == 0)
            {
                if ((piece.IsWhite && lastOddWhiteMovePos == end) || (!piece.IsWhite && lastOddBlackMovePos == end))
                {
                    repetitionCounter++;
                }
                else
                {
                    repetitionCounter = 0;
                }
            }
            else
            {
                if (piece.IsWhite)
                    lastOddWhiteMovePos = start;
                else
                    lastOddBlackMovePos = start;
            }

            // 3-fold repetition draw
            if (repetitionCounter == 3)
            {
                GameOver(GameResult.Draw);
            }

            // 50 moves draw
            if (gameState.FENContext.HalfMoveClock == 50)
            {
                GameOver(GameResult.Draw);
            }

            // win by mate
            if (MoveValidator.KingChecked(Pieces, gameState, !piece.IsWhite) &&
                MoveValidator.KingMated(Pieces, gameState, !piece.IsWhite))
            {
                GameOver(piece.IsWhite ? GameResult.WhiteWin : GameResult.BlackWin);
            }

            return true;
        }
    }
}
