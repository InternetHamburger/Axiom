using Axiom.src.core.Board;
using Axiom.src.core.Utility;
using System.Numerics;

namespace Axiom
{
    internal class Program
    {
        static void Main()
        {
            Board board = new();
            BitBoardUtlity.PrintBitBoard(board.BitBoards[Piece.BlackKing]);
        }
    }
}
