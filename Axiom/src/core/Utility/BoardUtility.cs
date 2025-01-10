using Axiom.src.core.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Utility
{
    public static class BoardUtility
    {
        public const string StartPos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public const string files = "abcdefgh";
        public const string ranks = "12345678";

        public static int File(int square) => square % 8;
        public static int Rank(int square) => square / 8;

        public static string NameOfSquare(int square)
        {
            int file = File(square);
            int rank = 7 - Rank(square);
            return files[file].ToString() + ranks[rank].ToString();
        }

        public static void PrintBoard(Board.Board board, bool fromWhitePerspective = true)
        {
            char[] b = new char[64];
            for (int i = 0; i < 64; i++)
            {
                b[i] = Piece.GetSymbol(board.Squares[i]);
            }

            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            Console.WriteLine($" | {b[8 * 1 - 8]} | {b[8 * 1 - 7]} | {b[8 * 1 - 6]} | {b[8 * 1 - 5]} | {b[8 * 1 - 4]} | {b[8 * 1 - 3]} | {b[8 * 1 - 2]} | {b[8 * 1 - 1]} | 8");
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            Console.WriteLine($" | {b[8 * 2 - 8]} | {b[8 * 2 - 7]} | {b[8 * 2 - 6]} | {b[8 * 2 - 5]} | {b[8 * 2 - 4]} | {b[8 * 2 - 3]} | {b[8 * 2 - 2]} | {b[8 * 2 - 1]} | 7");
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            Console.WriteLine($" | {b[8 * 3 - 8]} | {b[8 * 3 - 7]} | {b[8 * 3 - 6]} | {b[8 * 3 - 5]} | {b[8 * 3 - 4]} | {b[8 * 3 - 3]} | {b[8 * 3 - 2]} | {b[8 * 3 - 1]} | 6");
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            Console.WriteLine($" | {b[8 * 4 - 8]} | {b[8 * 4 - 7]} | {b[8 * 4 - 6]} | {b[8 * 4 - 5]} | {b[8 * 4 - 4]} | {b[8 * 4 - 3]} | {b[8 * 4 - 2]} | {b[8 * 4 - 1]} | 5");
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            Console.WriteLine($" | {b[8 * 5 - 8]} | {b[8 * 5 - 7]} | {b[8 * 5 - 6]} | {b[8 * 5 - 5]} | {b[8 * 5 - 4]} | {b[8 * 5 - 3]} | {b[8 * 5 - 2]} | {b[8 * 5 - 1]} | 4");
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            Console.WriteLine($" | {b[8 * 6 - 8]} | {b[8 * 6 - 7]} | {b[8 * 6 - 6]} | {b[8 * 6 - 5]} | {b[8 * 6 - 4]} | {b[8 * 6 - 3]} | {b[8 * 6 - 2]} | {b[8 * 6 - 1]} | 3");
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            Console.WriteLine($" | {b[8 * 7 - 8]} | {b[8 * 7 - 7]} | {b[8 * 7 - 6]} | {b[8 * 7 - 5]} | {b[8 * 7 - 4]} | {b[8 * 7 - 3]} | {b[8 * 7 - 2]} | {b[8 * 7 - 1]} | 2");
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            Console.WriteLine($" | {b[8 * 8 - 8]} | {b[8 * 8 - 7]} | {b[8 * 8 - 6]} | {b[8 * 8 - 5]} | {b[8 * 8 - 4]} | {b[8 * 8 - 3]} | {b[8 * 8 - 2]} | {b[8 * 8 - 1]} | 1");
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            Console.WriteLine("   a   b   c   d   e   f   g   h");
        }
    }
}
