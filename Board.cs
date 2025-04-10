﻿/*
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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess
{
    /// <summary>
    /// Reprezentuje stan obrócenia szachownicy
    /// </summary>
    public enum BoardRotation
    {
        WhiteBottom,
        BlackBottom
    }

    /// <summary>
    /// Reprezentuje część wizualną gry (szachownicę)
    /// </summary>
    public class ChessBoard
    {
        // grafiki figur
        static readonly Dictionary<string, BitmapImage> pieceBitmaps = new()
        {
            { "w_p", new BitmapImage(new Uri("images/pieces/w_p.png", UriKind.Relative)) },
            { "w_k", new BitmapImage(new Uri("images/pieces/w_k.png", UriKind.Relative)) },
            { "w_q", new BitmapImage(new Uri("images/pieces/w_q.png", UriKind.Relative)) },
            { "w_n", new BitmapImage(new Uri("images/pieces/w_n.png", UriKind.Relative)) },
            { "w_b", new BitmapImage(new Uri("images/pieces/w_b.png", UriKind.Relative)) },
            { "w_r", new BitmapImage(new Uri("images/pieces/w_r.png", UriKind.Relative)) },
            { "b_p", new BitmapImage(new Uri("images/pieces/b_p.png", UriKind.Relative)) },
            { "b_k", new BitmapImage(new Uri("images/pieces/b_k.png", UriKind.Relative)) },
            { "b_q", new BitmapImage(new Uri("images/pieces/b_q.png", UriKind.Relative)) },
            { "b_n", new BitmapImage(new Uri("images/pieces/b_n.png", UriKind.Relative)) },
            { "b_b", new BitmapImage(new Uri("images/pieces/b_b.png", UriKind.Relative)) },
            { "b_r", new BitmapImage(new Uri("images/pieces/b_r.png", UriKind.Relative)) }
        };
        // grafika szachownicy
        static readonly BitmapImage boardBitmap = new(new Uri("images/board.png", UriKind.Relative));
        static readonly SolidColorBrush highlightColor = new(Colors.LightCoral);

        public delegate void OnUpdate();

        // przechowuje graficzną reprezentacje figur na szachownicy
        readonly Image[,] pieceImages = new Image[8, 8];
        // okno, na którym rejestrowane są wydarzenia
        readonly Window contextWindow;
        readonly Canvas drawCanvas;
        readonly Image boardImage;
        readonly MoveHighlight moveHighlight;
        readonly SquareHighlight selectedHighlight;

        int TileSize => BoardSize / 8;
        Position? selectedPieceStartPos = null;
        Image selectedPiece = null;
        bool pieceClicked = false;
        bool pieceDragged = false;

        // wizualna wielkość szachownicy
        public int BoardSize { get; private set; }
        public BoardRotation Rotation { get; set; } = BoardRotation.WhiteBottom;
        public bool Interactable { get; set; } = true;
        // menedżer gry zarządzający tą szachownicą
        public GameManager GameManager { get; set; }

        public OnUpdate PiecesUpdateHandler { get; set; }

        /// <summary>
        /// Konstruktor główny
        /// </summary>
        /// <param name="contextWindow">okno, do którego dołączane są zdarzenia</param>
        /// <param name="drawCanvas">Kanwa, na której rysować</param>
        /// <param name="boardSize">wielkość szachownicy - <b>powinna być podzielna przez 8</b>,
        /// w przeciwnym razie figury będą sie rozjeżdżać</param>
        public ChessBoard(Window contextWindow, Canvas drawCanvas, int boardSize)
        {
            BoardSize = boardSize;
            this.drawCanvas = drawCanvas;
            this.contextWindow = contextWindow;

            contextWindow.MouseLeftButtonDown += BoardMouseDown;
            contextWindow.MouseMove += BoardMouseMove;
            contextWindow.MouseLeftButtonUp += BoardMouseUp;

            moveHighlight = new MoveHighlight(highlightColor, TileSize);
            selectedHighlight = new SquareHighlight(highlightColor, TileSize);

            // utworzenie i pokazanie obrazka szachownicy
            boardImage = new Image
            {
                Source = boardBitmap,
                Width = boardSize,
                Height = boardSize
            };
            Canvas.SetZIndex(boardImage, -10);
            drawCanvas.Children.Add(boardImage);
        }

        /// <summary>
        /// Ustawia pozycję grafiki figury
        /// </summary>
        /// <param name="obj">grafika do nastawienia</param>
        /// <param name="offset">pozycja</param>
        private void SetImagePosition(Image obj, Position offset)
        {
            Canvas.SetTop(obj, offset.Y * TileSize);
            Canvas.SetLeft(obj, offset.X * TileSize);
        }

        /// <summary>
        /// Cofa pozycję zaznaczonej figry
        /// </summary>
        private void RevertSelectedPiecePosition()
        {
            if (selectedPiece == null) return;

            Canvas.SetTop(selectedPiece, selectedPieceStartPos.Value.Y * TileSize);
            Canvas.SetLeft(selectedPiece, selectedPieceStartPos.Value.X * TileSize);
        }

        /// <summary>
        /// rozpoczyna przeciąganie figury
        /// </summary>
        private void StartDrag()
        {
            pieceDragged = true;
            contextWindow.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Kończy przeciąganie figury
        /// </summary>
        private void EndDrag()
        {
            pieceDragged = false;
            contextWindow.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Zaznacza figurę
        /// </summary>
        /// <param name="pos">pozycja figury</param>
        private void SelectPiece(Position pos)
        {
            if (pieceImages[pos.Y, pos.X] == null || selectedPiece == pieceImages[pos.Y, pos.X])
                return;

            // wybranie figury i aktywowanie podświetlenia

            selectedPiece = pieceImages[pos.Y, pos.X];

            selectedPieceStartPos = pos;
            Canvas.SetZIndex(selectedPiece, 10);

            selectedHighlight.SetPosition(pos);
            selectedHighlight.Show(drawCanvas);
        }

        /// <summary>
        /// Odznacza figure
        /// </summary>
        private void UnselectPiece()
        {
            if (selectedPiece == null) return;

            // odznaczenei figury i ukrycie podświetlenia

            Canvas.SetZIndex(selectedPiece, 0);
            selectedPiece = null;

            selectedHighlight.Hide(drawCanvas);
        }

        /*
         * wydarzenia...
         */

        private void BoardMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Interactable) return;

            // przekonwertowanie pozycji myszki na pozycję pola
            Point p = e.GetPosition(drawCanvas);
            Position pos = Position.From(p, TileSize);

            if (!pos.InBounds()) return;

            if (pieceClicked && selectedPieceStartPos != pos)
            {
                pieceClicked = false;

                // próba ruchu figurą (bez przeciągania)

                if (GameManager.TryMove(new Move(selectedPieceStartPos.Value, pos, Rotation)))
                {
                    UnselectPiece();
                }
                else
                {
                    // w przypadku niepowodzenia, odznaczenie figury
                    // i zaznaczenie nowej

                    RevertSelectedPiecePosition();
                    UnselectPiece();

                    if (pieceImages[pos.Y, pos.X] != null)
                    {
                        SelectPiece(pos);
                        StartDrag();
                    }
                }
            }
            else
            {
                // zaznaczenie nowej figury

                SelectPiece(pos);
                StartDrag();
            }
        }

        private void BoardMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!Interactable || selectedPiece == null) return;

            // przekonwertowanie pozycji myszki na pozycję pola
            Point p = e.GetPosition(drawCanvas);
            Position pos = Position.From(p, TileSize);

            if (pos == selectedPieceStartPos)
            {
                RevertSelectedPiecePosition();

                if (pieceClicked)
                {
                    // odznaczenie figury po jej
                    // ponownym kliknięciu

                    UnselectPiece();
                    pieceClicked = false;
                }
                else
                {
                    pieceClicked = true;
                }
            }
            else if (pieceDragged)
            {
                // próba ruchu figurą (z przeciąganiem)
                if (GameManager.TryMove(new Move(selectedPieceStartPos.Value, pos, Rotation)))
                {
                    // odznaczenie figury

                    UnselectPiece();
                    pieceClicked = false;
                }
                else
                {
                    // cofnięcie figury

                    RevertSelectedPiecePosition();
                    pieceClicked = true;
                }
            }

            EndDrag();
        }

        private void BoardMouseMove(object sender, MouseEventArgs e)
        {
            if (!Interactable || !pieceDragged || selectedPiece == null) return;

            // zabezpieczenie przed zwolnieniem lewego przycisku myszy
            // poza oknem aplikacji
            if (e.LeftButton == MouseButtonState.Released)
            {
                RevertSelectedPiecePosition();
                UnselectPiece();
                EndDrag();
                pieceClicked = false;

                return;
            }

            // przeciąganie wybranej figury po szachownicy

            Point p = e.GetPosition(drawCanvas);

            Canvas.SetLeft(selectedPiece, p.X - (TileSize / 2));
            Canvas.SetTop(selectedPiece, p.Y - (TileSize / 2));
        }

        /// <summary>
        /// Przemieszcza figurę (bez sprawdzania poprawności ruchu)
        /// </summary>
        /// <param name="move">ruch do wykonania</param>
        public void MovePiece(Move move)
        {
            // dostosowanie ruchu do obrócenia szachownicy
            if (Rotation == BoardRotation.BlackBottom)
                move.Rotate();

            (Position from, Position to) = move;

            // przeniesienie figury i usunięcie zbitej (jeśli istnieje)
            drawCanvas.Children.Remove(pieceImages[to.Y, to.X]);
            pieceImages[to.Y, to.X] = pieceImages[from.Y, from.X];
            pieceImages[from.Y, from.X] = null;

            // ustawienie podświetlenia ruchu
            moveHighlight.SetPosition(from, to);
            moveHighlight.Show(drawCanvas);

            SetImagePosition(pieceImages[to.Y, to.X], to);

            PiecesUpdateHandler?.Invoke();
        }

        /// <summary>
        /// Podmienia figurę
        /// </summary>
        /// <param name="pos">pozycja figury</param>
        /// <param name="piece">nowa figura</param>
        public void ReplacePiece(Position pos, Piece newPiece)
        {
            // dostosowanie ruchu do obrócenia szachownicy
            if (Rotation == BoardRotation.BlackBottom)
                pos.Rotate();

            // usunięcie starej figury...
            if (pieceImages[pos.Y, pos.X] != null)
            {
                drawCanvas.Children.Remove(pieceImages[pos.Y, pos.X]);
            }

            // i utworzenie oraz dodanie nowej
            pieceImages[pos.Y, pos.X] = new Image
            {
                Source = pieceBitmaps[newPiece.ToString()],
                Width = TileSize,
                Height = TileSize
            };
            SetImagePosition(pieceImages[pos.Y, pos.X], pos);
            drawCanvas.Children.Add(pieceImages[pos.Y, pos.X]);

            PiecesUpdateHandler?.Invoke();
        }

        /// <summary>
        /// Usuwa figurę
        /// </summary>
        /// <param name="pos">pozycja figury</param>
        public void RemovePiece(Position pos)
        {
            // dostosowanie ruchu do obrócenia szachownicy
            if (Rotation == BoardRotation.BlackBottom)
                pos.Rotate();

            if (pieceImages[pos.Y, pos.X] == null) return;

            // usunięcie figury
            drawCanvas.Children.Remove(pieceImages[pos.Y, pos.X]);
            pieceImages[pos.Y, pos.X] = null;

            PiecesUpdateHandler?.Invoke();
        }

        /// <summary>
        /// Inicjalizuje ułożenie figur
        /// </summary>
        /// <param name="board">figury do wczytania</param>
        public void InitPieces(Piece[,] board)
        {
            drawCanvas.Visibility = Visibility.Hidden;

            moveHighlight.Hide(drawCanvas);
            selectedHighlight.Hide(drawCanvas);
            drawCanvas.Children.Clear();

            drawCanvas.Children.Add(boardImage);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // dostosowanie koordynatów do stanu obrócenia szachownicy
                    int x = Position.AlignToRotation(i, Rotation);
                    int y = Position.AlignToRotation(j, Rotation);

                    if (board[y, x] == null)
                    {
                        pieceImages[j, i] = null;
                    }
                    else
                    {
                        // tworzenie i dodawanie odpowiednich figur

                        pieceImages[j, i] = new Image
                        {
                            Source = pieceBitmaps[board[y, x].ToString()],
                            Width = TileSize,
                            Height = TileSize
                        };

                        SetImagePosition(pieceImages[j, i], new Position(i, j));
                        drawCanvas.Children.Add(pieceImages[j, i]);
                    }
                }
            }

            drawCanvas.Visibility = Visibility.Visible;
            PiecesUpdateHandler?.Invoke();
        }

        /// <summary>
        /// Obraca szachownicę
        /// </summary>
        public void Rotate()
        {
            Rotation = Rotation == BoardRotation.WhiteBottom ? BoardRotation.BlackBottom : BoardRotation.WhiteBottom;

            bool showMoveHl = moveHighlight.IsVisible;
            bool showSelectionHl = selectedHighlight.IsVisible;

            InitPieces(GameManager.Pieces);

            moveHighlight.Rotate();
            if (showMoveHl)
                moveHighlight.Show(drawCanvas);

            selectedHighlight.Rotate();
            if (showSelectionHl)
                selectedHighlight.Show(drawCanvas);
        }

        /// <summary>
        /// Zmienia wielkość szachownicy
        /// </summary>
        /// <param name="newSize">nowa wielkość</param>
        public void Resize(int newSize)
        {
            drawCanvas.Visibility = Visibility.Hidden;

            BoardSize = newSize;

            boardImage.Width = BoardSize;
            boardImage.Height = BoardSize;

            moveHighlight.Resize(TileSize);
            selectedHighlight.Resize(TileSize);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pieceImages[j, i] == null)
                        continue;

                    pieceImages[j, i].Width = TileSize;
                    pieceImages[j, i].Height = TileSize;

                    SetImagePosition(pieceImages[j, i], new Position(i, j));
                }
            }

            drawCanvas.Visibility = Visibility.Visible;
        }
    }
}
