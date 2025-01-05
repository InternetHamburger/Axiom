using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Utility
{
    static class MoveUtility
    {
        public static int File(int square) => square % 8;
        public static int Rank(int square) => square / 8;
    }
}
