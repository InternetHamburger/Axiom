using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;

namespace Axiom.src.core.Board
{
    public sealed class Board
    {
        public GameState CurrentGameState;
        public bool WhiteToMove;
        public readonly byte[] Squares;
        public ulong[] BitBoards;
        public int[] KingSquares;

        private readonly Stack<GameState> GameHistory;



        public Board(string fen = BoardUtility.StartPos)
        {
            Squares = new byte[64];
            BitBoards = new ulong[Piece.MaxPieceIndex + 1];
            KingSquares = new int[2];
            WhiteToMove = true;

            CurrentGameState = new GameState();
            GameHistory = new Stack<GameState>();

            SetPosition(fen);
        }

        public void MakeMove(Move move)
        {
            int castlingRights = GameHistory.Peek().castlingRights;
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;

            byte movedPiece = Squares[startSquare];
            byte capturedPiece = Squares[targetSquare];

            int newEnpassantFile = -1;

            Squares[targetSquare] = movedPiece;
            Squares[startSquare] = Piece.None; // A move always leaves an empty square

            BitBoards[movedPiece] ^= 1UL << startSquare | 1UL << targetSquare;

            if (Piece.PieceType(movedPiece) == Piece.King)
            {
                if (WhiteToMove)
                {
                    castlingRights &= GameState.ClearWhiteKingsideMask;
                    castlingRights &= GameState.ClearWhiteQueensideMask;
                }
                else
                {
                    castlingRights &= GameState.ClearBlackKingsideMask;
                    castlingRights &= GameState.ClearBlackQueensideMask;
                }
                KingSquares[WhiteToMove ? 0 : 1] = targetSquare;
            }

            // Move is a capture
            if (capturedPiece != Piece.None)
            {
                BitBoards[capturedPiece] ^= 1UL << targetSquare;
            }

            if (move.IsDoublePawnPush)
            {
                newEnpassantFile = BoardUtility.File(targetSquare);
            }
            else if (move.IsPromotion)
            {
                byte promotionPiece = (byte)(move.PromotionPieceType | (WhiteToMove ? Piece.White : Piece.Black));
                BitBoards[movedPiece] ^= 1UL << targetSquare;
                BitBoards[promotionPiece] ^= 1UL << targetSquare;
                Squares[targetSquare] = promotionPiece;
            }
            else if (move.IsEnPassantCapture)
            {
                int enPassantCaptureSquare = targetSquare + (WhiteToMove ? 8 : -8);
                byte capturedPawn = WhiteToMove ? Piece.BlackPawn : Piece.WhitePawn;
                Squares[enPassantCaptureSquare] = Piece.None;
                BitBoards[capturedPawn] ^= 1UL << enPassantCaptureSquare;
            }
            else if (move.MoveFlag == Move.CastleFlag)
            {
                switch (targetSquare)
                {
                    case 62: // white short castle (g1)
                        Squares[61] = Squares[63];
                        Squares[63] = Piece.None;
                        BitBoards[Piece.WhiteRook] ^= 1UL << 61 | 1UL << 63;
                        castlingRights &= GameState.ClearWhiteKingsideMask;
                        break;
                    case 58: // white loing castle (c1)
                        Squares[59] = Squares[56];
                        Squares[56] = Piece.None;
                        BitBoards[Piece.WhiteRook] ^= 1UL << 59 | 1UL << 56;
                        castlingRights &= GameState.ClearWhiteQueensideMask;
                        break;
                    case 6: // black short castle (g8)
                        Squares[5] = Squares[7];
                        Squares[7] = Piece.None;
                        BitBoards[Piece.BlackRook] ^= 1UL << 5 | 1UL << 7;
                        castlingRights &= GameState.ClearBlackKingsideMask;
                        break;
                    case 2: // black long castle (c8)
                        Squares[3] = Squares[0];
                        Squares[0] = Piece.None;
                        BitBoards[Piece.BlackRook] ^= 1UL << 0 | 1UL << 3;
                        castlingRights &= GameState.ClearBlackQueensideMask;
                        break;
                }
            }

            GameState newGameState = new(capturedPiece, newEnpassantFile, castlingRights, 0, 0);
            CurrentGameState = newGameState;

            WhiteToMove = !WhiteToMove;
            GameHistory.Push(newGameState);
        }

