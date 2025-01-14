using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Move_Generation
{
    public static class MoveGenConstants
    {
        public const ulong AFileMask = 0x0101010101010101;
        public const ulong FirstRankMask = 0xFF00000000000000;

        public const ulong WhiteMoveMask = ~BlackStartRank;
        public const ulong BlackMoveMask = ~WhiteStartRank;

        public const ulong WhitePawnCaptureRightMask = ~(AFileMask << 7);
        public const ulong WhitePawnCaptureLeftMask = ~AFileMask;

        public const ulong BlackPawnCaptureRightMask = WhitePawnCaptureLeftMask;
        public const ulong BlackPawnCaptureLeftMask = WhitePawnCaptureRightMask;

        public const ulong WhiteStartRank = FirstRankMask >> 8;
        public const ulong BlackStartRank = FirstRankMask >> 48;

        public const ulong WhitePromotionMask = BlackStartRank;
        public const ulong BlackPromotionMask = WhiteStartRank;

        public const ulong WhiteCastleKing = (1UL << 62) | (1UL << 61);



        public static readonly int[] DirectionOffSets = [-8, 8, -1, 1, -9, 9, -7, 7];

        public static readonly int[,] numSquaresToEdge =
        {
            { 0, 7, 0, 7, 0, 7, 0, 0, },
            { 0, 7, 1, 6, 0, 6, 0, 1, },
            { 0, 7, 2, 5, 0, 5, 0, 2, },
            { 0, 7, 3, 4, 0, 4, 0, 3, },
            { 0, 7, 4, 3, 0, 3, 0, 4, },
            { 0, 7, 5, 2, 0, 2, 0, 5, },
            { 0, 7, 6, 1, 0, 1, 0, 6, },
            { 0, 7, 7, 0, 0, 0, 0, 7, },
            { 1, 6, 0, 7, 0, 6, 1, 0, },
            { 1, 6, 1, 6, 1, 6, 1, 1, },
            { 1, 6, 2, 5, 1, 5, 1, 2, },
            { 1, 6, 3, 4, 1, 4, 1, 3, },
            { 1, 6, 4, 3, 1, 3, 1, 4, },
            { 1, 6, 5, 2, 1, 2, 1, 5, },
            { 1, 6, 6, 1, 1, 1, 1, 6, },
            { 1, 6, 7, 0, 1, 0, 0, 6, },
            { 2, 5, 0, 7, 0, 5, 2, 0, },
            { 2, 5, 1, 6, 1, 5, 2, 1, },
            { 2, 5, 2, 5, 2, 5, 2, 2, },
            { 2, 5, 3, 4, 2, 4, 2, 3, },
            { 2, 5, 4, 3, 2, 3, 2, 4, },
            { 2, 5, 5, 2, 2, 2, 2, 5, },
            { 2, 5, 6, 1, 2, 1, 1, 5, },
            { 2, 5, 7, 0, 2, 0, 0, 5, },
            { 3, 4, 0, 7, 0, 4, 3, 0, },
            { 3, 4, 1, 6, 1, 4, 3, 1, },
            { 3, 4, 2, 5, 2, 4, 3, 2, },
            { 3, 4, 3, 4, 3, 4, 3, 3, },
            { 3, 4, 4, 3, 3, 3, 3, 4, },
            { 3, 4, 5, 2, 3, 2, 2, 4, },
            { 3, 4, 6, 1, 3, 1, 1, 4, },
            { 3, 4, 7, 0, 3, 0, 0, 4, },
            { 4, 3, 0, 7, 0, 3, 4, 0, },
            { 4, 3, 1, 6, 1, 3, 4, 1, },
            { 4, 3, 2, 5, 2, 3, 4, 2, },
            { 4, 3, 3, 4, 3, 3, 4, 3, },
            { 4, 3, 4, 3, 4, 3, 3, 3, },
            { 4, 3, 5, 2, 4, 2, 2, 3, },
            { 4, 3, 6, 1, 4, 1, 1, 3, },
            { 4, 3, 7, 0, 4, 0, 0, 3, },
            { 5, 2, 0, 7, 0, 2, 5, 0, },
            { 5, 2, 1, 6, 1, 2, 5, 1, },
            { 5, 2, 2, 5, 2, 2, 5, 2, },
            { 5, 2, 3, 4, 3, 2, 4, 2, },
            { 5, 2, 4, 3, 4, 2, 3, 2, },
            { 5, 2, 5, 2, 5, 2, 2, 2, },
            { 5, 2, 6, 1, 5, 1, 1, 2, },
            { 5, 2, 7, 0, 5, 0, 0, 2, },
            { 6, 1, 0, 7, 0, 1, 6, 0, },
            { 6, 1, 1, 6, 1, 1, 6, 1, },
            { 6, 1, 2, 5, 2, 1, 5, 1, },
            { 6, 1, 3, 4, 3, 1, 4, 1, },
            { 6, 1, 4, 3, 4, 1, 3, 1, },
            { 6, 1, 5, 2, 5, 1, 2, 1, },
            { 6, 1, 6, 1, 6, 1, 1, 1, },
            { 6, 1, 7, 0, 6, 0, 0, 1, },
            { 7, 0, 0, 7, 0, 0, 7, 0, },
            { 7, 0, 1, 6, 1, 0, 6, 0, },
            { 7, 0, 2, 5, 2, 0, 5, 0, },
            { 7, 0, 3, 4, 3, 0, 4, 0, },
            { 7, 0, 4, 3, 4, 0, 3, 0, },
            { 7, 0, 5, 2, 5, 0, 2, 0, },
            { 7, 0, 6, 1, 6, 0, 1, 0, },
            { 7, 0, 7, 0, 7, 0, 0, 0, },
        };

    }
}
