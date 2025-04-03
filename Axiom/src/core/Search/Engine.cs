using Axiom.src.core.Board;
using Axiom.src.core.Evaluation;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Axiom.src.core.Search
{
    public class Engine
    {


        const int PositiveInf = 999999999;
        const int NegativeInf = -999999999;

        const int sizeTTMb = 16;
        const int sizeTTEntry = 16;
        const ulong numTTEntries = sizeTTMb * 1024 * 1024 / sizeTTEntry;

        public TTEntry[] TT;

        public int GamePhase;
        public const int TotalPhase = 24;

        public int SearchedNodes;
        public Move bestMoveThisIteration;
        public Move currentBestMove;
        public int eval;
        public int currentEval;
        public int maxDepth;
        public bool printInfo;

        public double startTime;
        public double timeLimit;
        public int NodeLimit;


        public Board.Board board;
        public MoveOrderer moveOrderer;



        public Engine()
        {
            board = new Board.Board();
            moveOrderer = new();
            TT = new TTEntry[numTTEntries];
            printInfo = true;
            NegaMax(1, 0, NegativeInf, PositiveInf);
        }

        public void SetPosition(string fen)
        {
            board = new Board.Board(fen);
            InitSearch();
        }

        public void Search(int depthlimit, int timelimit = int.MaxValue, int nodeLimit = int.MaxValue)
        {
            InitSearch();
            timeLimit = timelimit;
            NodeLimit = nodeLimit;

            var watch = new Stopwatch();
            watch.Start();
            for (int depth = 1; depth <= depthlimit; depth++)
            {

                // Initial call
                // Very tight bounds, but pays off
                NegaMax(depth, 0, NegativeInf, PositiveInf);
                maxDepth = depth;


                if (IsTimeUp || SearchedNodes >= NodeLimit)
                {
                    return;
                }
                bestMoveThisIteration = currentBestMove;
                eval = currentEval;
                if (printInfo) Console.WriteLine($"info depth {depth} score {UCI.GetCorrectEval(eval)} nodes {SearchedNodes} nps {SearchedNodes / Math.Max(1, watch.ElapsedMilliseconds) * 1000} time {watch.ElapsedMilliseconds} pv {GetPv()}");
            }
            watch.Stop();
        }

        private void InitSearch()
        {
            maxDepth = 0;
            eval = 0;
            SearchedNodes = 0;
            moveOrderer.Init();
            TT = new TTEntry[numTTEntries];
            startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            currentBestMove = Move.NullMove;
            currentEval = NegativeInf;
            CalculateGamePhase();
        }


        private int NegaMax(int depth, int plyFromRoot, int alpha, int beta)
        {
            SearchedNodes++;
            if (board.IsTwofoldRepetition() && plyFromRoot > 0)
            {
                return 0;
            }
            if (depth == 0)
            {
                return Quiecence(alpha, beta);
            }



            ulong TTIndex = board.ZobristHash % numTTEntries;
            TTEntry ttEntry = TT[TTIndex];
            if (ttEntry.Depth == depth && plyFromRoot > 0 && ttEntry.ZobristHash == board.ZobristHash)
            {
                if (ttEntry.IsExact)
                {
                    return ttEntry.Score;
                }
                if (ttEntry.IsLowerBound && ttEntry.Score > alpha)
                {
                    alpha = ttEntry.Score;
                }
                else if (ttEntry.IsUpperBound && ttEntry.Score < beta)
                {
                    beta = ttEntry.Score;
                }
                if (alpha >= beta)
                {
                    return ttEntry.Score;
                }
            }

            int maxScore = NegativeInf;
            int numLegalMoves = 0;
            bool alphaWasRaised = false;
            Move[] pseudoLegalMoves = MoveGenerator.GetPseudoLegalMoves(board);
            moveOrderer.OrderMoves(pseudoLegalMoves, board, new(ttEntry.BestMove), plyFromRoot);
            Move bestMove = Move.NullMove;
            for (int i = 0; i < pseudoLegalMoves.Length; i++)
            {
                Move move = pseudoLegalMoves[i];
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
                        case 58: // white long castle (c1)
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
                if (board.IsInCheck(!board.WhiteToMove))
                {
                    board.UndoMove(move);
                    continue;
                }
                numLegalMoves++;
                int score = -NegaMax(depth - 1, plyFromRoot + 1, -beta, -alpha);

                board.UndoMove(move);

                

                if (score > maxScore)
                {

                    if (plyFromRoot == 0)
                    {
                        currentBestMove = move;
                        currentEval = score;
                    }
                    bestMove = move;
                    maxScore = score;
                    if (score > alpha)
                    {
                        alphaWasRaised = true;
                        alpha = score;
                    }
                }

                if (IsTimeUp || SearchedNodes >= NodeLimit) return 0;

                if (score >= beta)
                {
                    TT[TTIndex] = new(maxScore, depth, TTEntry.LowerBoundFlag, bestMove.Value, board.ZobristHash);
                    return maxScore;
                }
            }

            if (alphaWasRaised)
            {
                TT[TTIndex] = new(maxScore, depth, TTEntry.ExactFlag, bestMove.Value, board.ZobristHash);
            }
            else
            {
                TT[TTIndex] = new(maxScore, depth, TTEntry.UpperBoundFlag, bestMove.Value, board.ZobristHash);
            }
            if (numLegalMoves == 0)
            {
                return NegativeInf + plyFromRoot; // TODO: Update to support stalemate
            }

            return maxScore;
        }

        private int Quiecence(int alpha, int beta)
        {
            SearchedNodes++;
            int standingPat = Evaluator.EvaluateStatic(board, GamePhase);

            if (standingPat >= beta)
            {
                return standingPat;
            }

            if (alpha < standingPat)
            {
                alpha = standingPat;
            }

            Move[] captureMoves = MoveGenerator.GetPseudoLegalCaptures(board);
            moveOrderer.OrderCaptures(captureMoves, board);

            if (IsTimeUp)
            {
                return standingPat;
            }
            int bestScore = standingPat;
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
                    return score;
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    if (score > alpha)
                    {
                        alpha = score;
                    }
                }
            }

            return bestScore;
        }


        static int PhaseScore(byte piece)
        {
            return Piece.PieceType(piece) switch
            {
                Piece.Pawn => 0,
                Piece.Knight => 1,
                Piece.Bishop => 1,
                Piece.Rook => 2,
                Piece.Queen => 4,
                _ => 0
            };
        }

        public void CalculateGamePhase()
        {
            GamePhase = TotalPhase;
            foreach (byte piece in board.Squares)
            {
                GamePhase -= PhaseScore(piece);
            }
            GamePhase = ((GamePhase << 8) + (TotalPhase >> 1)) / TotalPhase;
        }

        public string GetPv()
        {
            string pv = "";
            ulong hash = board.ZobristHash;
            int depth = 0; // Prevent infinite loops
            List<Move> playedMoves = new List<Move>(); // Store played moves for undoing

            while (depth < maxDepth)
            {
                int index = (int)(hash % numTTEntries);
                TTEntry entry = TT[index];

                if (entry.ZobristHash != hash || entry.BestMove == 0 || board.IsTwofoldRepetition())
                    break; // Stop if no valid move is found in the TT

                string moveUci = BoardUtility.MoveToUci(new Move(entry.BestMove));
                pv += moveUci + " ";

                board.MakeMove(new Move(entry.BestMove)); // Play the move on the board
                playedMoves.Add(new Move(entry.BestMove)); // Store the move for undoing
                hash = board.ZobristHash; // Update hash after move
                depth++;
            }

            // Undo moves in reverse order to restore board state
            for (int i = playedMoves.Count - 1; i >= 0; i--)
            {
                board.UndoMove(playedMoves[i]);
            }

            return pv.Trim();
        }
        bool IsTimeUp => DateTime.Now.TimeOfDay.TotalMilliseconds - startTime > timeLimit;
    }
}
