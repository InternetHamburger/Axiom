using Axiom.src;
using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Perft;
using Nerual_Network.Chess;
using Nerual_Network.Setup;
using System.Numerics;

namespace Axiom
{
    internal class Program
    {
        static void Main()
        {
            //Board b = new();
            //Console.WriteLine();
            //int[] stmAccumulator = new int[b.nn.hlSize];
            //int[] nstmAccumulator = new int[b.nn.hlSize];
            //for (int i = 0; i < b.nn.hlSize; i++)
            //{
            //    stmAccumulator[i] = b.nn.StmAccumulator[i];
            //    nstmAccumulator[i] = b.nn.NstmAccumulator[i];
            //}
            //b.nn.AddFeature(Piece.WhitePawn, 52);
            //b.nn.RemoveFeature(Piece.WhitePawn, 52);

            ////Move move = new(52, 52-16);
            ////b.MakeMove(move);
            ////b.UndoMove(move);
            //for (int i = 0; i < b.nn.hlSize; i++)
            //{
            //    if (stmAccumulator[i] != b.nn.StmAccumulator[i])
            //    {
            //        Console.WriteLine(i);
            //    }
            //}

            UCI uci = new();

            string? message = Console.ReadLine();

            while (message != "quit")
            {
                if (message == null)
                {
                    Console.WriteLine("Unknown command");
                    continue;
                }

                uci.ReciveCommand(message);
                message = Console.ReadLine();
            }
        }
    }
}
