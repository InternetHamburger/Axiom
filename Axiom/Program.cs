using Axiom.src;

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
