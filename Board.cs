﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chess
{
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

        readonly Canvas drawCanvas;
        readonly Image boardImage;
        readonly Image[,] pieceImages;
        readonly Rectangle[] squareTints;

        Image selectedPiece = null;
        Position selectedPieceStartPos = new Position();
        bool pieceClicked = false;
        bool pieceDragged = false;

        // in pixels
        int BoardSize { get; set; }
        int TileSize => BoardSize / 8;

        public Game GameManager { get; set; }


        public ChessBoard(Window eventContextWindow, Canvas drawCanvas, int boardSize)
        {
            BoardSize = boardSize;
            this.drawCanvas = drawCanvas;

            eventContextWindow.MouseLeftButtonDown += BoardMouseDown;
            eventContextWindow.MouseMove += BoardMouseMove;
            eventContextWindow.MouseLeftButtonUp += BoardMouseUp;

            pieceImages = new Image[8, 8];

            squareTints = new Rectangle[3];
            for (int i = 0; i < squareTints.Length; i++)
            {
                squareTints[i] = new Rectangle
                {
                    Fill = System.Windows.Media.Brushes.Orange,
                    Opacity = 0.25,
                    Width = TileSize,
                    Height = TileSize
                };
                Canvas.SetZIndex(squareTints[i], -2);
            }

            boardImage = new Image
            {
                Source = boardBitmap,
                Width = BoardSize,
                Height = BoardSize
            };
            Canvas.SetZIndex(boardImage, -10);
        }

        private void RevertSelectedPiecePosition()
        {
            if (selectedPiece == null) return;

            Canvas.SetTop(selectedPiece, selectedPieceStartPos.Y * TileSize);
            Canvas.SetLeft(selectedPiece, selectedPieceStartPos.X * TileSize);
        }

        private bool CheckBoardBounds(Position pos)
        {
            return pos.X >= 0 && pos.X < 8 && pos.Y >= 0 && pos.Y < 8;
        }

        private void SelectPiece(Position pos)
        {
            if (selectedPiece == pieceImages[pos.Y, pos.X])
                return;

            selectedPiece = pieceImages[pos.Y, pos.X];

            if (selectedPiece == null) return;

            selectedPieceStartPos = pos;
            Canvas.SetZIndex(selectedPiece, 10);

            Canvas.SetTop(squareTints[0], pos.Y * TileSize);
            Canvas.SetLeft(squareTints[0], pos.X * TileSize);
            drawCanvas.Children.Add(squareTints[0]);
        }

        private void UnselectPiece()
        {
            if (selectedPiece == null) return;

            Canvas.SetZIndex(selectedPiece, 0);
            selectedPiece = null;

            drawCanvas.Children.Remove(squareTints[0]);
        }

        private void BoardMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(drawCanvas);
            Position pos = Position.FromPoint(p, TileSize);

            if (!CheckBoardBounds(pos)) return;

            if (pieceClicked && selectedPieceStartPos != pos)
            {
                if (GameManager.TryMove(selectedPieceStartPos, pos))
                {
                    UnselectPiece();
                    pieceClicked = false;
                }
                else
                {
                    RevertSelectedPiecePosition();
                    pieceClicked = false;
                }

                pieceDragged = false;
            }
            else
            {
                SelectPiece(pos);
                pieceDragged = true;
            }
        }

        private void BoardMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedPiece == null) return;

            Point p = e.GetPosition(drawCanvas);
            Position pos = Position.FromPoint(p, TileSize);

            if (pos == selectedPieceStartPos)
            {
                RevertSelectedPiecePosition();

                if (pieceClicked)
                {
                    UnselectPiece();
                    pieceClicked = false;
                }
                else
                {
                    pieceClicked = true;
                }
            }
            else
            {
                if (!GameManager.TryMove(selectedPieceStartPos, pos))
                {
                    RevertSelectedPiecePosition();
                }
                UnselectPiece();
                pieceClicked = false;

                if (!pieceDragged && CheckBoardBounds(pos) && pieceImages[pos.Y, pos.X] != null)
                {
                    SelectPiece(pos);
                    pieceClicked = true;
                }
            }

            pieceDragged = false;
        }

        private void BoardMouseMove(object sender, MouseEventArgs e)
        {
            if (!pieceDragged || selectedPiece == null) return;

            if (e.LeftButton == MouseButtonState.Released)
            {
                RevertSelectedPiecePosition();
                UnselectPiece();
                pieceClicked = false;
                pieceDragged = false;

                return;
            }

            Point p = e.GetPosition(drawCanvas);

            Canvas.SetLeft(selectedPiece, p.X - TileSize / 2);
            Canvas.SetTop(selectedPiece, p.Y - TileSize / 2);
        }

        private void SetObjectPosition(ref Image obj, int row, int column)
        {
            Canvas.SetTop(obj, row * TileSize);
            Canvas.SetLeft(obj, column * TileSize);
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

            SetObjectPosition(ref pieceImages[to.Y, to.X], to.Y, to.X);

            return true;
        }

        public void SetBoardTexture(string boardTexturePath)
        {
            boardImage.Source = new BitmapImage(new Uri(boardTexturePath, UriKind.Relative));
        }

        public void SetPosition(ref Piece[,] board)
        {
            drawCanvas.Children.Clear();

            drawCanvas.Children.Add(boardImage);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j] == null)
                    {
                        pieceImages[i, j] = null;
                    }
                    else
                    {
                        var test = board[i, j].ToString();
                        pieceImages[i, j] = new Image
                        {
                            Source = pieceBitmaps[test],
                            Width = TileSize,
                            Height = TileSize
                        };

                        SetObjectPosition(ref pieceImages[i, j], i, j);
                        drawCanvas.Children.Add(pieceImages[i, j]);
                    }
                }
            }
        }
    }
}
