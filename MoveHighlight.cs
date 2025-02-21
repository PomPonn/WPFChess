using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Chess
{
    public class MoveHighlight
    {
        private readonly SquareHighlight[] sqrs = new SquareHighlight[2];

        public bool IsVisible { get; private set; }

        public MoveHighlight(SolidColorBrush color, int tileSize)
        {
            IsVisible = false;

            for (int i = 0; i < 2; i++)
            {
                sqrs[i] = new SquareHighlight(color, tileSize);
            }
        }

        public void Show(Canvas canvas)
        {
            foreach (var sqr in sqrs)
            {
                sqr.Show(canvas);
            }

            IsVisible = true;
        }

        public void Hide(Canvas canvas)
        {
            foreach (var sqr in sqrs)
            {
                sqr.Hide(canvas);
            }

            IsVisible = false;
        }

        public void InitPieces(Position from, Position to)
        {
            sqrs[0].InitPieces(from);
            sqrs[1].InitPieces(to);
        }
    }

    public class SquareHighlight
    {
        static readonly float highlightOpacity = 0.5f;

        private readonly Rectangle rect;
        private readonly int tileSize;

        public bool IsVisible { get; private set; }


        public SquareHighlight(SolidColorBrush color, int tileSize)
        {
            this.tileSize = tileSize;
            IsVisible = false;

            rect = new Rectangle
            {
                Fill = color,
                Width = tileSize,
                Height = tileSize,
                Opacity = highlightOpacity
            };

            Panel.SetZIndex(rect, -2);
        }

        public void Show(Canvas canvas)
        {
            if (IsVisible) return;

            canvas.Children.Add(rect);
            IsVisible = true;
        }

        public void Hide(Canvas canvas)
        {
            if (!IsVisible) return;

            canvas.Children.Remove(rect);
            IsVisible = false;
        }

        public void InitPieces(Position pos)
        {
            Canvas.SetLeft(rect, pos.X * tileSize);
            Canvas.SetTop(rect, pos.Y * tileSize);
        }
    }
}
