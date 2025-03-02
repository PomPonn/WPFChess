/*
Zadanie zaliczeniowe z c#
Imię i nazwisko ucznia: Filip Gronkowski
Data wykonania zadania: 17.02.2025 - 04.03.2025
Treść zadania: 'Szachy'
Opis funkcjonalności aplikacji: 
    Aplikacja umożliwia grę w szachy z zachowaniem wszystkich zasad gry.
    Przed rozpoczęciem gry można ją skonfigurować. Dostępne parametry to:
        - tryb gry (gra lokalna i przeciwko AI),
        - pozycja startowa (w formacie FEN) oraz jej kopiowanie/wklejanie,
        - po wybraniu trybu 'przeciwko AI':
            * kolor gracza,
            * trudność AI od 4 do 16 (wyznaczająca głębokość liczenia silnika).
    Po rozpoczęciu gry pokazuje się szachownica (skalująca się wraz z rozmiarami okna),
    oraz przyciski, umożliwiające skopiowanie pozycji, obrócenie szachownicy i powrót do lobby.
*/


using System;
using System.Media;
using System.Threading.Tasks;
using System.Windows;

namespace Chess
{
    /// <summary>
    /// reprezentuje rezultat z gry
    /// </summary>
    public enum GameResult
    {
        WhiteWin,
        BlackWin,
        Draw,
        Interrupted
    }

    /// <summary>
    /// reprezentuje typ (tryb) gry
    /// </summary>
    public enum GameType
    {
        Local,
        AgainstBot,
    }

    /// <summary>
    /// Klasa zarządzająca warstwą logiczną gry
    /// </summary>
    public class GameManager
    {
        static readonly SoundPlayer moveSound = new("audio/piece_move.wav");
        static readonly SoundPlayer takeSound = new("audio/piece_take.wav");
        static readonly SoundPlayer checkSound = new("audio/piece_check.wav");
        static readonly int MIN_MATERIAL = 5;

        public delegate void onGameOver(GameResult result, string message = "");

        FEN.Context gameContext;
        Position? lastOddBlackMovePos;
        Position? lastOddWhiteMovePos;
        GameType gameType;

        int repetitionCounter;
        int engineDepth = 12;
        int whiteMaterial = 0;
        int blackMaterial = 0;

        bool isClientWhiteSide = true;
        private bool CanClientMove
            => gameType == GameType.Local || gameContext.IsWhiteToMove == isClientWhiteSide;

        public onGameOver? GameOverHandler { get; set; }
        public ChessBoard Board { get; set; }
        public Piece[,] Pieces = null;
        public bool GameRunning { get; private set; }
        public int MaterialDifference => whiteMaterial - blackMaterial;
        public string CurrentFEN => FEN.Build(Pieces, gameContext);
        public bool IsWhiteToMove => gameContext.IsWhiteToMove;


