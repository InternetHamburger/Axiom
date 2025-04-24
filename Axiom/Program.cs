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
                        _ = datagen.Run(tokens[4]);
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