        public void UndoMove(Move move)
        {
            WhiteToMove = !WhiteToMove;
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;

            byte movedPiece = Squares[targetSquare];
            byte capturedPiece = CurrentGameState.capturedPieceType;

            Squares[startSquare] = movedPiece;
            Squares[targetSquare] = capturedPiece;
            BitBoards[movedPiece] ^= 1UL << startSquare | 1UL << targetSquare;

            if (Piece.PieceType(movedPiece) == Piece.King)
            {
                KingSquares[WhiteToMove ? 0 : 1] = startSquare;
            }
            if (move.IsPromotion)
            {
                Squares[startSquare] = (byte)(Piece.Pawn | (WhiteToMove ? Piece.White : Piece.Black));
                BitBoards[movedPiece] ^= 1UL << startSquare;
            }
            else if (move.IsEnPassantCapture)
            {
                int enPassantCaptureSquare = targetSquare + (WhiteToMove ? 8 : -8);
                Squares[enPassantCaptureSquare] = (byte)(Piece.Pawn | (WhiteToMove ? Piece.Black : Piece.White));
                BitBoards[Squares[enPassantCaptureSquare]] ^= 1UL << enPassantCaptureSquare;
            }
            else if (move.MoveFlag == Move.CastleFlag)
            {
                switch (targetSquare)
                {
                    case 62: // white short castle (g1)
                        Squares[63] = Squares[61];
                        Squares[61] = Piece.None;
                        BitBoards[Piece.WhiteRook] ^= 1UL << 61 | 1UL << 63;
                        break;
                    case 58: // white loing castle (c1)
                        Squares[56] = Squares[59];
                        Squares[59] = Piece.None;
                        BitBoards[Piece.WhiteRook] ^= 1UL << 59 | 1UL << 56;
                        break;
                    case 6: // black short castle (g8)
                        Squares[7] = Squares[5];
                        Squares[5] = Piece.None;
                        BitBoards[Piece.BlackRook] ^= 1UL << 5 | 1UL << 7;
                        break;
                    case 2: // black long castle (c8)
                        Squares[0] = Squares[3];
                        Squares[3] = Piece.None;
                        BitBoards[Piece.BlackRook] ^= 1UL << 0 | 1UL << 3;
                        break;
                }
            }

            CurrentGameState = GameHistory.Pop();
        }

        public bool IsInCheck()
        {
            int square = KingSquares[WhiteToMove ? 0 : 1];
            for (int directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                for (int n = 0; n < MoveGenConstants.numSquaresToEdge[square, directionIndex]; n++)
                {

                    int targetSquare = square + MoveGenConstants.DirectionOffSets[directionIndex] * (n + 1);
                    byte pieceOnTargetSquare = Squares[targetSquare];

                    // Blocked by friendly piece
                    if (Piece.IsColour(pieceOnTargetSquare, WhiteToMove ? 0 : 8))
                    {
                        break;
                    }

                    // Blocked by enemy piece
                    if (Piece.IsColour(pieceOnTargetSquare, WhiteToMove ? 8 : 0))
                    {
                        if (pieceOnTargetSquare)

                        break;
                    }
                }
            }


            return false;
        }



        public void SetPosition(string fen)
        {
            FenUtility.PositionInfo pos = new(fen);
            SetPosition(pos);
        }

        public void SetPosition(FenUtility.PositionInfo pos)
        {
            WhiteToMove = pos.whiteToMove;
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                Squares[squareIndex] = pos.Squares[squareIndex];

                BitBoards[Squares[squareIndex]] |= 1UL << squareIndex;
               
            }
            CurrentGameState = new(0, pos.epFile, pos.fullCastlingRights, pos.fiftyMovePlyCount, 0);
            GameHistory.Push(CurrentGameState);
        }



        public ulong AllPieceBitBoard => BitBoards[Piece.WhitePawn] | BitBoards[Piece.BlackPawn] | BitBoards[Piece.WhiteKnight] | BitBoards[Piece.BlackKnight] | BitBoards[Piece.WhiteBishop] | BitBoards[Piece.BlackBishop] | BitBoards[Piece.WhiteRook] | BitBoards[Piece.BlackRook] | BitBoards[Piece.WhiteQueen] | BitBoards[Piece.BlackQueen] | BitBoards[Piece.WhiteKing] | BitBoards[Piece.BlackKing];
        public ulong WhitePieceBitBoard => BitBoards[Piece.WhitePawn] | BitBoards[Piece.WhiteKnight] | BitBoards[Piece.WhiteBishop] | BitBoards[Piece.WhiteRook] | BitBoards[Piece.WhiteQueen] | BitBoards[Piece.WhiteKing];
        public ulong BlackPieceBitBoard => BitBoards[Piece.BlackPawn] | BitBoards[Piece.BlackKnight] | BitBoards[Piece.BlackBishop] | BitBoards[Piece.BlackRook] | BitBoards[Piece.BlackQueen] | BitBoards[Piece.BlackKing];
    }
}
