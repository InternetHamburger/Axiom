using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Perft;
using Axiom.src.core.Utility;

namespace Axiom
{
    internal class Program
    {
        static void Main()
        {
            Board board = new("8/3P4/8/8/8/8/8/8 w - - 0 1");
            BoardUtility.PrintBoard(board);
            BitBoardUtlity.PrintBitBoard(board.BitBoards[Piece.WhitePawn]);
            BitBoardUtlity.PrintBitBoard(board.BitBoards[Piece.WhiteQueen]);
            Move move = new(BoardUtility.NameOfSquare("d7"), BoardUtility.NameOfSquare("d8"), Move.PromoteToQueenFlag);
            board.MakeMove(move);
            BitBoardUtlity.PrintBitBoard(board.BitBoards[Piece.WhitePawn]);
            BitBoardUtlity.PrintBitBoard(board.BitBoards[Piece.WhiteQueen]);
            board.UndoMove(move);
            BitBoardUtlity.PrintBitBoard(board.BitBoards[Piece.WhitePawn]);
            BitBoardUtlity.PrintBitBoard(board.BitBoards[Piece.WhiteQueen]);
        }
    }
}
