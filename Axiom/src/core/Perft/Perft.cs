using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Axiom.src.core.Perft
{
    static class Perft
    {

        public const string path = @"Axiom.src.core.Perft.perftsuite.edp";

        public static void RunSuite(int maxDepth = 6)
        {

            Assembly assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(path);
            using StreamReader reader = new StreamReader(stream);

            Console.WriteLine($"Running Test with depth {maxDepth}...\n");

            string? line = reader.ReadLine();

            int num = 0;
            var total = new Stopwatch();
            total.Start();
            ulong totalPositions = 0;

            Board.Board board;

            while (line != null)
            {
                num++;
                string fen = ExtractFen(line);
                var depthValues = ExtractDepthValues(line);

                board = new(fen);
                var watch = new Stopwatch();
                watch.Start();
                Console.WriteLine($"Fen: {fen}");
                for (int i = 0; i < maxDepth; i++)
                {
                    int depth = depthValues[i].Item1;
                    ulong output = SearchNoBulk(board, depth);
                    totalPositions += output;
                    ulong expectedOutput = depthValues[i].Item2;

                    Console.WriteLine($"Depth {depth} ply  Result: {output}  Time: {watch.ElapsedMilliseconds}{(output != expectedOutput ? "  Test failed (expected " + expectedOutput : "")}{(output != expectedOutput ? ")" : "")}");
                }

                line = reader.ReadLine();
            }


            total.Stop();
            Console.WriteLine("Total ms elpased: " + total.ElapsedMilliseconds);
            Console.WriteLine("Total positions: " + totalPositions);
            Console.WriteLine("Knps: " + totalPositions / (ulong)total.ElapsedMilliseconds);
        }

        public static (int, ulong)[] ExtractDepthValues(string line)
        {
            string pattern = @";D(\d+) (\d+)";

            var depthValues = new (int, ulong)[6];

            MatchCollection matches = Regex.Matches(line, pattern);

            int index = 0;
            foreach (Match match in matches)
            {
                depthValues[index] = (int.Parse(match.Groups[1].Value), ulong.Parse(match.Groups[2].Value));
                index++;
            }

            return depthValues;
        }

        public static string ExtractFen(string line)
        {
            int semicolonIndex = line.IndexOf(';');

            if (semicolonIndex != -1)
            {
                string beforeFirstSemicolon = line.Substring(0, semicolonIndex);
                return beforeFirstSemicolon.TrimEnd();
            }
            else
            {
                // No semicolon found, use the entire string
                return line;
            }
        }

        public static void PerftSearch(string fen, int depth)
        {
            Board.Board board = new(fen);

            PerftSearch(board, depth);
        }

        public static void PerftSearch(Board.Board board, int depth)
        {

            if (depth == 0)
            {
                Console.WriteLine("Nodes searched: 1");
                return;
            }

            var watch = new Stopwatch();
            watch.Start();

            Move[] moves = MoveGenerator.GetPseudoLegalMoves(board);


            ulong totalNodes = 0;
            ulong nodes = 0;

            foreach (Move move in moves)
            {
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

                nodes = SearchNoBulk(board, depth - 1);
                board.UndoMove(move);
                totalNodes += nodes;

                Console.WriteLine($"{BoardUtility.MoveToUci(move)}: {nodes}");
            }

            watch.Stop();
            Console.WriteLine("\nNodes searched: " + totalNodes);
            Console.WriteLine($"Time: {watch.ElapsedMilliseconds}");
            Console.WriteLine($"Knps: {totalNodes / (ulong)Math.Max(1, watch.ElapsedMilliseconds)}");
        }

        public static ulong SearchNoBulk(Board.Board board, int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            Move[] moves = MoveGenerator.GetPseudoLegalMoves(board);

            ulong nodes = 0;
            foreach (Move move in moves)
            {
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
                nodes += SearchNoBulk(board, depth - 1);
                board.UndoMove(move);
            }
            return nodes;
        }
    }
}