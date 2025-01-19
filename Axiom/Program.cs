using Axiom.src;
using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Perft;
using Axiom.src.core.Utility;

namespace Axiom
{
    internal class Program
    {
        static void Main()
        {
            Perft.RunSuite(5);
        }
    }
}
