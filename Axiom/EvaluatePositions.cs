using System;
using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Axiom.src.core.Search;

namespace Axiom
{
    static class EvaluatePositions
    {
        public static void Evaluate()
        {
            string edpFilePath = @"C:\c\data.edp";
            string outputFilePath = @"C:\c\evaluations.edp";
            int nodeLimit = 50000;
            int maxThreads = 10; // Avoid full CPU saturation

            string[] positions = File.ReadAllLines(edpFilePath);
            int totalPositions = positions.Length;
            ConcurrentQueue<string> positionQueue = new ConcurrentQueue<string>(positions);

            Engine[] engines = new Engine[maxThreads];
            for (int i = 0; i < maxThreads; i++)
            {
                engines[i] = new Engine();
                engines[i].printInfo = false;
            }

            int evaluatedCount = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            object fileLock = new object();
            ConcurrentQueue<string> resultQueue = new ConcurrentQueue<string>();

            // Worker tasks
            Task[] workers = new Task[maxThreads];
            for (int threadId = 0; threadId < maxThreads; threadId++)
            {
                int localThreadId = threadId; // Avoid closure issue
                workers[threadId] = Task.Run(() =>
                {
                    Engine engine = engines[localThreadId];
                    List<string> localResults = new List<string>();

                    while (positionQueue.TryDequeue(out string position))
                    {
                        engine.SetPosition(position);
                        engine.Search(256, int.MaxValue, nodeLimit);
                        int evaluation = engine.eval;

                        localResults.Add($"{position},{evaluation}");

                        int currentCount = Interlocked.Increment(ref evaluatedCount);

                        if (currentCount % 500 == 0) // Update progress
                        {
                            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                            double pps = elapsedSeconds > 0 ? currentCount / elapsedSeconds : 0;
                            double progress = (currentCount / (double)totalPositions) * 100;

                            Console.WriteLine($"Progress: {progress:F2}% | Evaluated: {currentCount}/{totalPositions} | PPS: {pps:F2}");
                        }

                        // Push results to file writer queue
                        if (localResults.Count >= 100)
                        {
                            foreach (var result in localResults)
                                resultQueue.Enqueue(result);
                            localResults.Clear();
                        }
                    }

                    // Push remaining results
                    foreach (var result in localResults)
                        resultQueue.Enqueue(result);
                });
            }

            // Writer task (prevents evaluation threads from waiting)
            Task writerTask = Task.Run(() =>
            {
                using (StreamWriter writer = new StreamWriter(outputFilePath, append: true))
                {
                    while (!workers.All(t => t.IsCompleted) || !resultQueue.IsEmpty)
                    {
                        if (resultQueue.TryDequeue(out string result))
                        {
                            writer.WriteLine(result);
                        }
                        else
                        {
                            Thread.Sleep(100); // Reduce CPU usage
                        }
                    }
                }
            });

            Task.WaitAll(workers);
            writerTask.Wait();
            stopwatch.Stop();

            Console.WriteLine($"Evaluation completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
            Console.WriteLine($"Final PPS: {(evaluatedCount / stopwatch.Elapsed.TotalSeconds):F2}");
        }
    }
}
