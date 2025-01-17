using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;

namespace Axiom.src.core.Perft
{
    static class Perft
    {
        public static void PerftSearch(string fen, int depth)
        {
            Board.Board board = new(fen);

            if (depth == 0)
            {
                Console.WriteLine("Nodes searched: " + 1);
                return;
            }

            Move[] moves = MoveGenerator.GetPseudoLegalMoves(board);

            ulong totalNodes = 0;
            ulong nodes = 0;
            foreach(Move move in moves)
            {
                board.MakeMove(move);
                if (board.IsInCheck())
                {
                    continue;
                }
                nodes = SearchNoBulk(board, depth - 1);
                board.UndoMove(move);
                totalNodes += nodes;
                Console.WriteLine($"{BoardUtility.MoveToUci(move)}: {nodes}");
                //BitBoardUtlity.PrintBitBoard(board.AllPieceBitBoard);
            }
            Console.WriteLine("\nNodes searched: " + totalNodes);
        }

        public static void PerftSearch(Board.Board board, int depth)
        {

            if (depth == 0)
            {
                Console.WriteLine("Nodes searched: " + 1);
                return;
            }

            Move[] moves = MoveGenerator.GetPseudoLegalMoves(board);

            ulong totalNodes = 0;
            ulong nodes = 0;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                if (board.IsInCheck())
                {
                    continue;
                }
                nodes = SearchNoBulk(board, depth - 1);
                board.UndoMove(move);
                totalNodes += nodes;
                Console.WriteLine($"{BoardUtility.MoveToUci(move)}: {nodes}");
                //BitBoardUtlity.PrintBitBoard(board.AllPieceBitBoard);
            }
            Console.WriteLine("\nNodes searched: " + totalNodes);
        }

        public static ulong SearchNoBulk(Board.Board board, int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            Move[] moves = MoveGenerator.GetPseudoLegalMoves(board);

            ulong nodes = 0;
            foreach(Move move in moves)
            {
                board.MakeMove(move);
                nodes += SearchNoBulk(board, depth - 1);
                board.UndoMove(move);
            }
            return nodes;
        }
    }
}