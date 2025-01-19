using Axiom.src.core.Board;
using Axiom.src.core.Perft;
using Axiom.src.core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src
{
    public class UCI
    {

        private Board board;

        public UCI()
        {
            board = new Board();
        }

        public void ReciveCommand(string input)
        {
            input = input.Trim();

            string messageType = input.Split(' ')[0].ToLower();

            switch (messageType)
            {
                case "uci":
                    RespondUCI();
                    break;
                case "position":
                    HandlePositionCommand(input);
                    break;
                case "go":
                    HandleGoCommand(input);
                    break;
                case "d":
                    Console.WriteLine();
                    BoardUtility.PrintBoard(board);
                    Console.WriteLine("\nFen: " + FenUtility.GetFen(board));

                    break;
                default:
                    Console.WriteLine("Unknown message");
                    break;
            }
        }
        private void RespondUCI()
        {
            Console.WriteLine("id name Axiom");
            Console.WriteLine("uciok");
        }

        private void HandlePositionCommand(string input)
        {
            string[] tokens = input.Split(' ');
            string fen = "";

            for (int i = 2; i < 7; i++)
            {
                if (tokens[i] == "moves")
                {
                    break;
                }
                else
                {
                    fen += tokens[i] + " ";
                }
            }

            board.SetPosition(fen);
        }

        private void HandleGoCommand(string input)
        {
            int depth = int.Parse(input.Split(' ')[2]);

            Perft.PerftSearch(board, depth);
        }
    }
}
