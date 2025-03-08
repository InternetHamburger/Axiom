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

        const int sizeTTMb = 32;
        const int sizeTTEntry = 16;
        const ulong numTTEntries = sizeTTMb * 1024 * 1024 / sizeTTEntry;

        public int GamePhase;
        public const int TotalPhase = 24;

        public int SearchedNodes;
        public int betaCutoffs;
        public int firstMoveBetaCutoffs;
        public Move bestMoveThisIteration;
        public Move currentBestMove;
        public int eval;
        public int currentEval;
        public int maxDepth;

        public double startTime;
        public double timeLimit;

        public TTEntry[] TT;
        public MoveOrderer moveOrderer;


        public Board.Board board;

        public Engine()
        {
            board = new Board.Board();
            TT = new TTEntry[numTTEntries];
            moveOrderer = new();

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

            const int delta = 7;


            var watch = new Stopwatch();
            watch.Start();
            for (int depth = 1; depth <= depthlimit; depth++)
            {
                alpha = eval - delta;
                beta = eval + delta;

                // Initial call
                // Very tight bounds, but pays off
                NegaMax(depth, 0, alpha, beta);
                maxDepth = depth;
                int numReSearches = 0;

                while (currentEval >= beta || currentEval <= alpha)
                {
                    if (numReSearches == 7)
                    {
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
                        break;
                    }
                    if (currentEval >= beta)
                    {
                        alpha = eval - delta;
                        beta += 3 * delta;
                        NegaMax(depth, 0, alpha, beta);
                    }
                    else if (currentEval <= alpha)
                    {
                        beta = eval + delta;
                        alpha -= 3 * delta;
                        NegaMax(depth, 0, alpha, beta);
                    }
                    numReSearches++;
                }


                if (IsTimeUp)
                {
                    return;
                }
                bestMoveThisIteration = currentBestMove;
                eval = currentEval;
                Console.WriteLine($"info depth {depth} score {UCI.GetCorrectEval(eval)} nodes {SearchedNodes} nps {SearchedNodes / Math.Max(1, watch.ElapsedMilliseconds) * 1000} time {watch.ElapsedMilliseconds} pv {GetPv()}");
            }
            watch.Stop();
        }

        private void InitSearch()
        {
            TT = new TTEntry[numTTEntries];
            moveOrderer.Init();
            SearchedNodes = 0;
            startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            currentBestMove = Move.NullMove;
            currentEval = NegativeInf;
            CalculateGamePhase();
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
            moveOrderer.OrderMoves(pseudoLegalMoves, board, new Move(ttEntry.BestMove), plyFromRoot);


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
                int score = 0;
                int extension = board.IsInCheck(board.WhiteToMove) ? 1 : 0;
                if (i == 0)
                {
                    score = -NegaMax(depth - 1 + extension, plyFromRoot + 1, -beta, -alpha);
                }
                else
                {
                    score = -NegaMax(depth - 1 + extension, plyFromRoot + 1, -alpha - 1, -alpha);
                    if (score > alpha)
                    {
                        score = -NegaMax(depth - 1 + extension, plyFromRoot + 1, -beta, -alpha);
                    }
                }





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
                        moveOrderer.UpdateHistoryTableAlphaRaise(board, move, depth);
                        alphaWasRaised = true;
                        alpha = score;
                    }
                }

                if (beta <= alpha)
                {
                    if (i == 0)
                    {
                        betaCutoffs++;
                        firstMoveBetaCutoffs++;
                    }
                    else
                    {
                        betaCutoffs++;
                    }
                    if (board.Squares[move.TargetSquare] == 0) // Is a quiet move
                    {
                        moveOrderer.UpdateHistoryTableBetaCutoff(board, move, depth);
                    }
                    else
                    {
                        moveOrderer.KillerMoves[plyFromRoot] = move;
                    }
                    TT[TTIndex] = new(bestScore, depth, TTEntry.LowerBoundFlag, bestMove.Value, board.ZobristHash);
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
                TT[TTIndex] = new(bestScore, depth, TTEntry.UpperBoundFlag, bestMove.Value, board.ZobristHash);
            }
            return bestScore;
        }

        private int Quiecence(int alpha, int beta)
        {
            SearchedNodes++;
            int standingPat = Evaluator.Evaluate(board, GamePhase);

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
                if (score > alpha)
                {
                    alpha = score;
                }
            }

            return alpha;
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

                if (entry.ZobristHash != hash || entry.BestMove == 0)
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
