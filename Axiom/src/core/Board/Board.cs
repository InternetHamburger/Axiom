using Axiom.src.core.Utility;

namespace Axiom.src.core.Board
{
    public sealed class Board
    {
        public GameState CurrentGameState;
        public bool WhiteToMove;
        public readonly byte[] Squares;
        public ulong[] BitBoards;

        private readonly Stack<GameState> GameHistory;



        public Board(string fen = BoardUtility.StartPos)
        {
            Squares = new byte[64];
            BitBoards = new ulong[Piece.MaxPieceIndex + 1];
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

            if (move.IsDoublePawnPush)
            {
                newEnpassantFile = MoveUtility.File(targetSquare);
            }
            else if (move.IsPromotion)
            {
                Squares[targetSquare] = (byte)(move.PromotionPieceType | (WhiteToMove ? Piece.White : Piece.Black));
            }
            else if (move.IsEnPassantCapture)
            {
                int enPassantCaptureSquare = targetSquare + (WhiteToMove ? 8 : -8);
                Squares[enPassantCaptureSquare] = Piece.None;
            }
            else if (move.MoveFlag == Move.CastleFlag)
            {
                switch (targetSquare)
                {
                    case 62: // white short castle (g1)
                        Squares[61] = Squares[63];
                        Squares[63] = Piece.None;
                        castlingRights &= GameState.ClearWhiteKingsideMask;
                        break;
                    case 58: // white loing castle (c1)
                        Squares[59] = Squares[56];
                        Squares[56] = Piece.None;
                        castlingRights &= GameState.ClearWhiteQueensideMask;
                        break;
                    case 6: // black short castle (g8)
                        Squares[5] = Squares[7];
                        Squares[7] = Piece.None;
                        castlingRights &= GameState.ClearBlackKingsideMask;
                        break;
                    case 2: // black long castle (c8)
                        Squares[3] = Squares[0];
                        Squares[0] = Piece.None;
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

            if (move.IsPromotion)
            {
                Squares[startSquare] = (byte)(Piece.Pawn | (WhiteToMove ? Piece.White : Piece.Black));
            }
            else if (move.IsEnPassantCapture)
            {
                int enPassantCaptureSquare = targetSquare + (WhiteToMove ? 8 : -8);
                Squares[enPassantCaptureSquare] = (byte)(Piece.Pawn | (WhiteToMove ? Piece.Black : Piece.White));
            }
            else if (move.MoveFlag == Move.CastleFlag)
            {
                switch (targetSquare)
                {
                    case 62: // white short castle (g1)
                        Squares[63] = Squares[61];
                        Squares[61] = Piece.None;
                        break;
                    case 58: // white loing castle (c1)
                        Squares[56] = Squares[59];
                        Squares[59] = Piece.None;
                        break;
                    case 6: // black short castle (g8)
                        Squares[7] = Squares[5];
                        Squares[5] = Piece.None;
                        break;
                    case 2: // black long castle (c8)
                        Squares[0] = Squares[3];
                        Squares[3] = Piece.None;
                        break;
                }
            }

            CurrentGameState = GameHistory.Pop();
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
    }
}
