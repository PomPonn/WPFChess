using System.Windows;
using System.Windows.Input;

namespace Chess
{
    public partial class MainWindow : Window
    {
        public int BoardSize { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ChessBoard board = new ChessBoard(cnv_boardWrapper, 720);

            Game game = new Game(board);

            game.LoadFENPosition("r3kb1r/pp1qpp1p/3p1np1/4n3/Q7/2N5/PPP2PPP/R1B1K1NR w KQkq - 0 10");

            //board.MovePiece(new Position('b', 2), new Position('b', 4));
        }
    }
}
