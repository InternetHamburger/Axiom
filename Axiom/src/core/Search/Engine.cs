using Axiom.src.core.Board;
using Axiom.src.core.Evaluation;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace Axiom.src.core.Search
{
    public class Engine
    {


        const int PositiveInf = 999999999;
        const int NegativeInf = -999999999;

        const int sizeTTMb = 32;
        const int sizeTTEntry = 16;
        const ulong numTTEntries = sizeTTMb * 1024 * 1024 / sizeTTEntry;


        public int SearchedNodes;
        public Move bestMoveThisIteration;
        public Move currentBestMove;
        public int eval;
        public int currentEval;

        public double startTime;
        public double timeLimit;

        public TTEntry[] TT;


        public Board.Board board;

        public Engine()
        {
            board = new Board.Board();
            TT = new TTEntry[numTTEntries];
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
            timeLimit = timelimit;



            int alpha;
            int beta;

            const int delta = 50;


            var watch = new Stopwatch();
            watch.Start();
            for (int depth = 1; depth <= depthlimit; depth++)
            {
                alpha = eval - delta;
                beta = eval + delta;


                NegaMax(depth, 0, alpha, beta);

                if (currentEval >= beta)
                {
                    alpha = eval - delta;

                    NegaMax(depth, 0, alpha, PositiveInf);
                }
                else if (currentEval <= alpha)
                {
                    beta = eval + delta;
                    NegaMax(depth, 0, NegativeInf, beta);
                }


                if (IsTimeUp)
                {
                    return;
                }
                bestMoveThisIteration = currentBestMove;
                this.eval = this.currentEval;
                Console.WriteLine($"info depth {depth} score {UCI.GetCorrectEval(this.eval)} nodes {SearchedNodes} nps {SearchedNodes / Math.Max(1, watch.ElapsedMilliseconds) * 1000} time {watch.ElapsedMilliseconds} pv {BoardUtility.MoveToUci(bestMoveThisIteration)}");
            }
            watch.Stop();
        }

        private void InitSearch()
        {
            TT = new TTEntry[numTTEntries];
            SearchedNodes = 0;
            startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            currentBestMove = Move.NullMove;
            currentEval = NegativeInf;
        }


        private int NegaMax(int depth, int plyFromRoot, int alpha, int beta)
        {
            
            ulong TTIndex = board.ZobristHash % numTTEntries;
            TTEntry ttEntry = TT[TTIndex];

            if (ttEntry.Depth >= depth && plyFromRoot > 0 && ttEntry.ZobristHash == board.ZobristHash)
            {
                if (ttEntry.IsExact)
                {
                    return ttEntry.Score;
                }
                else if (ttEntry.IsLowerBound && ttEntry.Score > alpha)
                {
                    alpha = ttEntry.Score;
                }
                else if (ttEntry.IsUpperBound && ttEntry.Score < beta)
                {
                    beta = ttEntry.Score;
                }

                if (alpha >= beta)
                {
                    return alpha; // Cutoff with the lower bound score
                }
            }

            SearchedNodes++;
            if (depth <= 0)
            {
                return Quiecence(alpha, beta);
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
            if (plyFromRoot == 0)
            {
                MoveOrderer.OrderMoves(pseudoLegalMoves, board, Move.NullMove);
            }
            else
            {
                MoveOrderer.OrderMoves(pseudoLegalMoves, board, new Move(ttEntry.BestMove));
            }
            
            
            int numLegalMoves = 0;
            int bestScore = NegativeInf;
            bool alphaWasRaised = false;
            Move bestMove = Move.NullMove;
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

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                    if (plyFromRoot == 0)
                    {
                        currentBestMove = move;
                        currentEval = score;
                    }
                    if (score > alpha)
                    {
                        alphaWasRaised = true;
                        alpha = score;
                    }
                }

                if (beta <= alpha)
                {
                    TT[TTIndex] = new(beta, depth, TTEntry.LowerBoundFlag, bestMove.Value, board.ZobristHash);
                    return bestScore; // Return beta on cutoff
                }
            }


            if (numLegalMoves == 0)
            {
                if (board.IsInCheck(board.WhiteToMove))
                {
                    return NegativeInf + plyFromRoot; // Checkmate
                }
                return 0; // Stalemate
            }



            if (alphaWasRaised)
            {
                TT[TTIndex] = new(bestScore, depth, TTEntry.ExactFlag, bestMove.Value, board.ZobristHash);
            }
            else
            {
                TT[TTIndex] = new(alpha, depth, TTEntry.UpperBoundFlag, bestMove.Value, board.ZobristHash);
            }
            return bestScore;
        }

        private int Quiecence(int alpha, int beta)
        {
            SearchedNodes++;
            int standingPat = Evaluator.Evaluate(board);

            if (standingPat >= beta)
            {
                return standingPat;
            }

            if (alpha < standingPat)
            {
                alpha = standingPat;
            }

            Move[] captureMoves = MoveGenerator.GetPseudoLegalCaptures(board);
            MoveOrderer.OrderCaptures(captureMoves, board);

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
