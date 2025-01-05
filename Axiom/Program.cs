using Axiom.src.core.Board;
using Axiom.src.core.Utility;
using System.Numerics;

namespace Axiom
{
    internal class Program
    {
        static void Main()
        {
            Board board = new("8/8/8/8/8/8/8/R3K2R w KQ - 0 1");
            BoardUtility.PrintBoard(board);
            board.MakeMove(new Move(60, 58, Move.CastleFlag));
            BoardUtility.PrintBoard(board);
            board.UndoMove(new Move(60, 58, Move.CastleFlag));
            BoardUtility.PrintBoard(board);

        }
    }
}
