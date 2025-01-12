using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;
using System.Numerics;

namespace Axiom
{
    internal class Program
    {
        static void Main()
        {
            Board board = new("4n3/5P2/8/8/8/3kq1q1/3P1P2/8 w - - 0 1");
            BoardUtility.PrintBoard(board);
            Move[] moves = MoveGenerator.GetPseudoLegalMoves(board);
            foreach(Move move in moves)
            {
                Console.WriteLine(BoardUtility.MoveToUci(move));
            }
        }
    }
}
