using System.Windows;

namespace Chess
{

    public partial class MainWindow : Window
    {
        // should be divisible by 8, to avoid weird pieces offset
        public int BoardSize { get; set; }

        public MainWindow()
        {
            BoardSize = 640;

            DataContext = this;


            InitializeComponent();

            //var test = FEN.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");


            Board board = new Board(cnv_boardWrapper, "images/board.png", BoardSize);

            board.LoadFromFENString("rnbqkbnr/pppp1ppp/8/4p3/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            board.WriteBoardCanvas();

            //board.LoadFromFENString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            //board.WriteBoardCanvas();

            //int tileSize = BoardSize / 8;

            //for (int i = 0; i < 8; i++)
            //{
            //    ChessPiece piece = new Pawn(PieceColor.White, tileSize);

            //    Canvas.SetTop(piece.VisualRepresentation, tileSize);
            //    Canvas.SetLeft(piece.VisualRepresentation, tileSize * i);

            //    cnv_boardWrapper.Children.Add(piece.VisualRepresentation);
            //}

            //ChessPiece king = new King(PieceColor.White, tileSize);

            //Canvas.SetTop(king.VisualRepresentation, 0);
            //Canvas.SetLeft(king.VisualRepresentation, tileSize * 4);
            //cnv_boardWrapper.Children.Add(king.VisualRepresentation);

            //ChessPiece queen = new Queen(PieceColor.White, tileSize);
            //Canvas.SetLeft(king.VisualRepresentation, tileSize * 6);
            //cnv_boardWrapper.Children.Add(queen.VisualRepresentation);

            //Canvas.SetTop(queen.VisualRepresentation, 0);

            //for (int i = 0; i < 8; i++)
            //{
            //    ChessPiece piece = new Pawn(PieceColor.Black, tileSize);

            //    Canvas.SetBottom(piece.VisualRepresentation, tileSize);
            //    Canvas.SetLeft(piece.VisualRepresentation, tileSize * i);

            //    cnv_boardWrapper.Children.Add(piece.VisualRepresentation);
            //}
        }
    }
}
