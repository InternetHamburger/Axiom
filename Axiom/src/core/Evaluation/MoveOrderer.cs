using Axiom.src.core.Board;

namespace Axiom.src.core.Evaluation
{
    public class MoveOrderer
    {
        public int[,,] HistoryTable;
        public Move[] KillerMoves;

        public MoveOrderer()
        {
            HistoryTable = new int[2, Piece.MaxPieceIndex + 1, 64];
            KillerMoves = new Move[256];
        }

        public void Init()
        {
            for (int i = 0; i < 2; i++)
            {
                for(int j = 0; j < Piece.MaxPieceIndex + 1; j++)
                {
                    for (int k = 0; k < 64; k++)
                    {
                        HistoryTable[i, j, k] = 0;
                    }
                }
            }
            for (int i = 0; i < 256; i++)
            {
                KillerMoves[i] = new Move(0);
            }
            //HistoryTable = new int[2, Piece.MaxPieceIndex + 1, 64];
            //KillerMoves = new Move[256];
        }

        public void OrderMoves(Move[] moves, Board.Board board, Move ttMove, int plyFromRoot)
        {
            (int, Move)[] MoveScores = new (int, Move)[moves.Length];
            for (int i = 0; i < MoveScores.Length; i++)
            {
                MoveScores[i] = (MoveScore(moves[i], board, ttMove, KillerMoves[plyFromRoot]), moves[i]);
            }
            Array.Sort(MoveScores, (a, b) => b.Item1.CompareTo(a.Item1));

            for (int i = 0; i < MoveScores.Length; i++)
            {
                moves[i] = MoveScores[i].Item2;
            }
        }

        public void OrderCaptures(Move[] moves, Board.Board board)
        {
            Array.Sort(moves, (a, b) => CaptureScore(b, board).CompareTo(CaptureScore(a, board)));
        }

        private static int CaptureScoreDelta(Move move, Board.Board board)
        {
            return (100 * MoveOrderPieceValue(board.Squares[move.TargetSquare])) - MoveOrderPieceValue(board.Squares[move.StartSquare]);
        }

        private static int MoveOrderPieceValue(byte piece)
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

        private int MoveScore(Move move, Board.Board board, Move ttMove, Move killerMove)
        {
            if (Move.SameMove(ttMove, move))
            {
                return 10000;
            }
            else if (Move.SameMove(move, killerMove))
            {
                return 1;
            }
            if (board.Squares[move.TargetSquare] != 0)
            {
                int captureMaterialDelta = CaptureScoreDelta(move, board);
                int moveScore = captureMaterialDelta;

                return moveScore;
            }
            else
            {
                int index1 = board.WhiteToMove ? 0 : 1;
                int index2 = board.Squares[move.TargetSquare];
                int index3 = move.TargetSquare;
                return HistoryTable[index1, index2, index3] - 10000;
            }
        }

        private static int CaptureScore(Move move, Board.Board board)
        {
            int captureMaterialDelta = CaptureScoreDelta(move, board);
            int moveScore = captureMaterialDelta;

            return moveScore;
        }

        public void UpdateHistoryTableBetaCutoff(Board.Board board, Move move, int depth)
        {
            int index1 = board.WhiteToMove ? 0 : 1;
            int index2 = board.Squares[move.TargetSquare];
            int index3 = move.TargetSquare;
            HistoryTable[index1, index2, index3] = Math.Clamp(HistoryTable[index1, index2, index3] + (3 * depth * depth), 0, 10000);
        }

        public void UpdateHistoryTableAlphaRaise(Board.Board board, Move move, int depth)
        {
            int index1 = board.WhiteToMove ? 0 : 1;
            int index2 = board.Squares[move.TargetSquare];
            int index3 = move.TargetSquare;
            HistoryTable[index1, index2, index3] = Math.Clamp(HistoryTable[index1, index2, index3] + (2 * depth), 0, 10000);
        }
    }
}
