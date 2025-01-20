using Axiom.src.core.Board;
using Axiom.src.core.Perft;
using Axiom.src.core.Utility;

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
            string fen = tokens[1] == "startpos" ? BoardUtility.StartPos : "";

            if (tokens[1] != "startpos")
            {
                for (int i = 2; i < Math.Min(7, tokens.Length - 1); i++)
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
            }
            
            board.SetPosition(fen);

            int movesPos = Array.IndexOf(tokens, "moves");

            if (movesPos != -1)
            {
                for (int i = movesPos + 1; i < tokens.Length; i++)
                {
                    string uciMove = tokens[i];
                    Move move = ReturnMove(board, uciMove);

                    board.MakeMove(move);
                }
            }
        }

        private void HandleGoCommand(string input)
        {
            int depth = int.Parse(input.Split(' ')[2]);

            Perft.PerftSearch(board, depth);
        }

        public static Move ReturnMove(Board board, string move)
        {
            
            int startSquare = BoardUtility.NameOfSquare(move.Substring(0, 2));
            int targetSquare = BoardUtility.NameOfSquare(move.Substring(2, 2));
            int promotionRank = board.WhiteToMove ? 1 : 6; // Rank for promotion
            byte piece = board.Squares[startSquare];

            int MoveFlag = Move.NoFlag;


            if (Piece.PieceType(piece) == Piece.Pawn)
            {
                if (BoardUtility.File(startSquare) != BoardUtility.File(targetSquare))
                {
                    if (board.Squares[targetSquare] == 0)
                    {
                        MoveFlag = Move.EnPassantCaptureFlag;
                    }
                }
                if (Math.Abs(BoardUtility.Rank(startSquare) - BoardUtility.Rank(targetSquare)) == 2)
                {
                    MoveFlag = Move.PawnTwoUpFlag;
                }
                if (BoardUtility.Rank(startSquare) == promotionRank)
                {
                    byte? promotionPieceType = Piece.GetType(move[4]) ?? throw new NotImplementedException();
                    promotionPieceType = (byte)promotionPieceType;

                    byte pievce = promotionPieceType.Value;

                    switch (Piece.PieceType(pievce))
                    {
                        case Piece.Queen:
                            MoveFlag = Move.PromoteToQueenFlag;
                            break;
                        case Piece.Rook:
                            MoveFlag = Move.PromoteToRookFlag;
                            break;
                        case Piece.Knight:
                            MoveFlag = Move.PromoteToKnightFlag;
                            break;
                        case Piece.Bishop:
                            MoveFlag = Move.PromoteToBishopFlag;
                            break;

                    }
                }

            }
            else if (Piece.PieceType(piece) == Piece.King)
            {
                if ((startSquare == 60 && (targetSquare == 62 || targetSquare == 58)) ||
                    (startSquare == 4 && (targetSquare == 6 || targetSquare == 2)))
                {
                    MoveFlag = Move.CastleFlag;
                }

            }

            return new Move(startSquare, targetSquare, MoveFlag);

        }
    }
}
