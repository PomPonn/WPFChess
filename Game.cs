using System;
using System.Media;
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
        static readonly SoundPlayer moveSound = new SoundPlayer("audio/piece_move.wav");
        static readonly SoundPlayer takeSound = new SoundPlayer("audio/piece_take.wav");
        static readonly SoundPlayer checkSound = new SoundPlayer("audio/piece_check.wav");
        static readonly int MIN_MATERIAL = 5;

        public Piece[,] Pieces = null;
		private Position lastOddBlackMovePos;
        private Position lastOddWhiteMovePos;
        public GameState gameState;
		private int whiteMaterial = 0;
		private int blackMaterial = 0;
		private int repetitionCounter;

        public ChessBoard Board { get; set; }


        public Game(ChessBoard board, Piece[,] pieces = null)
        {
            Board = board;
            board.Interactable = false;
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
                board.InitPosition(Pieces);
            }
        }

        private void GameOver(GameResult isWhite, string message = "")
        {
            Board.Interactable = false;

            string text;

            if (isWhite == GameResult.Draw)
                text = "Draw! " + message;
            else
                text = "Checkmate! " + (isWhite == GameResult.WhiteWin ? "White" : "Black") + " wins! " + message;

            MessageBox.Show(text, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Start()
        {
            if (Pieces == null)
                throw new InvalidOperationException("Pieces not loaded.");

            lastOddBlackMovePos = new Position(-1, -1);
            lastOddWhiteMovePos = new Position(-1, -1);

            repetitionCounter = 0;

            Board.Interactable = true;
        }

        public void LoadFENPosition(string fen)
        {
            var res = FEN.Parse(fen);

            Pieces = res.board;

            gameState.FENContext = res.context;

            Board.InitPosition(Pieces);
        }

        private bool AlliedKingCheck(Position start, Position end, Piece piece)
        {
            Piece temp = Pieces[end.Y, end.X];

            Pieces[end.Y, end.X] = piece;
            Pieces[start.Y, start.X] = null;

            // check if allied king is checked after the move
            bool isKingChecked = MoveValidator.KingChecked(Pieces, gameState, piece.IsWhite);

            // revert move
            Pieces[end.Y, end.X] = temp;
            Pieces[start.Y, start.X] = piece;

            return isKingChecked;
        }

        private void HandleRepetitionCounter(Position start, Position end, Piece piece)
        {
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
        }

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

        private bool PawnExists(bool isWhite)
        {
            foreach (Piece piece in Pieces)
            {
                if (piece == null) continue;
                if (piece.Type == PieceType.Pawn && piece.IsWhite == isWhite)
                    return true;
            }
            return false;
        }

        private void CountMaterial()
        {
            whiteMaterial = 0;
            blackMaterial = 0;
            foreach (Piece piece in Pieces)
            {
                if (piece == null) continue;
                if (piece.IsWhite)
                    whiteMaterial += piece.Value;
                else
                    blackMaterial += piece.Value;
            }
        }

        private bool CheckMaterial(bool isWhite)
        {
            return (isWhite ? whiteMaterial : blackMaterial) >= MIN_MATERIAL || PawnExists(isWhite);
        }

        public bool TryMove(Position start, Position end, BoardRotation rotation)
        {
            start.ApplyRotation(rotation);
            end.ApplyRotation(rotation);
            BoardRotation reversedRotation = BoardRotation.WhiteBottom == rotation ? BoardRotation.BlackBottom : BoardRotation.WhiteBottom;

            Piece piece = Pieces[start.Y, start.X];

            if (piece == null || gameState.FENContext.IsWhiteToMove != piece.IsWhite)
                return false;

            if (!MoveValidator.CheckMove(Pieces, start, end, gameState, out bool specialMove))
                return false;

            if (AlliedKingCheck(start, end, piece)) return false;

            Position originalStart = Position.ApplyRotation(start, rotation);
            Position originalEnd = Position.ApplyRotation(end, rotation);

            // move piece on the board
            if (!Board.MovePiece(originalStart, originalEnd))
            {
                // board and gameController are desynced
                // sync it back?
                throw new Exception("Piece move failed.");
            }

            UpdateGameState(start, end);

            // en passant capture
            if (piece.Type == PieceType.Pawn && specialMove)
            {
                int epY = end.Y + (piece.IsWhite ? 1 : -1);

                Board.RemovePiece(new Position(originalEnd.X, Position.ApplyRotation(epY, reversedRotation)));
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

                Board.MovePiece(
                    Position.ApplyRotation(rookX, rookY, rotation),
                    Position.ApplyRotation(castledRookX, rookY, rotation)
                );
            }

            bool isTake = Pieces[end.Y, end.X] != null;

            Pieces[end.Y, end.X] = piece;
            Pieces[start.Y, start.X] = null;

            // pawn succesion
            if (piece.Type == PieceType.Pawn && (end.Y == 0 || end.Y == 7))
            {
                Pieces[end.Y, end.X] = new Piece(PieceType.Queen, piece.IsWhite);
                Board.ReplacePiece(originalEnd, Pieces[end.Y, end.X]);
            }

            // update repetition counter
            HandleRepetitionCounter(start, end, piece);

            bool enenyKingChecked = MoveValidator.KingChecked(Pieces, gameState, !piece.IsWhite);

            if (enenyKingChecked)
                checkSound.Play();
            else if (isTake || specialMove)
                takeSound.Play();
            else
                moveSound.Play();

            CountMaterial();

            // win by mate
            if (MoveValidator.KingMated(Pieces, gameState, !piece.IsWhite))
            {
                GameOver(piece.IsWhite ? GameResult.WhiteWin : GameResult.BlackWin);
            }
            // insufficient material
            else if (!CheckMaterial(piece.IsWhite) && !CheckMaterial(!piece.IsWhite))
            {
                GameOver(GameResult.Draw, "Insufficient Material");
            }
            // 3-fold repetition draw
            else if (repetitionCounter == 3)
            {
                GameOver(GameResult.Draw, "By repetition");
            }
            // 50 moves draw
            else if (gameState.FENContext.HalfMoveClock == 50)
            {
                GameOver(GameResult.Draw, "50 passive moves");
            }
            // Stalemate draw
            else if (MoveValidator.Stalemate(Pieces, gameState, !piece.IsWhite))
            {
                GameOver(GameResult.Draw, "Stalemate");
            }

            return true;
        }
    }
}
