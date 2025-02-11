using System.Windows;

namespace Chess
{
    public partial class MainWindow : Window
    {
        public int BoardSize { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ChessBoard board = new ChessBoard(this, cnv_boardWrapper, 640);

            Game game = new Game(board);

            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            game.LoadFENPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 48 1");

            game.Start();

            //board.MovePiece(new Position('b', 2), new Position('b', 4));
        }
    }
}
