using Axiom.src.core.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Evaluation
{
    static class MoveOrderer
    {
        public static void OrderMoves(ref Move[] moves, Board.Board board)
        {
            Array.Sort(moves, (a, b) => MoveScore(b, board).CompareTo(MoveScore(a, board)));
            Array.Reverse(moves);
        }

        static int CaptureScoreDelta(Move move, Board.Board board)
        {
            return MoveOrderPieceValue(board.Squares[move.TargetSquare]) - MoveOrderPieceValue(board.Squares[move.StartSquare]);
        }

        static int MoveOrderPieceValue(byte piece)
        {
            return Piece.PieceType(piece) switch
            {
                Piece.Pawn => 1,
                Piece.Knight => 3,
                Piece.Bishop => 3,
                Piece.Rook => 5,
                Piece.Queen => 9,
                _ => 0
            };
        }

        static int MoveScore(Move move, Board.Board board)
        {
            if (board.Squares[move.TargetSquare] != 0)
            {
                int captureMaterialDelta = CaptureScoreDelta(move, board);
                int moveScore = captureMaterialDelta;

                return moveScore;
            }
            else
            {
                return 0;
            }
        }
    }
}
