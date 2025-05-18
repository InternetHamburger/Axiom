using Axiom.src.core.Move_Generation;
using Axiom.src.core.Search;

namespace Axiom.src.core.Search
{
    static class SearchConstants
    {
        // Indexed by depth, i
        public static int[,] LMR_TABLE;

        static SearchConstants()
        {
            LMR_TABLE = new int[Engine.MaxPly + 1, MoveGenerator.MaxNumMoves + 1];
            for (int i = 0; i < Engine.MaxPly + 1; i++)
            {
                for (int j = 0; j < MoveGenerator.MaxNumMoves + 1; j++)
                {
                    // Formula yoinked from heimdall
                    LMR_TABLE[i, j] = (int)(0.8 + Math.Log(i) * Math.Log(j) * 0.4);
                }
            }
        }
    }
}
