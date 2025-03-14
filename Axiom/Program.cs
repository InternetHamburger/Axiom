using Axiom.src;
using Axiom.src.core.Board;
using Nerual_Network.Chess;
using Nerual_Network.Setup;

namespace Axiom
{
    internal class Program
    {
        static void Main()
        {
            NeuralNetwork nn = new(768, 32, 1);
            nn.LoadFromFile("C:/c/nn.json");

            double[] input = FenUtlity.FenToArray("r1bqk2r/pppp1ppp/5n2/8/1b1nP3/2N5/PPP2PPP/R1B1KB1R");

            Console.WriteLine(nn.GetOutput(input));

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
