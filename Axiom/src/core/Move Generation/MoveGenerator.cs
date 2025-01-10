using Axiom.src.core.Board;
using Axiom.src.core.Utility;
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
            int numGeneratedMoves = 0;
            int color = board.WhiteToMove ? Piece.White : Piece.Black;

            for (int i = 0; i < 64; i++)
            {
                byte piece = board.Squares[i];

                if (Piece.IsColour(piece, color))
                {
                    if (Piece.IsSlidingPiece(piece))
                    {
                        GenerateSlidingMoves(board, ref moves, ref numGeneratedMoves, piece, i);
                    }
                    switch (Piece.PieceType(piece))
                    {
                    }

                }
            }

            // Slice off ungenerated moves
            if (numGeneratedMoves == 218)
            {
                return moves;
            }
            else
            {
                Move[] result = new Move[numGeneratedMoves];
                Array.Copy(moves, result, numGeneratedMoves);
                return result;
            }
        }



        private void GenerateSlidingMoves(Board.Board board, ref Move[] moves, ref int numGeneratedMoves, byte piece, int square)
        {
            int startIndex = Piece.IsOrthogonalSlider(piece) ? 0 : 4;
            int endIndex = Piece.IsDiagonalSlider(piece) ? 8 : 4;

            for (int directionIndex = startIndex; directionIndex < endIndex; directionIndex++)
            {
                for (int n = 0; n < MoveGenConstants.numSquaresToEdge[square, directionIndex]; n++)
                {

                    int targetSquare = square + MoveGenConstants.DirectionOffSets[directionIndex] * (n + 1);
                    byte pieceOnTargetSquare = board.Squares[targetSquare];

                    // Blocked by friendly piece
                    if (Piece.IsColour(pieceOnTargetSquare, board.WhiteToMove ? 0 : 8))
                    {
                        break;
                    }
                    Console.WriteLine(BoardUtility.NameOfSquare(square) + BoardUtility.NameOfSquare(targetSquare));
                    moves[numGeneratedMoves++] = new Move(square, targetSquare);

                    if (Piece.IsColour(pieceOnTargetSquare, board.WhiteToMove ? 8 : 0))
                    {
                        break;
                    }
                }
            }
        }

    }
}
