using Axiom.src.core.Search;
using Axiom.src.core.Utility;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Axiom.src.core.Perft
{
    internal static class Bench
    {
        public const string path = @"Axiom.src.core.Perft.perftsuite.edp";

        public static void RunSuite(int maxDepth = 10)
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

            Engine engine = new()
            {
                printInfo = false
            };
            while (line != null)
            {
                num++;
                string fen = ExtractFen(line);

                engine.board = new(fen);
                var watch = new Stopwatch();
                watch.Start();
                Console.WriteLine($"Fen: {fen}");
                for (int i = 1; i <= maxDepth; i++)
                {
                    engine.Search(i);
                    if (i == maxDepth)
                    {
                        totalPositions += (ulong)engine.SearchedNodes;
                    }


                    Console.WriteLine($"Depth {i} ply  Result: {engine.SearchedNodes}  Time: {watch.ElapsedMilliseconds} bestmove: {BoardUtility.MoveToUci(engine.bestMoveThisIteration)}");
                }

                line = reader.ReadLine();
            }


            total.Stop();
            Console.WriteLine("Total ms elpased: " + total.ElapsedMilliseconds);
            Console.WriteLine("Total nodes: " + totalPositions);
            Console.WriteLine("Total positions: " + num);
            Console.WriteLine("Knps: " + (totalPositions / (ulong)total.ElapsedMilliseconds));


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

    }
}
