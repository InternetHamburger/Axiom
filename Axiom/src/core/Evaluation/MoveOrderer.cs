using Axiom.src.core.Board;

namespace Axiom.src.core.Evaluation
{
    static class MoveOrderer
    {
        public static void OrderMoves(Move[] moves, Board.Board board, Move ttMove, Move killerMove)
        {
            Array.Sort(moves, (a, b) => MoveScore(b, board, ttMove, killerMove).CompareTo(MoveScore(a, board, ttMove, killerMove)));
        }

        public static void OrderCaptures(Move[] moves, Board.Board board)
        {
            Array.Sort(moves, (a, b) => CaptureScore(b, board).CompareTo(CaptureScore(a, board)));
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

        static int MoveScore(Move move, Board.Board board, Move ttMove, Move killerMove)
        {
            if (Move.SameMove(ttMove, move))
            {
                return 10000;
            }
            else if (Move.SameMove(move, killerMove))
            {
                return 9999;
            }
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

        static int CaptureScore(Move move, Board.Board board)
        {
            int captureMaterialDelta = CaptureScoreDelta(move, board);
            int moveScore = captureMaterialDelta;

            return moveScore;
        }
    }
}
