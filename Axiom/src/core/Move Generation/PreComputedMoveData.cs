using Axiom.src.core.Board;
using Axiom.src.core.Utility;

namespace Axiom.src.core.Move_Generation
{
    public static class PreComputedMoveData
    {
        public static readonly ulong[] KnightAttacks;

        static PreComputedMoveData()
        {
            KnightAttacks = new ulong[64];

            int[] allKnightJumps = { 15, 17, -17, -15, 10, -6, 6, -10 };


            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                int y = BoardUtility.Rank(squareIndex);
                int x = squareIndex - y * 8;

                ulong knightBitboard = 0;
                foreach (int knightJumpDelta in allKnightJumps)
                {
                    int knightJumpSquare = squareIndex + knightJumpDelta;
                    if (knightJumpSquare >= 0 && knightJumpSquare < 64)
                    {
                        int knightSquareY = BoardUtility.Rank(knightJumpSquare);
                        int knightSquareX = knightJumpSquare - knightSquareY * 8;
                        // Ensure knight has moved max of 2 squares on x/y axis (to reject indices that have wrapped around side of board)
                        int maxCoordMoveDst = System.Math.Max(System.Math.Abs(x - knightSquareX), System.Math.Abs(y - knightSquareY));
                        if (maxCoordMoveDst == 2)
                        {
                            knightBitboard |= 1ul << knightJumpSquare;
                        }
                    }
                }

                KnightAttacks[squareIndex] = knightBitboard;
            }
        }


    }
}
