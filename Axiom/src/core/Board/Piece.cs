using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Board
{
    public static class Piece
    {

        // Represent the pieces with four bits, aka a nibble :)
        // Format is ttttcppp
        // t is redundant, c is the color, p is the piece type
        public const byte ColorMask = 0b1000;
        public const byte PieceTypeMask = 0b0111;

        public const byte White = 0b0000;
        public const byte Black = 0b1000;

        public const byte None = 0b0000;

        public const byte Pawn = 0b0001;
        public const byte Knight = 0b0010;
        public const byte Bishop = 0b0011;
        public const byte Rook = 0b0100;
        public const byte Queen = 0b0101;
        public const byte King = 0b0110;

        public const byte WhitePawn = Pawn | White;
        public const byte WhiteKnight = Knight | White;
        public const byte WhiteBishop = Bishop | White;
        public const byte WhiteRook = Rook | White;
        public const byte WhiteQueen = Queen | White;
        public const byte WhiteKing = King | White;

        public const byte BlackPawn = Pawn | Black;
        public const byte BlackKnight = Knight | Black;
        public const byte BlackBishop = Bishop | Black;
        public const byte BlackRook = Rook | Black;
        public const byte BlackQueen = Queen | Black;
        public const byte BlackKing = King | Black;

        public const byte MaxPieceIndex = BlackKing;


        // Returns true if given piece matches the given colour. If piece is of type 'none', result will always be false.
        public static bool IsColour(int piece, int color) => (piece & ColorMask) == color && piece != 0;

        public static bool IsWhite(int piece) => IsColour(piece, White);

        public static int PieceType(int piece) => piece & PieceTypeMask;

        // Rook or Queen
        public static bool IsOrthogonalSlider(int piece) => PieceType(piece) is Queen or Rook;

        // Bishop or Queen
        public static bool IsDiagonalSlider(int piece) => PieceType(piece) is Queen or Bishop;

        // Bishop, Rook, or Queen
        public static bool IsSlidingPiece(int piece) => PieceType(piece) is Queen or Bishop or Rook;

        public static char GetSymbol(int piece)
        {
            int pieceType = PieceType(piece);
            char symbol = pieceType switch
            {
                Rook => 'R',
                Knight => 'N',
                Bishop => 'B',
                Queen => 'Q',
                King => 'K',
                Pawn => 'P',
                _ => ' '
            };
            symbol = IsWhite(piece) ? symbol : char.ToLower(symbol);
            return symbol;
        }

        public static byte? GetType(char piece)
        {
            return piece switch
            {
                'P' => WhitePawn,
                'N' => WhiteKnight,
                'B' => WhiteBishop,
                'R' => WhiteRook,
                'Q' => WhiteQueen,
                'K' => WhiteKing,
                'p' => BlackPawn,
                'n' => BlackKnight,
                'b' => BlackBishop,
                'r' => BlackRook,
                'q' => BlackQueen,
                'k' => BlackKing,
                _ => null
            };
        }
    }
}
