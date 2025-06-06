﻿using Axiom.src.core.Board;
using Axiom.src.core.Search;
using Axiom.src.core.Move_Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axiom.src;
using Axiom.src.core.Utility;
using System.Diagnostics;

namespace Axiom
{
    public class Datagen
    {
        public Players[] players;
        public int numConcurrentGames;

        public const int SOFT_NODE_LIMIT = 5000;
        public const int HARD_NODE_LIMIT = 10000;

        private static readonly object fileLock = new object();

        public Datagen(int threads)
        {
            players = new Players[threads];
            for (int i = 0; i < threads; i++)
            {
                players[i] = new(SOFT_NODE_LIMIT, HARD_NODE_LIMIT);
            }
            numConcurrentGames = threads;
        }

        public async Task Run(string outputPath)
        {
            Console.WriteLine("Running datagen with " + numConcurrentGames + " threads and output to " + outputPath);

            using StreamWriter writer = new(outputPath, append: true);

            int globalPositions = 0;
            int globalGames = 0;

            // start N infinite‑loop tasks
            Task[] gameTasks = new Task[numConcurrentGames];
            for (int i = 0; i < numConcurrentGames; i++)
            {
                int index = i; // capture
                gameTasks[i] = Task.Run(() =>
                {
                    var totalClock = new Stopwatch();
                    var localClock = new Stopwatch();
                    localClock.Start();
                    totalClock.Start();

                    int localPositions = 0;

                    List<string[]> games = [];

                    while (true)
                    {
                        string[] game = players[index].PlayGame(GetRandomStartpos());
                        games.Add(game);
                        

                        if (games.Count % 10 == 0)
                        {
                            lock (fileLock)
                            {
                                globalGames += games.Count;
                                foreach (string[] g in games)
                                {
                                    localPositions += g.Length;
                                    globalPositions += g.Length;
                                    foreach (string fen in g)
                                    {
                                        writer.WriteLine(fen);
                                    }
                                }

                                // only flush occasionally if you want

                                writer.Flush();
                                Console.WriteLine("------------------");
                                Console.WriteLine($"Total games          |  {globalGames}");
                                Console.WriteLine($"Total positions      |  {globalPositions}");
                                Console.WriteLine($"Time elapsed         |  {totalClock.Elapsed.TotalSeconds:F1}s");
                                Console.WriteLine($"Avg positions/sec    |  {Math.Round(globalPositions / totalClock.Elapsed.TotalSeconds)}");
                                Console.WriteLine($"Local time           |  {localClock.Elapsed.TotalSeconds:F1}s");
                                Console.WriteLine($"Local positions/sec  |  {Math.Round(localPositions / localClock.Elapsed.TotalSeconds)}");
                                Console.WriteLine("------------------\n");
                                localPositions = 0;
                                localClock.Stop();
                                localClock.Restart();
                                localClock.Start();
                                games = [];
                            }
                        }

                    }
                });
            }

            await Task.WhenAll(gameTasks);
        }

        public static string GetRandomStartpos()
        {
            Board board = new();
            Random r = new();

            int targetPlies = r.Next(2) == 1 ? 10 : 9;

            for (int i = 0; i < targetPlies;)
            {
                Move[] pseudoMoves = MoveGenerator.GetPseudoLegalMoves(board);
                List<Move> legalMoves = [];

                foreach (var move in pseudoMoves)
                {
                    if (move.MoveFlag == Move.CastleFlag)
                    {
                        if (board.IsInCheck(board.WhiteToMove))
                            continue;

                        switch (move.TargetSquare)
                        {
                            case 62: // white short castle (g1)
                                if (board.IsUnderAttack(61, true) || board.IsUnderAttack(62, true)) continue;
                                break;
                            case 58: // white long castle (c1)
                                if (board.IsUnderAttack(58, true) || board.IsUnderAttack(59, true)) continue;
                                break;
                            case 6: // black short castle (g8)
                                if (board.IsUnderAttack(5, false) || board.IsUnderAttack(6, false)) continue;
                                break;
                            case 2: // black long castle (c8)
                                if (board.IsUnderAttack(2, false) || board.IsUnderAttack(3, false)) continue;
                                break;
                        }
                    }

                    board.MakeMove(move);

                    if (!board.IsInCheck(!board.WhiteToMove)) // move is legal
                    {
                        legalMoves.Add(move);
                    }

                    board.UndoMove(move);
                }

                // If no legal moves, we hit checkmate or stalemate
                if (legalMoves.Count == 0)
                {
                    break;
                }

                // Pick a random legal move
                Move chosenMove = legalMoves[r.Next(legalMoves.Count)];
                board.MakeMove(chosenMove);
                i++;
            }

            return board.Fen;
        }

    }

    public class Players
    {
        private readonly Engine engine;
        readonly int softNodes;
        readonly int hardNodes;

        public Players(int softNodes, int hardNodes)
        {
            engine = new()
            {
                printInfo = false,
                sizeTTMb = 1,
                ClearTTBetweenSearches = false
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

                engine.Search(255, int.MaxValue, hardNodes, softNodes);
                fens[gameLength++] = engine.board.Fen + " | " + UCI.GetCorrectEval(engine.eval * (engine.board.WhiteToMove ? 1 : -1)) + " | " + BoardUtility.MoveToUci(engine.bestMoveThisIteration) + " | ";

                engine.board.MakeMove(engine.bestMoveThisIteration);


            }

            // Slice off
            string[] result = new string[gameLength];
            Array.Copy(fens, result, gameLength);

            for (int i = 0; i < gameLength; i++)
            {
                result[i] += gameResult;
            }

            return result;
        }

        public bool IsGameOver(Board b)
        {
            Move[] moves = MoveGenerator.GetPseudoLegalMoves(b);

            foreach (Move move in moves)
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
