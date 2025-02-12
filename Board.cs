﻿using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess
{
    public enum BoardRotation
    {
        WhiteBottom,
        BlackBottom
    }

    public class ChessBoard
    {
        static readonly Dictionary<string, BitmapImage> pieceBitmaps = new Dictionary<string, BitmapImage>
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
        static readonly BitmapImage boardBitmap = new BitmapImage(new Uri("images/board.png", UriKind.Relative));
        static readonly SoundPlayer moveSound = new SoundPlayer("sounds/piece_move.wav");
        static readonly SolidColorBrush highlightColor = new SolidColorBrush(Colors.LightCoral);

        readonly Canvas drawCanvas;
        readonly Image boardImage;
        readonly Image[,] pieceImages = new Image[8, 8];
        readonly MoveHighlight moveHighlight;
        readonly SquareHighlight selectedHighlight;

        Image selectedPiece = null;
        Position selectedPieceStartPos = new Position();
        bool pieceSelected = false;
        bool pieceDragged = false;

        // in pixels
        int BoardSize { get; set; }
        int TileSize => BoardSize / 8;

        public BoardRotation Rotation { get; set; } = BoardRotation.WhiteBottom;
        public Game GameManager { get; set; }

        public bool Interactable { get; set; } = true;

        public ChessBoard(Window eventContextWindow, Canvas drawCanvas, int boardSize)
        {
            BoardSize = boardSize;
            this.drawCanvas = drawCanvas;

            eventContextWindow.MouseLeftButtonDown += BoardMouseDown;
            eventContextWindow.MouseMove += BoardMouseMove;
            eventContextWindow.MouseLeftButtonUp += BoardMouseUp;

            moveHighlight = new MoveHighlight(highlightColor, TileSize);
            selectedHighlight = new SquareHighlight(highlightColor, TileSize);

            boardImage = new Image
            {
                Source = boardBitmap,
                Width = BoardSize,
                Height = BoardSize
            };
            Canvas.SetZIndex(boardImage, -10);
        }

        private int ProjectToRotation(int x)
        {
            return Rotation == BoardRotation.WhiteBottom ? x : 7 - x;
        }

        private void SetImagePosition(Image obj, int row, int column)
        {
            Canvas.SetTop(obj, row * TileSize);
            Canvas.SetLeft(obj, column * TileSize);
        }

        private void RevertSelectedPiecePosition()
        {
            if (selectedPiece == null) return;

            Canvas.SetTop(selectedPiece, selectedPieceStartPos.Y * TileSize);
            Canvas.SetLeft(selectedPiece, selectedPieceStartPos.X * TileSize);
        }

        private void SelectPiece(Position pos)
        {
            if (selectedPiece == pieceImages[pos.Y, pos.X])
                return;

            selectedPiece = pieceImages[pos.Y, pos.X];

            if (selectedPiece == null) return;

            selectedPieceStartPos = pos;
            Canvas.SetZIndex(selectedPiece, 10);

            selectedHighlight.SetPosition(pos);
            selectedHighlight.Show(drawCanvas);
        }

        private void UnselectPiece()
        {
            if (selectedPiece == null) return;

            Canvas.SetZIndex(selectedPiece, 0);
            selectedPiece = null;

            selectedHighlight.Hide(drawCanvas);
        }

        private void BoardMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Interactable) return;

            Point p = e.GetPosition(drawCanvas);
            Position pos = Position.FromPoint(p, TileSize);

            if (!pos.InBounds()) return;

            if (pieceSelected && selectedPieceStartPos != pos)
            {
                pieceSelected = false;
                if (GameManager.TryMove(selectedPieceStartPos, pos, Rotation))
                {
                    UnselectPiece();
                }
                else
                {
                    RevertSelectedPiecePosition();
                    UnselectPiece();

                    if (pieceImages[pos.Y, pos.X] != null)
                    {
                        SelectPiece(pos);
                        pieceDragged = true;
                    }
                }
            }
            else
            {
                SelectPiece(pos);
                pieceDragged = true;
            }
        }

        private void BoardMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!Interactable || selectedPiece == null) return;

            Point p = e.GetPosition(drawCanvas);
            Position pos = Position.FromPoint(p, TileSize);

            if (pos == selectedPieceStartPos)
            {
                RevertSelectedPiecePosition();

                if (pieceSelected)
                {
                    UnselectPiece();
                    pieceSelected = false;
                }
                else
                {
                    pieceSelected = true;
                }
            }
            else if (pieceDragged)
            {
                if (GameManager.TryMove(selectedPieceStartPos, pos, Rotation))
                {
                    UnselectPiece();
                    pieceSelected = false;
                }
                else
                {
                    RevertSelectedPiecePosition();
                    pieceSelected = true;
                }
            }

            pieceDragged = false;
        }

        private void BoardMouseMove(object sender, MouseEventArgs e)
        {
            if (!Interactable || !pieceDragged || selectedPiece == null) return;

            if (e.LeftButton == MouseButtonState.Released)
            {
                RevertSelectedPiecePosition();
                UnselectPiece();
                pieceSelected = false;
                pieceDragged = false;

                return;
            }

            Point p = e.GetPosition(drawCanvas);

            Canvas.SetLeft(selectedPiece, p.X - (TileSize / 2));
            Canvas.SetTop(selectedPiece, p.Y - (TileSize / 2));
        }

        public bool MovePiece(Position from, Position to)
        {
            if (pieceImages[from.Y, from.X] == null)
            {
                return false;
            }

            drawCanvas.Children.Remove(pieceImages[to.Y, to.X]);
            pieceImages[to.Y, to.X] = pieceImages[from.Y, from.X];
            pieceImages[from.Y, from.X] = null;

            moveHighlight.SetPosition(from, to);
            moveHighlight.Show(drawCanvas);

            SetImagePosition(pieceImages[to.Y, to.X], to.Y, to.X);

            moveSound.Play();

            return true;
        }

        public void ReplacePiece(Position pos, Piece piece)
        {
            if (pieceImages[pos.Y, pos.X] != null)
            {
                drawCanvas.Children.Remove(pieceImages[pos.Y, pos.X]);
            }

            pieceImages[pos.Y, pos.X] = new Image
            {
                Source = pieceBitmaps[piece.ToString()],
                Width = TileSize,
                Height = TileSize
            };
            SetImagePosition(pieceImages[pos.Y, pos.X], pos.Y, pos.X);
            drawCanvas.Children.Add(pieceImages[pos.Y, pos.X]);
        }

        public void RemovePiece(Position pos)
        {
            if (pieceImages[pos.Y, pos.X] == null) return;

            drawCanvas.Children.Remove(pieceImages[pos.Y, pos.X]);
            pieceImages[pos.Y, pos.X] = null;
        }

        public void SetPosition(Piece[,] board)
        {
            drawCanvas.Children.Clear();

            drawCanvas.Children.Add(boardImage);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int x = ProjectToRotation(i);
                    int y = ProjectToRotation(j);

                    if (board[x, y] == null)
                    {
                        pieceImages[i, j] = null;
                    }
                    else
                    {
                        pieceImages[i, j] = new Image
                        {
                            Source = pieceBitmaps[board[x, y].ToString()],
                            Width = TileSize,
                            Height = TileSize
                        };

                        SetImagePosition(pieceImages[i, j], i, j);
                        drawCanvas.Children.Add(pieceImages[i, j]);
                    }
                }
            }
        }
    }
}
