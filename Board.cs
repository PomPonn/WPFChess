using System;
using System.Collections.Generic;
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
        static readonly SolidColorBrush highlightColor = new SolidColorBrush(Colors.LightCoral);

        readonly Image[,] pieceImages = new Image[8, 8];
        readonly Window contextWindow;
        readonly Canvas drawCanvas;
        readonly Image boardImage;
        readonly MoveHighlight moveHighlight;
        readonly SquareHighlight selectedHighlight;

        Position selectedPieceStartPos = new Position();
        Image selectedPiece = null;
        bool pieceSelected = false;
        bool pieceDragged = false;

        public BoardRotation Rotation { get; set; } = BoardRotation.WhiteBottom;
        public bool Interactable { get; set; } = true;
        public GameManager GameManager { get; set; }
        int BoardSize { get; set; }

        int TileSize => BoardSize / 8;


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

            boardImage = new Image
            {
                Source = boardBitmap,
                Width = BoardSize,
                Height = BoardSize
            };
            Canvas.SetZIndex(boardImage, -10);
        }

        private void SetImagePosition(Image obj, Position offset)
        {
            Canvas.SetTop(obj, offset.Y * TileSize);
            Canvas.SetLeft(obj, offset.X * TileSize);
        }

        private void RevertSelectedPiecePosition()
        {
            if (selectedPiece == null) return;

            Canvas.SetTop(selectedPiece, selectedPieceStartPos.Y * TileSize);
            Canvas.SetLeft(selectedPiece, selectedPieceStartPos.X * TileSize);
        }

        private void StartDrag()
        {
            pieceDragged = true;
            contextWindow.Cursor = Cursors.Hand;
        }

        private void EndDrag()
        {
            pieceDragged = false;
            contextWindow.Cursor = Cursors.Arrow;
        }

        private void SelectPiece(Position pos)
        {
            if (selectedPiece == pieceImages[pos.Y, pos.X])
                return;

            selectedPiece = pieceImages[pos.Y, pos.X];

            if (selectedPiece == null) return;

            selectedPieceStartPos = pos;
            Canvas.SetZIndex(selectedPiece, 10);

            selectedHighlight.InitPosition(pos);
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
            Position pos = Position.From(p, TileSize);

            if (!pos.InBounds()) return;

            if (pieceSelected && selectedPieceStartPos != pos)
            {
                pieceSelected = false;
                if (GameManager.TryMove(new Move(selectedPieceStartPos, pos, Rotation)))
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
                        StartDrag();
                    }
                }
            }
            else
            {
                SelectPiece(pos);
                StartDrag();
            }
        }

        private void BoardMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!Interactable || selectedPiece == null) return;

            Point p = e.GetPosition(drawCanvas);
            Position pos = Position.From(p, TileSize);

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
                if (GameManager.TryMove(new Move(selectedPieceStartPos, pos, Rotation)))
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

            EndDrag();
        }

        private void BoardMouseMove(object sender, MouseEventArgs e)
        {
            if (!Interactable || !pieceDragged || selectedPiece == null) return;

            if (e.LeftButton == MouseButtonState.Released)
            {
                RevertSelectedPiecePosition();
                UnselectPiece();
                pieceSelected = false;
                EndDrag();

                return;
            }

            Point p = e.GetPosition(drawCanvas);

            Canvas.SetLeft(selectedPiece, p.X - (TileSize / 2));
            Canvas.SetTop(selectedPiece, p.Y - (TileSize / 2));
        }

        public bool MovePiece(Move move)
        {
            if (Rotation == BoardRotation.BlackBottom)
                move.Rotate();

            (Position from, Position to) = move;

            if (pieceImages[from.Y, from.X] == null)
            {
                return false;
            }

            drawCanvas.Children.Remove(pieceImages[to.Y, to.X]);
            pieceImages[to.Y, to.X] = pieceImages[from.Y, from.X];
            pieceImages[from.Y, from.X] = null;

            moveHighlight.InitPosition(from, to);
            moveHighlight.Show(drawCanvas);

            SetImagePosition(pieceImages[to.Y, to.X], to);

            return true;
        }

        public void ReplacePiece(Position pos, Piece piece)
        {
            if (Rotation == BoardRotation.BlackBottom)
                pos.Rotate();

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
            SetImagePosition(pieceImages[pos.Y, pos.X], pos);
            drawCanvas.Children.Add(pieceImages[pos.Y, pos.X]);
        }

        public void RemovePiece(Position pos)
        {
            if (Rotation == BoardRotation.BlackBottom)
                pos.Rotate();

            if (pieceImages[pos.Y, pos.X] == null) return;

            drawCanvas.Children.Remove(pieceImages[pos.Y, pos.X]);
            pieceImages[pos.Y, pos.X] = null;
        }

        public void InitPosition(Piece[,] board)
        {
            drawCanvas.Children.Clear();

            drawCanvas.Children.Add(boardImage);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int x = Position.AlignToRotation(i, Rotation);
                    int y = Position.AlignToRotation(j, Rotation);

                    if (board[y, x] == null)
                    {
                        pieceImages[j, i] = null;
                    }
                    else
                    {
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
        }
    }
}
