using Axiom.src.core.Board;
using Axiom.src.core.Utility;
using System.Numerics;

namespace Axiom.src.core.Search
{

    // Implementation yoinked from Ethereal
    // https://github.com/AndyGrant/Ethereal/blob/master/src/search.c#L929
    static class StaticExchangeEvaluation
    {
        public static int[] SEEPieceValues = {
            0, // None
            100, // Pawn
            320, // Knight
            330, // Bishop
            500, // Rook
            900, // Queen
            20000 // King
        };

        public static bool SEE(Board.Board board, Move move, int threshold)
        {
            int from, to, type, balance, nextVictim;
            bool color;
            ulong bishops, rooks, queens, occupied, attackers, myAttackers;

            from = move.StartSquare;
            to = move.TargetSquare;
            type = move.MoveFlag;

            nextVictim = !move.IsPromotion ? Piece.PieceType(board.Squares[from]) : move.PromotionPieceType;

            balance = moveEstimValue(board, move) - threshold;

            if (balance < 0)
            {
                return false;
            }

            balance -= SEEPieceValues[nextVictim];

            if (balance >= 0)
            {
                return true;
            }
            queens = board.BitBoards[Piece.WhiteQueen] | board.BitBoards[Piece.BlackQueen];

            bishops = board.BitBoards[Piece.WhiteBishop] | board.BitBoards[Piece.BlackBishop] | queens;

            rooks = board.BitBoards[Piece.WhiteRook] | board.BitBoards[Piece.BlackRook] | queens;


            occupied = board.AllPieceBitBoard;
            occupied = (occupied ^ (1UL << from)) | (1UL << to);
            if (move.IsEnPassantCapture)
            {
                int enPassantCaptureSquare = to + (board.WhiteToMove ? 8 : -8);
                occupied ^= (1UL << enPassantCaptureSquare);
            }

            attackers = board.Attackers(to, occupied) & occupied;

            color = !board.WhiteToMove;

            while (true)
            {
                myAttackers = attackers & (color ? board.WhitePieceBitBoard : board.BlackPieceBitBoard);
                if (myAttackers == 0)
                {
                    break;
                }

                for (nextVictim = Piece.Pawn; nextVictim <= Piece.King; nextVictim++)
                {
                    if ((myAttackers & (board.BitBoards[nextVictim] | board.BitBoards[nextVictim + Piece.Black])) != 0)
                    {
                        break;
                    }
                }
                int LSPpos = BitOperations.TrailingZeroCount(myAttackers & (board.BitBoards[nextVictim] | board.BitBoards[nextVictim + Piece.Black]));
                occupied ^= 1UL << LSPpos;


                attackers = board.Attackers(to, occupied) & occupied;


                color = !color;

                balance = -balance - 1 - SEEPieceValues[nextVictim];


                if (balance >= 0)
                {
                    if (nextVictim == Piece.King && (attackers & (color ? board.WhitePieceBitBoard : board.BlackPieceBitBoard)) != 0)
                    {
                        color = !color;
                    }

                    break;
                }
            }
            return board.WhiteToMove != color;
        }

        private static int moveEstimValue(Board.Board board, Move move)
        {
            int value = SEEPieceValues[Piece.PieceType(board.Squares[move.TargetSquare])];

            if (move.IsPromotion)
            {
                value += SEEPieceValues[move.PromotionPieceType] - SEEPieceValues[Piece.Pawn];
            }
            else if (move.IsEnPassantCapture)
            {
                value = SEEPieceValues[Piece.Pawn];
            }

            return value;
        }
    }
}
