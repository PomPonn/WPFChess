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


using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Chess
{
    /// <summary>
    /// Reprezentuje podświetlenie ruchu na szachownicy
    /// </summary>
    public class MoveHighlight
    {
        private readonly SquareHighlight[] sqrs = new SquareHighlight[2];

        public bool IsVisible { get; private set; }

        /// <summary>
        /// Konstruktor główny
        /// </summary>
        /// <param name="color">kolor podświetlenia</param>
        /// <param name="tileSize">rozmiar pola</param>
        public MoveHighlight(SolidColorBrush color, int tileSize)
        {
            IsVisible = false;

            for (int i = 0; i < 2; i++)
            {
                sqrs[i] = new SquareHighlight(color, tileSize);
            }
        }

        /// <summary>
        /// Pokazuje podświetlenie na wskananym obiekcie <i>canvas</i>
        /// </summary>
        /// <param name="canvas">obiekt <i>canvas</i>, na którym wyświetlić obiekt</param>
        public void Show(Canvas canvas)
        {
            foreach (var sqr in sqrs)
            {
                sqr.Show(canvas);
            }

            IsVisible = true;
        }

        /// <summary>
        /// Obraca pozycje podświetlenia
        /// </summary>
        public void Rotate()
        {
            foreach (var sqr in sqrs)
            {
                sqr.Rotate();
            }
        }

        /// <summary>
        /// zmienia rozmiar pól podświetlenia
        /// </summary>
        /// <param name="newSize">nowy rozmiar pola</param>
        public void Resize(int newSize)
        {
            sqrs[0].Resize(newSize);
            sqrs[1].Resize(newSize);
        }

        /// <summary>
        /// Ukrywa podświetlenie na wskananym obiekcie <i>canvas</i>
        /// </summary>
        /// <param name="canvas">obiekt <i>canvas</i>, na którym ukryć obiekt</param>
        public void Hide(Canvas canvas)
        {
            foreach (var sqr in sqrs)
            {
                sqr.Hide(canvas);
            }

            IsVisible = false;
        }

        /// <summary>
        /// Ustawia pozycję podświetlenia
        /// </summary>
        /// <param name="from">pozycja początkowa</param>
        /// <param name="to">pozycja końcowa</param>
        public void SetPosition(Position from, Position to)
        {
            sqrs[0].SetPosition(from);
            sqrs[1].SetPosition(to);
        }
    }

    /// <summary>
    /// Reprezentuje podświetlenie pola na szachownicy
    /// </summary>
    public class SquareHighlight
    {
        static readonly float highlightOpacity = 0.5f;

        private readonly Rectangle rect;
        private int tileSize;

        public Position CurrentPos { get; private set; }
        public bool IsVisible { get; private set; }


        /// <summary>
        /// Konstruktor główny
        /// </summary>
        /// <param name="color">kolor podświetlenia</param>
        /// <param name="tileSize">rozmiar pola podświetlenia</param>
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

        /// <summary>
        /// Pokazuje podświetlenie na wskananym obiekcie <i>canvas</i>
        /// </summary>
        /// <param name="canvas">obiekt <i>canvas</i>, na którym wyświetlić obiekt</param>
        public void Show(Canvas canvas)
        {
            if (IsVisible) return;

            canvas.Children.Add(rect);
            IsVisible = true;
        }

        /// <summary>
        /// Ukrywa podświetlenie na wskananym obiekcie <i>canvas</i>
        /// </summary>
        /// <param name="canvas">obiekt <i>canvas</i>, na którym ukryć obiekt</param>
        public void Hide(Canvas canvas)
        {
            if (!IsVisible) return;

            canvas.Children.Remove(rect);
            IsVisible = false;
        }

        /// <summary>
        /// Obraca pozycje podświetlenia
        /// </summary>
        public void Rotate()
        {
            SetPosition(Position.Rotate(CurrentPos));
        }

        /// <summary>
        /// zmienia rozmiar pola podświetlenia
        /// </summary>
        /// <param name="newSize">nowy rozmiar pola</param>
        public void Resize(int newSize)
        {
            tileSize = newSize;

            rect.Width = newSize;
            rect.Height = newSize;

            SetPosition(CurrentPos);
        }

        /// <summary>
        /// Ustawia pozycję podświetlenia
        /// </summary>
        /// <param name="from">pozycja podświetlenia</param>
        /// <param name="to"></param>
        public void SetPosition(Position pos)
        {
            CurrentPos = pos;
            Canvas.SetLeft(rect, pos.X * tileSize);
            Canvas.SetTop(rect, pos.Y * tileSize);
        }
    }
}
