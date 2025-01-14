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
            string fen = tokens[1] + tokens[2] + tokens[3] + tokens[4] + tokens[5] + tokens[6];

        }
    }
}
