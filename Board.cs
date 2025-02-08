using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess
{

    public class Board
    {
        Canvas drawCanvas;
        Image boardImage;
        PieceRepresentation[,] pieces;
        int BoardSize { get; set; }

        public Board(Canvas drawCanvas, string boardTexturePath, int boardSize)
        {
            BoardSize = boardSize;
            this.drawCanvas = drawCanvas;

            this.boardImage = new Image
            {
                Source = new BitmapImage(new Uri(boardTexturePath, UriKind.Relative)),
                Width = boardSize,
                Height = boardSize
            };

            Canvas.SetZIndex(boardImage, -1);
        }

        public void LoadFromFENString(string fen)
        {
            var res = FEN.Parse(fen);

            int tileSize = BoardSize / 8;

            pieces = new PieceRepresentation[8, 8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (res.piecePlacement[i, j] == null) continue;

                    pieces[i, j] = new PieceRepresentation(res.piecePlacement[i, j], tileSize);
                }
            }
        }

        public void WriteBoardCanvas()
        {
            drawCanvas.Children.Clear();

            drawCanvas.Children.Add(boardImage);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pieces[i, j] != null)
                    {
                        Canvas.SetTop(pieces[i, j].Visual, i * (BoardSize / 8));
                        Canvas.SetLeft(pieces[i, j].Visual, j * (BoardSize / 8));
                        drawCanvas.Children.Add(pieces[i, j].Visual);
                    }
                }
            }
        }
    }

}
