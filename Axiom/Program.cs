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
