using Axiom.src.core.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Move_Generation
{
    public class MoveGenerator
    {
        public const int MaxNumMoves = 218;


        public Move[] GetPseudoLegalMoves(Board.Board board)
        {
            Move[] moves = new Move[MaxNumMoves];
            int NumGeneratedMoves = 0;
            int color = board.WhiteToMove ? Piece.White : Piece.Black;

            for (int i = 0; i < 64; i++)
            {
                int piece = board.Squares[i];

                if (Piece.IsColour(piece, color))
                {
                    switch (Piece.PieceType(piece))
                    {

                    }
                }
            }

            // Slice off ungenerated moves
            if (NumGeneratedMoves == 218)
            {
                return moves;
            }
            else
            {
                Move[] result = new Move[NumGeneratedMoves];
                Array.Copy(moves, result, NumGeneratedMoves);
                return result;
            }
        }
    }
}
