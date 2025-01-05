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

            switch(messageType)
            {
                case "uci":
                    RespondUCI();
                    break;
            }


            
        }
        private void RespondUCI()
        {
            Console.WriteLine("id name Axiom");
            Console.WriteLine("uciok");
        }
    }
}
