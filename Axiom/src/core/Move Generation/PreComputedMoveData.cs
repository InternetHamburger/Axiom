using Axiom.src.core.Board;
using Axiom.src.core.Utility;

namespace Axiom.src.core.Move_Generation
{
    public static class PreComputedMoveData
    {
        public static readonly ulong[] KnightAttacks;
        public static readonly ulong[] KingAttacks;
        public static readonly int[] DstFromCenter;
        public static readonly int[,,] NNInputIndicies;

        static PreComputedMoveData()
        {
            KnightAttacks = new ulong[64];
            KingAttacks = new ulong[64];
            DstFromCenter = new int[64];
            NNInputIndicies = new int[2, (Piece.MaxPieceIndex + 1), 64];
            int[] allKnightJumps = { 15, 17, -17, -15, 10, -6, 6, -10 };


            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                DstFromCenter[squareIndex] = GetDstFromCenter(squareIndex);
                int rank = BoardUtility.Rank(squareIndex);
                int file = squareIndex - (rank * 8);



                ulong knightBitboard = 0;

                foreach (int knightJumpDelta in allKnightJumps)
                {
                    int knightJumpSquare = squareIndex + knightJumpDelta;
                    if (knightJumpSquare >= 0 && knightJumpSquare < 64)
                    {
                        int knightSquareRank = BoardUtility.Rank(knightJumpSquare);
                        int knightSquareFile = knightJumpSquare - (knightSquareRank * 8);
                        // Ensure knight has moved max of 2 squares on x/y axis (to reject indices that have wrapped around side of board)
                        int maxCoordMoveDst = Math.Max(Math.Abs(file - knightSquareFile), Math.Abs(rank - knightSquareRank));
                        if (maxCoordMoveDst == 2)
                        {
                            knightBitboard |= 1ul << knightJumpSquare;
                        }
                    }
                }

                for (int stm = 0; stm < 2; stm++)
                {
                    for (int piece = 1; piece <= Piece.MaxPieceIndex; piece++)
                    {
                        for (int square = 0; square < 64; square++)
                        {
                            int pieceIndex = piece;
                            if (pieceIndex > Piece.WhiteKing) pieceIndex -= 2;
                            if (stm == 0)
                            {
                                int index = (64 * (pieceIndex - 1)) + square;
                                NNInputIndicies[stm, piece, square] = index;
                            }
                            else
                            {
                                pieceIndex += piece > Piece.WhiteKing ? -6 : 6;
                                int index = (64 * (pieceIndex - 1)) + BoardUtility.FlipSquare(square);
                                NNInputIndicies[stm, piece, square] = index;
                            }
                        }
                    }
                }


                KingAttacks[squareIndex] = ComputeKingAttacks(squareIndex);
                KnightAttacks[squareIndex] = knightBitboard;
            }
        }


        private static ulong ComputeKingAttacks(int square)
        {
            ulong attacks = 0UL;

            int rank = square / 8;
            int file = square % 8;

            // Relative moves of the king
            int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 }; // Row deltas
            int[] df = { -1, 0, 1, -1, 1, -1, 0, 1 }; // File deltas

            for (int i = 0; i < 8; i++)
            {
                int newRank = rank + dr[i];
                int newFile = file + df[i];

                // Check if the new position is on the board
                if (newRank >= 0 && newRank < 8 && newFile >= 0 && newFile < 8)
                {
                    int newSquare = (newRank * 8) + newFile;
                    attacks |= 1UL << newSquare;
                }
            }

            return attacks;
        }

        private static int GetDstFromCenter(int square)
        {
            int file = BoardUtility.File(square);
            int rank = BoardUtility.Rank(square);

            int dstToCentreFile = Math.Max(3 - file, file - 4);
            int dstToCentreRank = Math.Max(3 - rank, rank - 4);
            int dstToCentre = dstToCentreFile + dstToCentreRank;

            return dstToCentre;


        }

    }
}
