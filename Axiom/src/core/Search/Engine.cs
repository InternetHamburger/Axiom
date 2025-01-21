using Axiom.src.core.Board;
using Axiom.src.core.Evaluation;
using Axiom.src.core.Move_Generation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Search
{
    public class Engine
    {


        const int PositiveInf = 999999999;
        const int NegativeInf = -999999999;


        public int SearchedNodes;


        readonly Board.Board board;

        public Engine()
        {
            board = new Board.Board();

            NegaMax(1);
        }

        public void Search(int depthlimit, int timelimit = int.MaxValue)
        {
            InitSearch();
            var watch = new Stopwatch();
            watch.Start();
            Console.WriteLine(NegaMax(depthlimit));
            watch.Stop();
            Console.WriteLine(SearchedNodes);
            Console.WriteLine(SearchedNodes / watch.ElapsedMilliseconds * 1000);
        }

        private void InitSearch()
        {
            SearchedNodes = 0;
        }


        private int NegaMax(int depth)
        {
            SearchedNodes++;
            if (depth <= 0)
            {
                return Evaluator.Evaluate(board);
            }



            Move[] pseudoLegalMoves = MoveGenerator.GetPseudoLegalMoves(board);
            int maxScore = NegativeInf;
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

                int score = -NegaMax(depth - 1);

                board.UndoMove(move);

                if (score > maxScore)
                {
                    maxScore = score;
                }
                

            }

            return maxScore;
        }
    }
}
