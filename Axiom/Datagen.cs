using Axiom.src.core.Board;
using Axiom.src.core.Search;
using Axiom.src.core.Move_Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axiom.src;
using Axiom.src.core.Utility;

namespace Axiom
{
    public class Datagen
    {
        public Players[] players;
        public int numConcurrentGames;

        public const int SOFT_NODE_LIMIT = 5000;
        public const int HARD_NODE_LIMIT = 5000;

        public Datagen(int threads)
        {
            players = new Players[threads];
            for(int i = 0; i < threads; i++)
            {
                players[i] = new(SOFT_NODE_LIMIT, HARD_NODE_LIMIT);
            }
            numConcurrentGames = threads;
        }

        public void Run(string outputPath)
        {
            using StreamWriter writer = new StreamWriter(outputPath);

            for(int i = 0; i < numConcurrentGames; i++)
            {
                string[] game = players[i].PlayGame(GetRandomStartpos());

                foreach(string fen in game)
                {
                    writer.WriteLine(fen);
                }
                
            }
        }

        public static string GetRandomStartpos()
        {
            Board board = new();
            Random r = new Random();
            
            // Generate a random position after n ply
            for (int i = 0; i < (r.Next(2) == 1 ? 10 : 9);)
            {
                Move[] moves = MoveGenerator.GetPseudoLegalMoves(board);
                Move randMove = moves[r.Next(moves.Length)];

                // Filter illegal castling moves
                if (randMove.MoveFlag == Move.CastleFlag)
                {
                    if (board.IsInCheck(board.WhiteToMove))
                    {
                        continue;
                    }
                    switch (randMove.TargetSquare)
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

                board.MakeMove(randMove);

                // Filter illegal moves
                if (board.IsInCheck(!board.WhiteToMove))
                {
                    board.UndoMove(randMove);
                    continue;
                }
                i++;
            }

            return board.Fen;
        }
    }

    public class Players
    {
        private Engine engine;
        int softNodes;
        int hardNodes;

        public Players(int softNodes, int hardNodes)
        {
            engine = new()
            {
                printInfo = false
            };
            this.softNodes = softNodes;
            this.hardNodes = hardNodes;
        }

        public string[] PlayGame(string startFen)
        {
            string[] fens = new string[5899 * 2];
            string gameResult;
            int gameLength = 0;
            engine.SetPosition(startFen);
            while (true)
            {
                engine.Search(255, int.MaxValue, hardNodes, softNodes);
                fens[gameLength++] = engine.board.Fen + " | " + UCI.GetCorrectEval(engine.eval * (engine.board.WhiteToMove ? 1 : -1)) + " | " + BoardUtility.MoveToUci(engine.bestMoveThisIteration) + " | ";

                engine.board.MakeMove(engine.bestMoveThisIteration);

                if (IsGameOver(engine.board))
                {
                    gameResult = engine.board.IsInCheck(engine.board.WhiteToMove) ? engine.board.WhiteToMove ? "0" : "1" : "0.5";
                    break;
                }
                else if (engine.board.IsThreefoldRepetition())
                {
                    gameResult = "0.5";
                    break;
                }
            }

            // Slice off
            string[] result = new string[gameLength];
            Array.Copy(fens, result, gameLength);

            for(int i = 0; i < gameLength; i++)
            {
                result[i] += gameResult;
            }

            return result;
        }

        public bool IsGameOver(Board b)
        {
            Move[] moves = MoveGenerator.GetPseudoLegalMoves(b);

            foreach(Move move in moves)
            {
                // Filter illegal castling moves
                if (move.MoveFlag == Move.CastleFlag)
                {
                    if (b.IsInCheck(b.WhiteToMove))
                    {
                        continue;
                    }
                    switch (move.TargetSquare)
                    {
                        case 62: // white short castle (g1)
                            if (b.IsUnderAttack(61, true) || b.IsUnderAttack(62, true)) { continue; }
                            break;
                        case 58: // white loing castle (c1)
                            if (b.IsUnderAttack(58, true) || b.IsUnderAttack(59, true)) { continue; }
                            break;
                        case 6: // black short castle (g8)
                            if (b.IsUnderAttack(5, false) || b.IsUnderAttack(6, false)) { continue; }
                            break;
                        case 2: // black long castle (c8)
                            if (b.IsUnderAttack(2, false) || b.IsUnderAttack(3, false)) { continue; }
                            break;
                    }
                }

                b.MakeMove(move);

                // Filter illegal moves
                if (b.IsInCheck(!b.WhiteToMove))
                {
                    b.UndoMove(move);
                    continue;
                }
                b.UndoMove(move);
                return false;
            }
            return true;
        }
    }
}
