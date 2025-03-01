using Axiom.src.core.Board;
using Axiom.src.core.Evaluation;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;
using System.Diagnostics;

namespace Axiom.src.core.Search
{
    public class Engine
    {


        const int PositiveInf = 999999999;
        const int NegativeInf = -999999999;


        public int SearchedNodes;
        public Move bestMove;
        public Move currentBestMove;
        public int eval;
        public int currentEval;

        public double startTime;
        public double timeLimit;



        public Board.Board board;

        public Engine()
        {
            board = new Board.Board();

            NegaMax(1, 0, NegativeInf, PositiveInf);
        }

        public void SetPosition(string fen)
        {
            board = new Board.Board(fen);
            InitSearch();
        }

        public void Search(int depthlimit, int timelimit = int.MaxValue)
        {
            InitSearch();
            this.timeLimit = timelimit;
            var watch = new Stopwatch();
            watch.Start();

            for (int depth = 1; depth <= depthlimit; depth++)
            {
                NegaMax(depth, 0, NegativeInf, PositiveInf);
                if (IsTimeUp)
                {
                    return;
                }
                bestMove = currentBestMove;
                eval = currentEval;
                Console.WriteLine($"info depth {depth} score cp {eval} nodes {SearchedNodes} nps {SearchedNodes / Math.Max(1, watch.ElapsedMilliseconds) * 1000} time {watch.ElapsedMilliseconds} pv {BoardUtility.MoveToUci(bestMove)}");
            }
            
          
            watch.Stop();
        }

        private void InitSearch()
        {
            SearchedNodes = 0;
            startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            currentBestMove = Move.NullMove;
        }


        private int NegaMax(int depth, int plyFromRoot, int alpha, int beta)
        {
            SearchedNodes++;
            if (depth <= 0)
            {
                return Evaluator.Evaluate(board);
            }

            // Not in root node
            if (plyFromRoot > 0)
            {
                if (board.IsThreefoldRepetition())
                {
                    return 0;
                }
            }


            Move[] pseudoLegalMoves = MoveGenerator.GetPseudoLegalMoves(board);
            MoveOrderer.OrderMoves(pseudoLegalMoves, board);
            int numLegalMoves = 0;
            for (int i = 0; i < pseudoLegalMoves.Length; i++)
            {
                Move move = pseudoLegalMoves[i];


                // Filter illegal castling moves
                if (move.MoveFlag == Move.CastleFlag)
                {
                    if (board.IsInCheck(board.WhiteToMove))
                    {
                        continue;
                    }
                    switch (move.TargetSquare)
                    {
                        case 62: // white short castle (g1)
                            if (board.IsUnderAttack(61, true) || board.IsUnderAttack(62, true)) { continue; }
                            break;
                        case 58: // white loing castle (c1)
                            if (board.IsUnderAttack(58, true) || board.IsUnderAttack(59, true)) { continue; }
                            break;
                        case 6: // black short castle (g8)
                            if (board.IsUnderAttack(5, false) || board.IsUnderAttack(6, false)) { continue; }
                            break;
                        case 2: // black long castle (c8)
                            if (board.IsUnderAttack(2, false) || board.IsUnderAttack(3, false)) { continue; }
                            break;
                    }
                }

                board.MakeMove(move);

                // Filter illegal moves
                if (board.IsInCheck(!board.WhiteToMove))
                {
                    board.UndoMove(move);
                    continue;
                }

                numLegalMoves++;

                int score = -NegaMax(depth - 1, plyFromRoot + 1, -beta, -alpha);

                board.UndoMove(move);

                if (IsTimeUp)
                {
                    return 0;
                }

                if (score > alpha)
                {
                    if (plyFromRoot == 0)
                    {
                        currentBestMove = move;
                        currentEval = score;
                    }
                    alpha = score;
                }

                if (beta <= alpha)
                {
                    return beta; // Return beta on cutoff
                }
            }

            if (numLegalMoves == 0)
            {
                if (board.IsInCheck(board.WhiteToMove))
                {
                    return NegativeInf; // Checkmate
                }
                return 0; // Stalemate
            }

            return alpha;
        }

        private int Quiecence(int alpha, int beta)
        {
            SearchedNodes++;
            int standingPat = Evaluator.Evaluate(board);

            if (standingPat >= beta)
            {
                return beta;
            }

            if (alpha < standingPat)
            {
                alpha = standingPat;
            }

            Move[] captureMoves = MoveGenerator.GetPseudoLegalCaptures(board);
            MoveOrderer.OrderMoves(captureMoves, board);

            if (IsTimeUp)
            {
                return standingPat;
            }

            foreach (Move move in captureMoves)
            {
                // No need to filter illegal castling moves
                // as they are not generated in qSearch

                board.MakeMove(move);

                // Filter illegal moves
                if (board.IsInCheck(!board.WhiteToMove))
                {
                    board.UndoMove(move);
                    continue;
                }

                int score = -Quiecence(-beta, -alpha);
                board.UndoMove(move);

                if (score >= beta)
                {
                    return beta;
                }
                if (score > alpha)
                {
                    alpha = score;
                }
            }

            return alpha;
        }

        bool IsTimeUp => DateTime.Now.TimeOfDay.TotalMilliseconds - startTime > timeLimit;
    }
}
