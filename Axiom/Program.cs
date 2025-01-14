using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Perft;
using Axiom.src.core.Utility;
using System.Numerics;

namespace Axiom
{
    internal class Program
    {
        static void Main()
        {
            Perft.PerftSearch(BoardUtility.StartPos, 1);
        }
    }
}
