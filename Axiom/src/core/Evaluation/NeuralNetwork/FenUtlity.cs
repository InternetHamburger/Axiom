using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerual_Network.Chess
{
    static class FenUtlity
    {

        public static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Pow(10, -x / 400));
        }

        public static double[] FenToArray(string mainFen, bool IsUs)
        {
            byte[] squarePieces = new byte[64];

            int squareIndex = 0;
            int fenIndex = 0;
            while (fenIndex < mainFen.Length)
            {
                char c = mainFen[fenIndex];

                if (int.TryParse(c.ToString(), out int num))
                {
                    squareIndex += num;
                    fenIndex++;
                    continue;
                }

                byte? piece = Piece.GetType(c);
                
                if (piece == null)
                {
                    fenIndex++;
                }
                else
                {
                    squarePieces[squareIndex] = (byte)piece;

                    squareIndex++;
                    fenIndex++;
                }
            }
            double[] nnInput = new double[64 * 6 * 2];
            for (int i = 0; i < 64; i++)
            {

                byte piece = squarePieces[BoardUtility.FlipSquare(i)];
                if (piece != 0)
                {
                    if (IsUs)
                    {
                        nnInput[PreComputedMoveData.NNInputIndicies[0, piece, i]] = 1;
                    }
                    else
                    {

                        nnInput[PreComputedMoveData.NNInputIndicies[1, piece, i]] = 1;
                    }
                }
            }
            return nnInput;
        }

        public static string GetMainPart(string fen)
        {
            return fen.Split(" ")[0];
        }
    }
}
