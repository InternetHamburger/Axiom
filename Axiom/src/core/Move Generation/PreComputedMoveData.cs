using Axiom.src.core.Board;
using Axiom.src.core.Utility;

namespace Axiom.src.core.Move_Generation
{
    public static class PreComputedMoveData
    {
        public static readonly Move[][] WhitePawnPushes;

        static PreComputedMoveData()
        {
            WhitePawnPushes = new Move[256][];
        }


    }
}
