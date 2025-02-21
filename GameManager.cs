using System;
using System.Media;
using System.Threading.Tasks;
using System.Windows;

namespace Chess
{
    public enum GameResult
    {
        WhiteWin,
        BlackWin,
        Draw
    }

    public enum GameType
    {
        Local,
        AgainstBot,
    }

    public class GameManager
    {
        static readonly SoundPlayer moveSound = new("audio/piece_move.wav");
        static readonly SoundPlayer takeSound = new("audio/piece_take.wav");
        static readonly SoundPlayer checkSound = new("audio/piece_check.wav");
        static readonly int MIN_MATERIAL = 5;


        FEN.Context gameContext;
        Position? lastOddBlackMovePos;
        Position? lastOddWhiteMovePos;
        GameType gameType;

        int repetitionCounter;
        int whiteMaterial = 0;
        int blackMaterial = 0;
        int engineDepth = 12;

        bool gameRunning;
        bool isClientWhiteSide = true;
        private bool CanClientMove
    => gameType != GameType.Local ? gameContext.IsWhiteToMove == isClientWhiteSide : true;

        public ChessBoard Board { get; set; }
        public Piece[,] Pieces = null;



        public GameManager(ChessBoard board, Piece[,] pieces = null)
        {
            Board = board;
            board.Interactable = false;
            board.GameManager = this;

            gameContext = new FEN.Context
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
            gameRunning = false;

            string text;

            if (isWhite == GameResult.Draw)
                text = "Draw! " + message;
            else
                text = "Checkmate! " + (isWhite == GameResult.WhiteWin ? "White" : "Black") + " wins! " + message;

            MessageBox.Show(text, "GameManager Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Start()
        {
            if (Pieces == null)
                throw new InvalidOperationException("Pieces not loaded.");
            if (gameRunning)
                throw new InvalidOperationException("Game is already running.");

            lastOddBlackMovePos = null;
            lastOddWhiteMovePos = null;

            repetitionCounter = 0;

            gameRunning = true;
            Board.Interactable = true;
        }

        public void StartLocalGame()
        {
            gameType = GameType.Local;

            Start();
        }

        public void StartGameAgainstBot(int botEngineDepth, bool isClientWhiteSide)
        {
            this.isClientWhiteSide = isClientWhiteSide;
            engineDepth = botEngineDepth;
            gameType = GameType.AgainstBot;

            Start();

            if (!isClientWhiteSide)
                _ = RequestBotMove();
        }

        public void LoadFENPosition(string fen)
        {
            var res = FEN.Parse(fen);

            Pieces = res.board;

            gameContext = res.context;

            Board.InitPosition(Pieces);
        }

        private bool AlliedKingCheck(Move move, Piece piece)
        {
            (Position start, Position end) = move;

            Piece temp = Pieces[end.Y, end.X];

            Pieces[end.Y, end.X] = piece;
            Pieces[start.Y, start.X] = null;

            // check if allied king is checked after the move
            bool isKingChecked = MoveValidator.KingChecked(Pieces, gameContext, piece.IsWhite);

            // revert move
            Pieces[end.Y, end.X] = temp;
            Pieces[start.Y, start.X] = piece;

            return isKingChecked;
        }

        private void HandleRepetitionCounter(Move move, Piece piece)
        {
            if (gameContext.FullMoveCounter % 2 == 0)
            {
                if ((piece.IsWhite && lastOddWhiteMovePos == move.End) || (!piece.IsWhite && lastOddBlackMovePos == move.End))
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
                    lastOddWhiteMovePos = move.Start;
                else
                    lastOddBlackMovePos = move.Start;
            }
        }

        private void UpdateGameState(Position moveStart, Position moveEnd)
        {
            Piece piece = Pieces[moveStart.Y, moveStart.X];

            gameContext.IsWhiteToMove = !gameContext.IsWhiteToMove;

            // increase move counters
            if (gameContext.IsWhiteToMove)
            {
                gameContext.FullMoveCounter++;
                gameContext.HalfMoveClock++;
            }

            // reset half move clock
            if (piece.Type == PieceType.Pawn || Pieces[moveEnd.Y, moveEnd.X] != null)
                gameContext.HalfMoveClock = 0;

            // refresh castling rights
            if (piece.Type == PieceType.King)
            {
                if (piece.IsWhite)
                {
                    gameContext.castlingRights.UnsetFlag(CastlingAbility.K);
                    gameContext.castlingRights.UnsetFlag(CastlingAbility.Q);
                }
                else
                {
                    gameContext.castlingRights.UnsetFlag(CastlingAbility.k);
                    gameContext.castlingRights.UnsetFlag(CastlingAbility.q);
                }
            }
            else if (piece.Type == PieceType.Rook)
            {
                if (piece.IsWhite && moveStart.Y == 7)
                {
                    if (moveStart.X == 0)
                        gameContext.castlingRights.UnsetFlag(CastlingAbility.Q);
                    else if (moveStart.X == 7)
                        gameContext.castlingRights.UnsetFlag(CastlingAbility.K);
                }
                else if (moveStart.Y == 0)
                {
                    if (moveStart.X == 0)
                        gameContext.castlingRights.UnsetFlag(CastlingAbility.q);
                    else if (moveStart.X == 7)
                        gameContext.castlingRights.UnsetFlag(CastlingAbility.k);
                }
            }

            // set en passant target
            if (piece.Type == PieceType.Pawn && Math.Abs(moveStart.Y - moveEnd.Y) == 2)
                gameContext.EnPassantTarget = new Position(moveEnd.X, (moveStart.Y + moveEnd.Y) / 2);
            else
                gameContext.EnPassantTarget = null;
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

        private async Task RequestBotMove()
        {
            EngineAPICLient.APIResponse response;

            try
            {
                response = await EngineAPICLient.Request(FEN.Build(Pieces, gameContext), engineDepth);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Nie udało się połączyć z botem:\n{e.Message}", "błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Move bestMove = response.ParseBestMove().bestMove;

            MoveValidator.CheckMove(Pieces, bestMove, gameContext, out bool specialMove);
            MakeMove(bestMove, specialMove);
        }

        private void MakeMove(Move move, bool specialMove)
        {
            (Position start, Position end) = move;

            Piece piece = Pieces[start.Y, start.X];

            // move piece on the board
            if (!Board.MovePiece(move))
            {
                // board and gameController are desynced
                // sync it back?
                throw new Exception("Piece move failed.");
            }

            UpdateGameState(start, end);

            // en passant capture
            if (piece.Type == PieceType.Pawn && specialMove)
            {
                int targetY = end.Y + (piece.IsWhite ? 1 : -1);

                Board.RemovePiece(new Position(end.X, targetY));
                Pieces[targetY, end.X] = null;
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
                    new Move(
                        new Position(rookX, rookY),
                        new Position(castledRookX, rookY)
                    )
                );
            }

            bool isTake = Pieces[end.Y, end.X] != null;

            Pieces[end.Y, end.X] = piece;
            Pieces[start.Y, start.X] = null;

            // pawn succesion
            if (piece.Type == PieceType.Pawn && (end.Y == 0 || end.Y == 7))
            {
                Pieces[end.Y, end.X] = new Piece(PieceType.Queen, piece.IsWhite);
                Board.ReplacePiece(end, Pieces[end.Y, end.X]);
            }

            // update repetition counter
            HandleRepetitionCounter(move, piece);

            bool enenyKingChecked = MoveValidator.KingChecked(Pieces, gameContext, !piece.IsWhite);

            if (enenyKingChecked)
                checkSound.Play();
            else if (isTake || specialMove)
                takeSound.Play();
            else
                moveSound.Play();

            CheckForGameEnd(piece);
        }

        private void CheckForGameEnd(Piece movedPiece)
        {
            CountMaterial();

            // win by mate
            if (MoveValidator.KingMated(Pieces, gameContext, !movedPiece.IsWhite))
            {
                GameOver(movedPiece.IsWhite ? GameResult.WhiteWin : GameResult.BlackWin);
            }
            // insufficient material
            else if (!CheckMaterial(movedPiece.IsWhite) && !CheckMaterial(!movedPiece.IsWhite))
            {
                GameOver(GameResult.Draw, "Insufficient Material");
            }
            // 3-fold repetition draw
            else if (repetitionCounter == 3)
            {
                GameOver(GameResult.Draw, "By repetition");
            }
            // 50 moves draw
            else if (gameContext.HalfMoveClock == 50)
            {
                GameOver(GameResult.Draw, "50 passive moves");
            }
            // Stalemate draw
            else if (MoveValidator.Stalemate(Pieces, gameContext, !movedPiece.IsWhite))
            {
                GameOver(GameResult.Draw, "Stalemate");
            }

        }

        public bool TryMove(Move move)
        {
            if (!CanClientMove) return false;

            Piece piece = Pieces[move.Start.Y, move.Start.X];

            if (piece == null || gameContext.IsWhiteToMove != piece.IsWhite)
                return false;

            if (!MoveValidator.CheckMove(Pieces, move, gameContext, out bool specialMove))
                return false;

            if (AlliedKingCheck(move, piece)) return false;

            MakeMove(move, specialMove);

            // ask bot to move
            if (gameType == GameType.AgainstBot)
            {
                _ = RequestBotMove();
            }

            return true;
        }
    }
}
