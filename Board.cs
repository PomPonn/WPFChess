using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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

        Image selectedPiece = null;
        Position selectedPieceStartPos = new Position();

        // in pixels
        int BoardSize { get; set; }
        int TileSize => BoardSize / 8;

        public Game GameManager { get; set; }

        private void BoardMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(sender as IInputElement);

            selectedPieceStartPos.X = (int)(p.X / TileSize);
            selectedPieceStartPos.Y = (int)(p.Y / TileSize);

            selectedPiece = pieceImages[selectedPieceStartPos.Y, selectedPieceStartPos.X];

            if (selectedPiece == null) return;

            Canvas.SetZIndex(selectedPiece, 10);
        }

        private void BoardMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedPiece == null) return;

            Point p = e.GetPosition(sender as IInputElement);

            Position newPos = new Position((int)(p.X / TileSize), (int)(p.Y / TileSize));

            if (!GameManager.TryMove(selectedPieceStartPos, newPos))
            {
                Canvas.SetTop(selectedPiece, selectedPieceStartPos.Y * TileSize);
                Canvas.SetLeft(selectedPiece, selectedPieceStartPos.X * TileSize);
            }

            Canvas.SetZIndex(selectedPiece, 0);
            selectedPiece = null;
        }

        private void BoardMouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPiece == null) return;

            if (e.LeftButton == MouseButtonState.Released)
            {
                BoardMouseUp(sender, null);
                return;
            }

            Point t = e.GetPosition(sender as IInputElement);

            Canvas.SetTop(selectedPiece, t.Y - selectedPiece.ActualHeight / 2);
            Canvas.SetLeft(selectedPiece, t.X - selectedPiece.ActualWidth / 2);
        }

        public ChessBoard(Canvas drawCanvas, int boardSize)
        {
            BoardSize = boardSize;
            this.drawCanvas = drawCanvas;

            drawCanvas.MouseDown += BoardMouseDown;
            drawCanvas.MouseMove += BoardMouseMove;
            drawCanvas.MouseUp += BoardMouseUp;

            pieceImages = new Image[8, 8];

            boardImage = new Image
            {
                Source = boardBitmap,
                Width = BoardSize,
                Height = BoardSize
            };

            Canvas.SetZIndex(boardImage, -1);
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
