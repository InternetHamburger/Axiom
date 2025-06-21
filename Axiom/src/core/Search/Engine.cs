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

        public int sizeTTMb = 32;
        const int sizeTTEntry = 16;
        ulong numTTEntries;

        public static int MaxPly = 255;

        public int GamePhase;
        public const int TotalPhase = 24;

        public int SearchedNodes;
        public Move bestMoveThisIteration;
        public Move currentBestMove;
        public int eval;
        public int currentEval;
        public int maxDepth;
        public bool printInfo;
        public bool ClearTTBetweenSearches;

        public double startTime;
        public double timeLimit;
        public int NodeLimit;

        public TTEntry[] TT;
        public MoveOrderer moveOrderer;


        public Board.Board board;

        public Engine()
        {
            sizeTTMb = 32;
            numTTEntries = (ulong)sizeTTMb * 1024 * 1024 / sizeTTEntry;

            board = new Board.Board();
            TT = new TTEntry[numTTEntries];
            moveOrderer = new();
            printInfo = true;
            ClearTTBetweenSearches = false;
            //NegaMax(1, 0, NegativeInf, PositiveInf);
        }

        public void SetPosition(string fen)
        {
            board = new Board.Board(fen);
            InitSearch();
        }

        public void SetTTSize(int mb)
        {
            sizeTTMb = mb;
            numTTEntries = (ulong)sizeTTMb * 1024 * 1024 / sizeTTEntry;

            TT = new TTEntry[numTTEntries];
        }

        public void Search(int depthlimit, int timelimit = int.MaxValue, int hardNodeLimit = int.MaxValue, int softNodeLimit = int.MaxValue)
        {
            if (softNodeLimit == int.MaxValue) softNodeLimit = hardNodeLimit;

            InitSearch();
            timeLimit = timelimit;
            NodeLimit = hardNodeLimit;


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
                NegaMax(depth, 0, alpha, beta, false);
                maxDepth = depth;
                int numReSearches = 0;

                while (currentEval >= beta || currentEval <= alpha)
                {
                    if (numReSearches == 7)
                    {
                        if (currentEval >= beta)
                        {
                            alpha = eval - delta;
                            NegaMax(depth, 0, alpha, PositiveInf, false);
                        }
                        else if (currentEval <= alpha)
                        {
                            beta = eval + delta;
                            NegaMax(depth, 0, NegativeInf, beta, false);
                        }
                        break;
                    }
                    if (currentEval >= beta)
                    {
                        alpha = eval - delta;
                        beta += 3 * delta;
                        NegaMax(depth, 0, alpha, beta, false);
                    }
                    else if (currentEval <= alpha)
                    {
                        beta = eval + delta;
                        alpha -= 3 * delta;
                        NegaMax(depth, 0, alpha, beta, false);
                    }
                    numReSearches++;
                }

                if ((IsTimeUp || SearchedNodes >= softNodeLimit) && depth >= 2)
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
            
            //TT = new TTEntry[numTTEntries];
            //for (ulong i = 0; i < numTTEntries; i++)
            //{
            //    TT[i] = new();
            //}
            if (ClearTTBetweenSearches)
            {
                Array.Clear(TT, 0, TT.Length);
            }
            moveOrderer.Init();
            SearchedNodes = 0;
            startTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            currentBestMove = Move.NullMove;
            currentEval = NegativeInf;
            CalculateGamePhase();
        }


        private int NegaMax(int depth, int plyFromRoot, int alpha, int beta, bool cutnode)
        {
            bool IsPvNode = (beta - alpha) > 1;
            ulong TTIndex = board.ZobristHash % numTTEntries;
            TTEntry ttEntry = TT[TTIndex];
            bool ttHit = ttEntry.ZobristHash == board.ZobristHash;

            if (ttEntry.Depth >= depth && plyFromRoot > 0 && ttHit && !IsPvNode)
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
                    return ttEntry.Score;
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
                if (board.IsTwofoldRepetition())
                {
                    return 0;
                }
            }

            if (plyFromRoot > 0 && depth >= 3 && (IsPvNode || cutnode) && (!ttHit || ttEntry.BestMove == 0))
            {
                depth--;
            }

            bool InCheck = board.IsInCheck(board.WhiteToMove);
            int staticEval = board.Eval;
            int margin = 150 * depth; // e.g. 150 * depth
            if (plyFromRoot > 0 && !InCheck && ttEntry.BestMove == 0 && staticEval >= beta + margin)
            {   
                return staticEval; // fail soft
            }
            // Null Move Pruning
            const int NULL_MOVE_MIN_DEPTH = 3;
            int R = depth > 5 ? 3 : 2; // Reduction factor for null move pruning

            if (depth >= NULL_MOVE_MIN_DEPTH && !InCheck && !board.InEndgame(GamePhase) && !IsPvNode && staticEval >= beta)
            {
                board.MakeNullMove();
                int nullMoveScore = -NegaMax(depth - 1 - R, plyFromRoot + 1, -beta, -beta + 1, !cutnode);
                board.UndoNullMove();

                if (nullMoveScore >= beta)
                {
                    return nullMoveScore; // Fail-high cutoff
                }
            }

            Move[] pseudoLegalMoves = MoveGenerator.GetPseudoLegalMoves(board);
            moveOrderer.OrderMoves(pseudoLegalMoves, board, new Move(ttEntry.BestMove), plyFromRoot);

            List<Move> quietMoves = [];

            int numLegalMoves = 0;
            int bestScore = NegativeInf;
            bool alphaWasRaised = false;
            bool skipQuiets = false;
            Move bestMove = Move.NullMove;
            for (int i = 0; i < pseudoLegalMoves.Length; i++)
            {
                Move move = pseudoLegalMoves[i];

                bool isCapture = board.Squares[move.TargetSquare] != 0;
                if (skipQuiets && !isCapture && !move.IsPromotion) continue;

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

                if (!isCapture) quietMoves.Add(move);

                numLegalMoves++;


                int futilitymargin = 200 * depth; // Dynamic futility margin

                // Futility pruning: skip moves unlikely to raise alpha
                if (depth <= 3 && staticEval + futilitymargin <= alpha && !isCapture && !InCheck && i > 1 && !move.IsPromotion && plyFromRoot > 0)
                {
                    board.UndoMove(move);
                    skipQuiets = true;
                    continue;
                }

                if (!isCapture && !move.IsPromotion && i >= 5 + 2 * depth * depth && bestScore > (NegativeInf + 255) && !IsPvNode)
                {
                    board.UndoMove(move);
                    skipQuiets = true;
                    continue;
                }

                int score;
                int extension = board.IsInCheck(board.WhiteToMove) ? 1 : 0;
                if (i == 0)
                {
                    score = -NegaMax(depth - 1 + extension, plyFromRoot + 1, -beta, -alpha, IsPvNode ? false : !cutnode);
                }
                else
                {
                    if (depth >= 2 && i >= 1)
                    {
                        int reduction = SearchConstants.LMR_TABLE[Math.Min(depth, MaxPly), Math.Min(i, MoveGenerator.MaxNumMoves)];
                        if (i >= 4 && depth >= 3 && !isCapture && !board.IsInCheck(board.WhiteToMove))
                            reduction++;


                        score = -NegaMax(depth - 1 - reduction + extension, plyFromRoot + 1, -alpha - 1, -alpha, true);
                        if (score > alpha && (IsPvNode || reduction > 0))
                        {
                            score = -NegaMax(depth - 1 + extension, plyFromRoot + 1, -beta, -alpha, !cutnode);
                        }
                    }
                    else
                    {
                        score = -NegaMax(depth - 1 + extension, plyFromRoot + 1, -beta, -alpha, !cutnode);
                    }
                    
                }

                board.UndoMove(move);

                if (IsTimeUp || SearchedNodes >= NodeLimit)
                {
                    return 0;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    
                    if (plyFromRoot == 0)
                    {
                        currentBestMove = move;
                        currentEval = score;
                    }
                    if (score > alpha)
                    {
                        bestMove = move;
                        alphaWasRaised = true;
                        alpha = score;
                    }
                }

                if (beta <= alpha)
                {
                    if (board.Squares[move.TargetSquare] == 0) // Is a quiet move
                    {
                        moveOrderer.UpdateHistoryTableBetaCutoff(board, move, depth);
                        foreach (Move qMove in quietMoves)
                        {
                            if (Move.SameMove(qMove, move)) continue;
                            moveOrderer.UpdateHistoryTableBadQuiet(board, qMove, depth);
                        }
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
                TT[TTIndex] = new(bestScore, depth, TTEntry.UpperBoundFlag, ttEntry.BestMove, board.ZobristHash);
            }
            return bestScore;
        }

        private int Quiecence(int alpha, int beta)
        {
            SearchedNodes++;
            int standingPat = board.Eval;

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

                // Skip bad captures
                if (!StaticExchangeEvaluation.SEE(board, move, 0))
                {
                    continue;
                }

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
