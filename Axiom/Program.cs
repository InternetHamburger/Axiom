using Axiom.src;
using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Perft;
using Nerual_Network.Chess;
using Nerual_Network.Setup;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Numerics;

namespace Axiom
{
    internal class Program
    {
        static void Main()
        {
            UCI uci = new();

            string? message = Console.ReadLine();

            while (message != "quit")
            {
                if (message == null)
                {
                    Console.WriteLine("Unknown command");
                    continue;
                }
                if (message.Split(' ')[0] == "datagen")
                {
                    string[] tokens = message.Split(" ");
                    if (tokens.Length == 1)
                    {
                        Console.WriteLine("The format is: datagen threads {threads} output {outputPath}");
                    }
                    else
                    {
                        Datagen datagen = new(int.Parse(tokens[2]));
                        datagen.Run(tokens[4]);
                    }
                }
                else
                {
                    uci.ReciveCommand(message);
                }
                message = Console.ReadLine();
            }
        }
    }
}
