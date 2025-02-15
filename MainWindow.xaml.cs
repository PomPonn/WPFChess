using System.Windows;

namespace Chess
{
    public partial class MainWindow : Window
    {
        public int BoardSize { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ChessBoard board = new ChessBoard(this, cnv_boardWrapper, 640)
            {
                Rotation = BoardRotation.WhiteBottom
            };

            Game game = new Game(board);

            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            // "k7/8/8/8/8/8/8/QK6 b - - 0 30"
            game.LoadFENPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.Start();
        }
    }
}
