using Axiom.src.core.Board;
using Axiom.src.core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Move_Generation
{
    public class MoveGenerator
    {
        public const int MaxNumMoves = 218;


        public static Move[] GetPseudoLegalMoves(Board.Board board)
        {
            Move[] moves = new Move[MaxNumMoves];
            int numGeneratedMoves = 0;
            int color = board.WhiteToMove ? Piece.White : Piece.Black;

            // Generate pawn moves independtently of main loop
            GeneratePawnMoves(board, ref moves, ref numGeneratedMoves);

            for (int i = 0; i < 64; i++)
            {
                byte piece = board.Squares[i];

                if (Piece.IsColour(piece, color))
                {
                    if (Piece.IsSlidingPiece(piece))
                    {
                        GenerateSlidingMoves(board, ref moves, ref numGeneratedMoves, piece, i);
                    }
                    switch (Piece.PieceType(piece))
                    {
                        case Piece.Knight:
                            GenerateKnightMoves(board, ref moves, ref numGeneratedMoves, i);
                            break;
                    }

                }
            }

            // Slice off ungenerated moves
            if (numGeneratedMoves == 218)
            {
                return moves;
            }
            else
            {
                Move[] result = new Move[numGeneratedMoves];
                Array.Copy(moves, result, numGeneratedMoves);
                return result;
            }
        }



        private static void GenerateSlidingMoves(Board.Board board, ref Move[] moves, ref int numGeneratedMoves, byte piece, int square)
        {
            int startIndex = Piece.IsOrthogonalSlider(piece) ? 0 : 4;
            int endIndex = Piece.IsDiagonalSlider(piece) ? 8 : 4;

            for (int directionIndex = startIndex; directionIndex < endIndex; directionIndex++)
            {
                for (int n = 0; n < MoveGenConstants.numSquaresToEdge[square, directionIndex]; n++)
                {

                    int targetSquare = square + MoveGenConstants.DirectionOffSets[directionIndex] * (n + 1);
                    byte pieceOnTargetSquare = board.Squares[targetSquare];

                    // Blocked by friendly piece
                    if (Piece.IsColour(pieceOnTargetSquare, board.WhiteToMove ? 0 : 8))
                    {
                        break;
                    }

                    moves[numGeneratedMoves++] = new Move(square, targetSquare);

                    // Blocked by enemy piece
                    if (Piece.IsColour(pieceOnTargetSquare, board.WhiteToMove ? 8 : 0))
                    {
                        break;
                    }
                }
            }
        }


        private static void GeneratePawnMoves(Board.Board board, ref Move[] moves, ref int numGeneratedMoves)
        {
            if (board.WhiteToMove)
            {
                ulong pawns = board.BitBoards[Piece.WhitePawn];
                ulong pawnMoves;

                // Double pawn pushes
                pawnMoves = (pawns & MoveGenConstants.WhiteStartRank) >> 8;
                pawnMoves &= ~board.AllPieceBitBoard;
                pawnMoves >>= 8;
                pawnMoves &= ~board.AllPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 16, targetSquare, Move.PawnTwoUpFlag);
                }

                // Single pawn pushes
                // Filter out promotion moves
                pawnMoves = (pawns & MoveGenConstants.WhiteMoveMask) >> 8;
                pawnMoves &= ~board.AllPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 8, targetSquare);
                }

                // Capture right
                pawnMoves = (pawns & MoveGenConstants.WhitePawnCaptureRightMask & MoveGenConstants.WhiteMoveMask) >> 7;
                pawnMoves &= board.BlackPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 7, targetSquare);
                }

                // Capture left
                pawnMoves = (pawns & MoveGenConstants.WhitePawnCaptureLeftMask & MoveGenConstants.WhiteMoveMask) >> 9;
                pawnMoves &= board.BlackPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 9, targetSquare);
                }


                // Promotion
                // Single pawn pushes
                pawnMoves = (pawns & MoveGenConstants.WhitePromotionMask) >> 8;
                pawnMoves &= ~board.AllPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 8, targetSquare, Move.PromoteToQueenFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 8, targetSquare, Move.PromoteToRookFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 8, targetSquare, Move.PromoteToBishopFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 8, targetSquare, Move.PromoteToKnightFlag);
                }

                // Capture right
                pawnMoves = (pawns & MoveGenConstants.WhitePawnCaptureRightMask & MoveGenConstants.WhitePromotionMask) >> 7;
                pawnMoves &= board.BlackPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 7, targetSquare, Move.PromoteToQueenFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 7, targetSquare, Move.PromoteToRookFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 7, targetSquare, Move.PromoteToBishopFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 7, targetSquare, Move.PromoteToKnightFlag);
                }

                // Capture left
                pawnMoves = (pawns & MoveGenConstants.WhitePawnCaptureLeftMask & MoveGenConstants.WhitePromotionMask) >> 9;
                pawnMoves &= board.BlackPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 9, targetSquare, Move.PromoteToQueenFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 9, targetSquare, Move.PromoteToRookFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 9, targetSquare, Move.PromoteToBishopFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare + 9, targetSquare, Move.PromoteToKnightFlag);
                }


                // En passant
                if (board.CurrentGameState.enPassantFile != -1)
                {
                    ulong enPassantSquareMask = 1UL << (board.CurrentGameState.enPassantFile + 16);
                    BitBoardUtlity.PrintBitBoard(enPassantSquareMask);

                    // Capture right
                    pawnMoves = (pawns & MoveGenConstants.WhitePawnCaptureRightMask) >> 7;
                    pawnMoves &= enPassantSquareMask;

                    while (pawnMoves != 0)
                    {
                        int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                        moves[numGeneratedMoves++] = new Move(targetSquare + 7, targetSquare, Move.EnPassantCaptureFlag);
                    }

                    // Capture left
                    pawnMoves = (pawns & MoveGenConstants.WhitePawnCaptureLeftMask) >> 9;
                    pawnMoves &= enPassantSquareMask;

                    while (pawnMoves != 0)
                    {
                        int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                        moves[numGeneratedMoves++] = new Move(targetSquare + 9, targetSquare, Move.EnPassantCaptureFlag);
                    }

                }
            }
            else
            {
                ulong pawns = board.BitBoards[Piece.BlackPawn];
                ulong pawnMoves;

                // Double pawn pushes
                pawnMoves = (pawns & MoveGenConstants.BlackStartRank) << 8;
                pawnMoves &= ~board.AllPieceBitBoard;
                pawnMoves <<= 8;
                pawnMoves &= ~board.AllPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 16, targetSquare, Move.PawnTwoUpFlag);
                }

                // Single pawn pushes
                // Filter out promotion moves
                pawnMoves = (pawns & MoveGenConstants.BlackMoveMask) << 8;
                pawnMoves &= ~board.AllPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 8, targetSquare);
                }

                // Capture right
                pawnMoves = (pawns & MoveGenConstants.BlackPawnCaptureRightMask & MoveGenConstants.BlackMoveMask) << 7;
                pawnMoves &= board.WhitePieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 7, targetSquare);
                }

                // Capture left
                pawnMoves = (pawns & MoveGenConstants.BlackPawnCaptureLeftMask & MoveGenConstants.BlackMoveMask) << 9;
                pawnMoves &= board.WhitePieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 9, targetSquare);
                }


                // Promotion
                // Single pawn pushes
                pawnMoves = (pawns & MoveGenConstants.WhitePromotionMask) << 8;
                pawnMoves &= ~board.AllPieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 8, targetSquare, Move.PromoteToQueenFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 8, targetSquare, Move.PromoteToRookFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 8, targetSquare, Move.PromoteToBishopFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 8, targetSquare, Move.PromoteToKnightFlag);
                }

                // Capture right
                pawnMoves = (pawns & MoveGenConstants.WhitePawnCaptureRightMask & MoveGenConstants.WhitePromotionMask) << 7;
                pawnMoves &= board.WhitePieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 7, targetSquare, Move.PromoteToQueenFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 7, targetSquare, Move.PromoteToRookFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 7, targetSquare, Move.PromoteToBishopFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 7, targetSquare, Move.PromoteToKnightFlag);
                }

                // Capture left
                pawnMoves = (pawns & MoveGenConstants.BlackPawnCaptureLeftMask & MoveGenConstants.BlackPromotionMask) << 9;
                pawnMoves &= board.WhitePieceBitBoard;

                while (pawnMoves != 0)
                {
                    int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 9, targetSquare, Move.PromoteToQueenFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 9, targetSquare, Move.PromoteToRookFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 9, targetSquare, Move.PromoteToBishopFlag);
                    moves[numGeneratedMoves++] = new Move(targetSquare - 9, targetSquare, Move.PromoteToKnightFlag);
                }


                // En passant
                if (board.CurrentGameState.enPassantFile != -1)
                {
                    ulong enPassantSquareMask = 1UL << (board.CurrentGameState.enPassantFile + 40);
                    BitBoardUtlity.PrintBitBoard(enPassantSquareMask);

                    // Capture right
                    pawnMoves = (pawns & MoveGenConstants.BlackPawnCaptureRightMask) << 7;
                    pawnMoves &= enPassantSquareMask;

                    while (pawnMoves != 0)
                    {
                        int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                        moves[numGeneratedMoves++] = new Move(targetSquare - 7, targetSquare, Move.EnPassantCaptureFlag);
                    }

                    // Capture left
                    pawnMoves = (pawns & MoveGenConstants.BlackPawnCaptureLeftMask) << 9;
                    pawnMoves &= enPassantSquareMask;

                    while (pawnMoves != 0)
                    {
                        int targetSquare = BitBoardUtlity.PopLSB(ref pawnMoves);
                        moves[numGeneratedMoves++] = new Move(targetSquare - 9, targetSquare, Move.EnPassantCaptureFlag);
                    }

                }
            }
        }


        private static void GenerateKnightMoves(Board.Board board, ref Move[] moves, ref int numGeneratedMoves, int square)
        {
            ulong possibleMoves = PreComputedMoveData.KnightAttacks[square];

            possibleMoves &= ~(board.WhiteToMove ? board.WhitePieceBitBoard : board.BlackPieceBitBoard);

            while (possibleMoves != 0)
            {
                int targetSquare = BitBoardUtlity.PopLSB(ref possibleMoves);
                moves[numGeneratedMoves++] = new Move(square, targetSquare);
            }
        }
    }
}
