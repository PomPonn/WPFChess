using System.Windows;

namespace Chess
{
    public partial class MainWindow : Window
    {
        public int BoardSize { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ChessBoard board = new(this, cnv_boardWrapper, 640)
            {
                Rotation = BoardRotation.BlackBottom,
            };

            GameManager game = new(board);

            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" -- starting position
            // "k7/8/8/8/8/8/8/QK6 b - - 0 30" -- check test
            // "k7/7P/8/8/8/8/8/QK6 b - - 0 30" -- pawn succession test
            game.LoadFENPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            game.StartGameAgainstBot(2, true);
            //game.StartLocalGame();
        }
    }
}
