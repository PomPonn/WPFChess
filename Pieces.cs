using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess
{
    public enum PieceType
    {
        Pawn = 'p',
        King = 'k',
        Queen = 'q',
        Knight = 'n',
        Bishop = 'b',
        Rook = 'r'
    }

    public class Piece
    {
        public PieceType Type { get; }
        public bool IsWhite { get; }

        public Piece(PieceType type, bool isWhite)
        {
            Type = type;
            IsWhite = isWhite;
        }
    }

    public class PieceRepresentation
    {
        public Image Visual { get; }
        public Piece Piece { get; }

        public PieceRepresentation(Piece piece, int imageSize)
        {
            Piece = piece;

            Visual = new Image
            {
                Source = new BitmapImage(new Uri(ResolvePieceImagePath(piece.Type, piece.IsWhite), UriKind.Relative)),
                Width = imageSize,
                Height = imageSize
            };
        }

        private string ResolvePieceImagePath(PieceType type, bool isWhite)
        {
            return $"images/pieces/{(isWhite ? 'w' : 'b')}_{(char)type}.png";
        }
    }
        //public class ChessPiece
        //{
        //    public string PieceName { get; set; }
        //    public PieceColor Color { get; set; }
        //    public Image VisualRepresentation { get; set; }

        //    protected string ResolvePieceImagePath(string pieceName)
        //    {
        //        return $"images/pieces/{(Color == PieceColor.White ? 'w' : 'b')}_{pieceName}.png";
        //    }

        //    public ChessPiece(PieceColor Color, string PieceName, int imageSize)
        //    {
        //        this.Color = Color;
        //        this.PieceName = PieceName;

        //        var test = new BitmapImage(new Uri(ResolvePieceImagePath(PieceName), UriKind.Relative));

        //        VisualRepresentation = new Image
        //        {
        //            Source = new BitmapImage(new Uri(ResolvePieceImagePath(PieceName), UriKind.Relative)),
        //            Width = imageSize,
        //            Height = imageSize
        //        };
        //    }
        //}

        //public class Pawn : ChessPiece
        //{
        //    public Pawn(PieceColor color, int imageSize) : base(color, "pawn", imageSize)
        //    { }
        //}

        //public class King : ChessPiece
        //{
        //    public King(PieceColor color, int imageSize) : base(color, "king", imageSize)
        //    { }
        //}

        //public class Queen : ChessPiece
        //{
        //    public Queen(PieceColor color, int imageSize) : base(color, "queen", imageSize)
        //    { }
        //}

        //public class Knight : ChessPiece
        //{
        //    public Knight(PieceColor color, int imageSize) : base(color, "knight", imageSize)
        //    { }
        //}

        //public class Bishop : ChessPiece
        //{
        //    public Bishop(PieceColor color, int imageSize) : base(color, "bishop", imageSize)
        //    { }
        //}

        //public class Rook : ChessPiece
        //{
        //    public Rook(PieceColor color, int imageSize) : base(color, "rook", imageSize)
        //    { }
        //}

    }
