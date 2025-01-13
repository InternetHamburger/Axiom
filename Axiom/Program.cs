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
            foreach(ulong bitboard in PreComputedMoveData.KnightAttacks)
            {
                BitBoardUtlity.PrintBitBoard(bitboard);
            }
        }
    }
}
