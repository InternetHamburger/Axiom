using Axiom.src.core.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Evaluation
{
    static class Evaluator
    {
        public static readonly int[,] MaterialValues;


        static Evaluator()
        {
            MaterialValues = new int[Piece.MaxPieceIndex + 1, 64];

            for (int i = 0; i < 64; i++)
            {
                MaterialValues[Piece.WhitePawn, i] = 100;
                MaterialValues[Piece.WhiteKnight, i] = 300;
                MaterialValues[Piece.WhiteBishop, i] = 300;
                MaterialValues[Piece.WhiteRook, i] = 500;
                MaterialValues[Piece.WhiteQueen, i] = 900;
                MaterialValues[Piece.WhiteKing, i] = 0;

                MaterialValues[Piece.BlackPawn, i] = -100;
                MaterialValues[Piece.BlackKnight, i] = -300;
                MaterialValues[Piece.BlackBishop, i] = -300;
                MaterialValues[Piece.BlackRook, i] = -500;
                MaterialValues[Piece.BlackQueen, i] = -900;
                MaterialValues[Piece.BlackKing, i] = 0;
            }
        }

        public static int Evaluate(Board.Board board)
        {
            int eval = 0;
            for (int i = 0; i < 64; i++)
            {
                eval += MaterialValue(board.Squares[i]);
            }

            return eval * (board.WhiteToMove ? 1 : -1);
        }

        public static int MaterialValue(byte piece)
        {
            return MaterialValues[piece, 0];
        }
    }
}