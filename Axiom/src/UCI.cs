using Axiom.src.core.Board;
using Axiom.src.core.Evaluation;
using Axiom.src.core.Perft;
using Axiom.src.core.Search;
using Axiom.src.core.Utility;
using Nerual_Network.Chess;
using Nerual_Network.Setup;
using System.Collections.Immutable;

namespace Axiom.src
{
    public class UCI
    {

        private Engine engine;

        public UCI()
        {
            engine = new Engine();
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
                case "isready":
                    Console.WriteLine("readyok");
                    break;
                case "position":
                    HandlePositionCommand(input);
                    break;
                case "go":
                    HandleGoCommand(input);
                    break;
                case "d":
                    Console.WriteLine();
                    BoardUtility.PrintBoard(engine.board);
                    Console.WriteLine("\nFen: " + engine.board.Fen);
                    Console.WriteLine("Hash: " + engine.board.ZobristHash);
                    break;
                case "r":
                    Console.WriteLine(BoardUtility.FlipSquare(63));
                    break;
                case "eval":
                    engine.CalculateGamePhase();
                    Console.WriteLine(Evaluator.Evaluate(engine.board, engine.GamePhase) * (engine.board.WhiteToMove ? 1 : -1));
                    break;
                case "nneval":
                    Console.WriteLine(engine.evaluator.EvaluateNN(engine.board));
                    break;
                case "bench":
                    string[] tokens = input.Split(' ');
                    if (tokens.Length > 1 && int.TryParse(tokens[1], out int depth))
                    {
                        Bench.RunSuite(depth);
                    }
                    else
                    {
                        Bench.RunSuite();
                    }
                    break;
                default:
                    Console.WriteLine("Unknown message");
                    break;
            }
        }
        private static void RespondUCI()
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
                for (int i = 2; i < Math.Min(7, tokens.Length + 1); i++)
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
            engine.SetPosition(fen);

            int movesPos = Array.IndexOf(tokens, "moves");

            if (movesPos != -1)
            {
                for (int i = movesPos + 1; i < tokens.Length; i++)
                {
                    string uciMove = tokens[i];
                    Move move = ReturnMove(engine.board, uciMove);
                    engine.board.MakeMove(move);
                }
            }
        }

        private void HandleGoCommand(string input)
        {
            int depth;
            string[] tokens = input.Split(' ');
            string type = tokens[1];

            switch (type)
            {
                case "perft":
                    depth = int.Parse(input.Split(' ')[2]);

                    Perft.PerftSearch(engine.board, depth);
                    break;
                case "nodes":
                    int nodes = int.Parse(input.Split(' ')[2]);

                    engine.Search(256, int.MaxValue, nodes);
                    Console.WriteLine("bestmove " + BoardUtility.MoveToUci(engine.bestMoveThisIteration));
                    break;
                default:
                    int searchTime = ExtractMoveTime(tokens);
                    depth = 256;
                    if (tokens.Contains("depth"))
                    {
                        depth = int.Parse(tokens[Array.IndexOf(tokens, "depth") + 1]);
                    }


                    engine.Search(depth, searchTime);

                    Console.WriteLine("bestmove " + BoardUtility.MoveToUci(engine.bestMoveThisIteration));
                    break;
            }
        }

        public static int GetSearchTime(int timeLeftMs, int incrementMs)
        {
            return timeLeftMs / 60 + incrementMs / 2;
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

        private int ExtractMoveTime(string[] tokens)
        {
            int timeLeft = int.MaxValue;
            int increment = 0;
            if (tokens.Contains("movetime"))
            {
                return int.Parse(tokens[Array.IndexOf(tokens, "movetime") + 1]);
            }
            if (engine.board.WhiteToMove)
            {
                if (tokens.Contains("wtime"))
                {
                    timeLeft = int.Parse(tokens[Array.IndexOf(tokens, "wtime") + 1]);
                }

                if (tokens.Contains("winc"))
                {
                    increment = int.Parse(tokens[Array.IndexOf(tokens, "winc") + 1]);
                }
            }
            else
            {
                if (tokens.Contains("btime"))
                {
                    timeLeft = int.Parse(tokens[Array.IndexOf(tokens, "btime") + 1]);
                }

                if (tokens.Contains("binc"))
                {
                    increment = int.Parse(tokens[Array.IndexOf(tokens, "binc") + 1]);
                }
            }


            return GetSearchTime(timeLeft, increment);
        }

        public static string GetCorrectEval(int eval)
        {
            if (Math.Abs(eval) > 99999999)
            {
                int mateLength = 999999999 - Math.Abs(eval) + 1;
                mateLength *= eval < 0 ? -1 : 1;
                return "mate " + mateLength / 2;
            }
            else
            {
                return "cp " + eval;
            }
        }
    }
}