        /// <summary>
        /// Konstruktor główny
        /// </summary>
        /// <param name="board">wizuualna szachownica, z którą powiązać obiekt</param>
        /// <param name="pieces">tablica figur do wczytania</param>
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
                board.InitPieces(Pieces);
            }
        }

        /// <summary>
        /// Rozpoczęcie gry
        /// </summary>
        /// <exception cref="InvalidOperationException">nieprawidłowe rozpoczęcie gry</exception>
        private void Start()
        {
            if (Pieces == null)
                throw new InvalidOperationException("Pieces not loaded.");
            if (GameRunning)
                throw new InvalidOperationException("Game is already running.");

            lastOddBlackMovePos = null;
            lastOddWhiteMovePos = null;

            repetitionCounter = 0;

            GameRunning = true;
            Board.Interactable = true;

            CheckForGameEnd(!gameContext.IsWhiteToMove);
        }

        /// <summary>
        /// Zakończenie gry
        /// </summary>
        /// <param name="result">rezultat gry</param>
        /// <param name="message">opcjonalna wiadomość</param>
        private void GameOver(GameResult result, string message = null)
        {
            if (!GameRunning) return;

            Board.Interactable = false;
            GameRunning = false;

            GameOverHandler?.Invoke(result, message);
        }

        /// <summary>
        /// Forsowne zakończenie gry
        /// </summary>
        /// <param name="message">opcjonalna wiadomość</param>
        public void ForceGameOver(string message = null)
        {
            GameOver(GameResult.Interrupted, message);
        }

        /// <summary>
        /// Rozpoczęcie lokalnej gry
        /// </summary>
        public void StartLocalGame()
        {
            gameType = GameType.Local;

            Start();
        }

        /// <summary>
        /// Rozpoczęcie gry przeciwko AI
        /// </summary>
        /// <param name="botEngineDepth">głębokość liczenia silnika</param>
        /// <param name="isClientWhiteSide">czy klient (użytkownik) gra białymi</param>
        public void StartGameAgainstBot(int botEngineDepth, bool isClientWhiteSide)
        {
            this.isClientWhiteSide = isClientWhiteSide;
            engineDepth = botEngineDepth;
            gameType = GameType.AgainstBot;

            Start();

            // asynchroniczne pobranie ruchu z api
            if (!isClientWhiteSide)
                _ = RequestBotMove();
        }

        /// <summary>
        /// Wczytuje pozycję FEN
        /// </summary>
        /// <param name="fen">tekst przechowujący pozycję</param>
        public void LoadFEN(string fen)
        {
            var res = FEN.Parse(fen);

            Pieces = res.board;

            gameContext = res.context;

            Board.InitPieces(Pieces);
        }

        /// <summary>
        /// Sprawdza czy przyjazny król jest pod szachem po wykonaniu ruchu
        /// </summary>
        /// <param name="move">ruch do zasymulowania</param>
        /// <param name="piece">ruszająca figura</param>
        /// <returns></returns>
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

        /// <summary>
        /// zarządza licznikiem powtórzeń ruchów
        /// </summary>
        /// <param name="move">wykonany ruch</param>
        /// <param name="piece">ruszona figura</param>
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

        /// <summary>
        /// Aktualizuje stan gry
        /// </summary>
        /// <param name="moveStart">pozycja startowa ruchu</param>
        /// <param name="moveEnd">pozycja końcowa ruchu</param>
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

        /// <summary>
        /// Sprawdza, czy dla danego koloru istnieje jeszcze jakiś pionek
        /// </summary>
        /// <param name="isWhite">kolor do srawdzenia</param>
        /// <returns></returns>
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

        /// <summary>
        /// Podlicza materiał obu stron
        /// </summary>
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

        /// <summary>
        /// Sprawdza, czy nie ma za mało materiału, aby kontynuować grę
        /// </summary>
        /// <param name="isWhite"></param>
        private bool CheckMaterial(bool isWhite)
        {
            return (isWhite ? whiteMaterial : blackMaterial) >= MIN_MATERIAL || PawnExists(isWhite);
        }

        /// <summary>
        /// asynchronicznie pobiera ruch z API i go wykonuje
        /// </summary>
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

        /// <summary>
        /// Wykonuje ruch i odzwierciedla go wizualnej szachownicy
        /// </summary>
        /// <param name="move">ruch do wykonania</param>
        /// <param name="specialMove">czy ruch wymaga dodatkowych poruszeń figur</param>
        private void MakeMove(Move move, bool specialMove)
        {
            (Position start, Position end) = move;

            Piece piece = Pieces[start.Y, start.X];

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

            CountMaterial();

            // move piece on the board
            Board.MovePiece(move);

            // update repetition counter
            HandleRepetitionCounter(move, piece);

            bool enenyKingChecked = MoveValidator.KingChecked(Pieces, gameContext, !piece.IsWhite);

            if (enenyKingChecked)
                checkSound.Play();
            else if (isTake || specialMove)
                takeSound.Play();
            else
                moveSound.Play();

            CheckForGameEnd(piece.IsWhite);
        }

        /// <summary>
        /// Sprawdza, czy gra powinna być zakończona
        /// </summary>
        /// <param name="movedPieceColor">kolor ruszonej figury</param>
        private void CheckForGameEnd(bool movedPieceColor)
        {
            // win by mate
            if (MoveValidator.KingMated(Pieces, gameContext, !movedPieceColor))
            {
                GameOver(movedPieceColor ? GameResult.WhiteWin : GameResult.BlackWin);
            }
            // insufficient material
            else if (!CheckMaterial(movedPieceColor) && !CheckMaterial(!movedPieceColor))
            {
                GameOver(GameResult.Draw, "(Niewystarczający materiał)");
            }
            // 3-fold repetition draw
            else if (repetitionCounter == 3)
            {
                GameOver(GameResult.Draw, "(Powtórzenie pozycji)");
            }
            // 50 moves draw
            else if (gameContext.HalfMoveClock == 50)
            {
                GameOver(GameResult.Draw, "(50 pasywnych ruchów)");
            }
            // Stalemate draw
            else if (MoveValidator.Stalemate(Pieces, gameContext, !movedPieceColor))
            {
                GameOver(GameResult.Draw, "(Pat)");
            }

        }

        /// <summary>
        /// próbuje ruszyć figurą
        /// </summary>
        /// <param name="move">ruch do wykonania</param>
        /// <returns>czy ruch się powiódł</returns>
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
