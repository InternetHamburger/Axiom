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
            Board board = new("8/8/8/8/8/8/8/Q7 w - - 0 1");
            MoveGenerator moveGenerator = new();
            Console.WriteLine(moveGenerator.GetPseudoLegalMoves(board).Length);
        }
    }
}
