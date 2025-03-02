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

        public void Rotate()
        {
            foreach (var sqr in sqrs)
            {
                sqr.Rotate();
            }
        }

        public void Resize(int newSize)
        {
            sqrs[0].Resize(newSize);
            sqrs[1].Resize(newSize);
        }

        public void Hide(Canvas canvas)
        {
            foreach (var sqr in sqrs)
            {
                sqr.Hide(canvas);
            }

            IsVisible = false;
        }

        public void SetPosition(Position from, Position to)
        {
            sqrs[0].SetPosition(from);
            sqrs[1].SetPosition(to);
        }
    }

    public class SquareHighlight
    {
        static readonly float highlightOpacity = 0.5f;

        private readonly Rectangle rect;
        private int tileSize;

        public Position CurrentPos { get; private set; }
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

        public void Rotate()
        {
            SetPosition(Position.Rotate(CurrentPos));
        }

        public void Resize(int newSize)
        {
            tileSize = newSize;

            rect.Width = newSize;
            rect.Height = newSize;

            SetPosition(CurrentPos);
        }

        public void SetPosition(Position pos)
        {
            CurrentPos = pos;
            Canvas.SetLeft(rect, pos.X * tileSize);
            Canvas.SetTop(rect, pos.Y * tileSize);
        }
    }
}
