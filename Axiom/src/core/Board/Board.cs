using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;

namespace Axiom.src.core.Board
{
    public sealed class Board
    {
        public const ulong InitialZobristSeed = 2576434379651712347;

        public GameState CurrentGameState;
        public bool WhiteToMove;
        public byte[] Squares;
        public ulong[] BitBoards;
        public int[] KingSquares;
        public ulong ZobristHash;

        private Stack<GameState> GameHistory;
        private List<ulong> RepetitionHistory;



        public Board(string fen = BoardUtility.StartPos)
        {
            Squares = new byte[64];
            BitBoards = new ulong[Piece.MaxPieceIndex + 1];
            KingSquares = new int[2];
            WhiteToMove = true;

            CurrentGameState = new GameState();
            GameHistory = new Stack<GameState>(256);
            RepetitionHistory = new List<ulong>();

            SetPosition(fen);
        }

        private void Init()
        {
            Squares = new byte[64];
            BitBoards = new ulong[Piece.MaxPieceIndex + 1];
            KingSquares = new int[2];
            WhiteToMove = true;

            CurrentGameState = new GameState();
            GameHistory = new Stack<GameState>(256);
            RepetitionHistory = new List<ulong>();
        }

        public void MakeMove(Move move)
        {
            RepetitionHistory.Add(ZobristHash);
            int castlingRights = GameHistory.Peek().castlingRights;
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;

            byte movedPiece = Squares[startSquare];
            byte capturedPiece = Squares[targetSquare];

            int newEnpassantFile = -1;

            Squares[targetSquare] = movedPiece;
            Squares[startSquare] = Piece.None; // A move always leaves an empty square

            ZobristHash ^= Zobrist.ZobristPieceValues[movedPiece, startSquare];
            ZobristHash ^= Zobrist.ZobristPieceValues[movedPiece, targetSquare];

            BitBoards[movedPiece] ^= 1UL << startSquare | 1UL << targetSquare;


            // Remove legality for en passant
            if (CurrentGameState.enPassantFile != -1)
            {
                ZobristHash ^= Zobrist.EnPassantFiles[CurrentGameState.enPassantFile];
            }

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

            // Remove castling rights
            if (startSquare == 63 || targetSquare == 63) // h1
            {
                castlingRights &= GameState.ClearWhiteKingsideMask;
            }
            if (startSquare == 56 || targetSquare == 56) // a1
            {
                castlingRights &= GameState.ClearWhiteQueensideMask;
            }
            if (startSquare == 7 || targetSquare == 7) // h8
            {
                castlingRights &= GameState.ClearBlackKingsideMask;
            }
            if (startSquare == 0 || targetSquare == 0) // a8
            {
                castlingRights &= GameState.ClearBlackQueensideMask;
            }


            // Move is a capture
            if (capturedPiece != Piece.None)
            {
                BitBoards[capturedPiece] ^= 1UL << targetSquare;
                ZobristHash ^= Zobrist.ZobristPieceValues[capturedPiece, targetSquare];
            }

            if (move.IsDoublePawnPush)
            {
                newEnpassantFile = BoardUtility.File(targetSquare);
                ZobristHash ^= Zobrist.EnPassantFiles[newEnpassantFile];
            }
            else if (move.IsPromotion)
            {
                byte promotionPiece = (byte)(move.PromotionPieceType | (WhiteToMove ? Piece.White : Piece.Black));
                BitBoards[movedPiece] ^= 1UL << targetSquare;
                BitBoards[promotionPiece] ^= 1UL << targetSquare;
                Squares[targetSquare] = promotionPiece;
                ZobristHash ^= Zobrist.ZobristPieceValues[movedPiece, targetSquare];
                ZobristHash ^= Zobrist.ZobristPieceValues[promotionPiece, targetSquare];
            }
            else if (move.IsEnPassantCapture)
            {
                int enPassantCaptureSquare = targetSquare + (WhiteToMove ? 8 : -8);
                byte capturedPawn = WhiteToMove ? Piece.BlackPawn : Piece.WhitePawn;
                Squares[enPassantCaptureSquare] = Piece.None;
                BitBoards[capturedPawn] ^= 1UL << enPassantCaptureSquare;
                ZobristHash ^= Zobrist.ZobristPieceValues[capturedPawn, enPassantCaptureSquare];
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
                        ZobristHash ^= Zobrist.ZobristPieceValues[Piece.WhiteRook, 61];
                        ZobristHash ^= Zobrist.ZobristPieceValues[Piece.WhiteRook, 63];
                        break;
                    case 58: // white loing castle (c1)
                        Squares[59] = Squares[56];
                        Squares[56] = Piece.None;
                        BitBoards[Piece.WhiteRook] ^= 1UL << 59 | 1UL << 56;
                        castlingRights &= GameState.ClearWhiteQueensideMask;
                        ZobristHash ^= Zobrist.ZobristPieceValues[Piece.WhiteRook, 56];
                        ZobristHash ^= Zobrist.ZobristPieceValues[Piece.WhiteRook, 59];
                        break;
                    case 6: // black short castle (g8)
                        Squares[5] = Squares[7];
                        Squares[7] = Piece.None;
                        BitBoards[Piece.BlackRook] ^= 1UL << 5 | 1UL << 7;
                        castlingRights &= GameState.ClearBlackKingsideMask;
                        ZobristHash ^= Zobrist.ZobristPieceValues[Piece.BlackRook, 5];
                        ZobristHash ^= Zobrist.ZobristPieceValues[Piece.BlackRook, 7];
                        break;
                    case 2: // black long castle (c8)
                        Squares[3] = Squares[0];
                        Squares[0] = Piece.None;
                        BitBoards[Piece.BlackRook] ^= 1UL << 0 | 1UL << 3;
                        castlingRights &= GameState.ClearBlackQueensideMask;
                        ZobristHash ^= Zobrist.ZobristPieceValues[Piece.BlackRook, 0];
                        ZobristHash ^= Zobrist.ZobristPieceValues[Piece.BlackRook, 3];
                        break;
                }
            }
            ZobristHash ^= Zobrist.CastlingRights[castlingRights];
            ZobristHash ^= Zobrist.CastlingRights[GameHistory.Peek().castlingRights];
            ZobristHash ^= Zobrist.WhiteToMove;

            GameState newGameState = new(capturedPiece, newEnpassantFile, castlingRights, 0, ZobristHash);
            CurrentGameState = newGameState;

            WhiteToMove = !WhiteToMove;
            GameHistory.Push(newGameState);
        }

        public void UndoMove(Move move)
        {
            WhiteToMove = !WhiteToMove;
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;

            byte movedPiece = Squares[targetSquare]; // The promoted piece in case of promotion
            byte capturedPiece = CurrentGameState.capturedPiece;

            Squares[startSquare] = movedPiece;
            Squares[targetSquare] = capturedPiece;
            BitBoards[movedPiece] ^= 1UL << startSquare | 1UL << targetSquare;


            if (capturedPiece != Piece.None)
            {
                BitBoards[capturedPiece] |= 1UL << targetSquare;
            }
            if (Piece.PieceType(movedPiece) == Piece.King)
            {
                KingSquares[WhiteToMove ? 0 : 1] = startSquare;
            }
            if (move.IsPromotion)
            {
                byte movedPawn = (byte)(Piece.Pawn | (WhiteToMove ? Piece.White : Piece.Black));
                Squares[startSquare] = movedPawn;
                BitBoards[movedPiece] ^= 1UL << startSquare;
                BitBoards[movedPawn] ^= 1UL << startSquare;
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

            GameHistory.Pop();
            CurrentGameState = GameHistory.Peek();
            ZobristHash = CurrentGameState.zobristKey;
            RepetitionHistory.RemoveAt(RepetitionHistory.Count - 1);
        }

        public void MakeNullMove()
        {
            RepetitionHistory.Add(ZobristHash);
            int castlingRights = GameHistory.Peek().castlingRights;

            byte capturedPiece = 0;

            int newEnpassantFile = -1;

            // Remove legality for en passant
            if (CurrentGameState.enPassantFile != -1)
            {
                ZobristHash ^= Zobrist.EnPassantFiles[CurrentGameState.enPassantFile];
            }

            ZobristHash ^= Zobrist.WhiteToMove;

            GameState newGameState = new(capturedPiece, newEnpassantFile, castlingRights, 0, ZobristHash);
            CurrentGameState = newGameState;

            WhiteToMove = !WhiteToMove;
            GameHistory.Push(newGameState);
        }

        public void UndoNullMove()
        {
            WhiteToMove = !WhiteToMove;

            GameHistory.Pop();
            CurrentGameState = GameHistory.Peek();
            ZobristHash = CurrentGameState.zobristKey;
            RepetitionHistory.RemoveAt(RepetitionHistory.Count - 1);
        }

        public bool IsInCheck(bool IsWhite)
        {
            int square = KingSquares[IsWhite ? 0 : 1];

            return IsUnderAttack(square, IsWhite);
        }

        public bool IsUnderAttack(int square, bool IsWhite)
        {

            IsWhite = !IsWhite;
            ulong bitboard = 1Ul << square;

            if (IsWhite)
            {
                if ((((bitboard & MoveGenConstants.WhitePawnCaptureLeftMask) << 7 | ((bitboard & MoveGenConstants.WhitePawnCaptureRightMask) << 9)) & BitBoards[Piece.WhitePawn]) != 0)
                {
                    return true;
                }
            }
            else
            {
                if ((((bitboard & MoveGenConstants.BlackPawnCaptureLeftMask) >> 7 | ((bitboard & MoveGenConstants.BlackPawnCaptureRightMask) >> 9)) & BitBoards[Piece.BlackPawn]) != 0)
                {
                    return true;
                }
            }
            if ((PreComputedMoveData.KnightAttacks[square] & BitBoards[IsWhite ? Piece.WhiteKnight : Piece.BlackKnight]) != 0)
            {
                return true;
            }
            else if ((PreComputedMoveData.KingAttacks[square] & BitBoards[IsWhite ? Piece.WhiteKing : Piece.BlackKing]) != 0)
            {
                return true;
            }

            ulong enemySliders;
            if (IsWhite)
            {
                enemySliders = BitBoards[Piece.WhiteBishop] | BitBoards[Piece.WhiteRook] | BitBoards[Piece.WhiteQueen];
            }
            else
            {
                enemySliders = BitBoards[Piece.BlackBishop] | BitBoards[Piece.BlackRook] | BitBoards[Piece.BlackQueen];
            }


            for (int directionIndex = 0; directionIndex < 8; directionIndex++)
            {

                if ((MoveGenConstants.RayMasks[square, directionIndex] & enemySliders) == 0)
                {
                    continue;
                }

                for (int n = 0; n < MoveGenConstants.numSquaresToEdge[square, directionIndex]; n++)
                {

                    int targetSquare = square + MoveGenConstants.DirectionOffSets[directionIndex] * (n + 1);
                    byte pieceOnTargetSquare = Squares[targetSquare];

                    // Blocked by friendly piece
                    if (Piece.IsColour(pieceOnTargetSquare, IsWhite ? 8 : 0))
                    {
                        break;
                    }

                    // Blocked by enemy piece
                    if (Piece.IsColour(pieceOnTargetSquare, IsWhite ? 0 : 8))
                    {
                        if (Piece.IsDiagonalSlider(pieceOnTargetSquare) && directionIndex > 3)
                        {
                            return true;
                        }
                        else if (Piece.IsOrthogonalSlider(pieceOnTargetSquare) && directionIndex < 4)
                        {
                            return true;
                        }
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
            Init();
            WhiteToMove = pos.whiteToMove;

            if (WhiteToMove)
            {
                ZobristHash ^= Zobrist.WhiteToMove;
            }

            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                Squares[squareIndex] = pos.Squares[squareIndex];

                BitBoards[Squares[squareIndex]] |= 1UL << squareIndex;
                ZobristHash ^= Zobrist.ZobristPieceValues[Squares[squareIndex], squareIndex];

                if (Piece.PieceType(Squares[squareIndex]) == Piece.King)
                {
                    if (Piece.IsWhite(Squares[squareIndex]))
                    {
                        KingSquares[0] = squareIndex;
                    }
                    else
                    {
                        KingSquares[1] = squareIndex;
                    }
                }
            }
            if (pos.epFile != -1)
            {
                ZobristHash ^= Zobrist.EnPassantFiles[pos.epFile];
            }
            ZobristHash ^= Zobrist.CastlingRights[pos.fullCastlingRights];

            CurrentGameState = new(0, pos.epFile, pos.fullCastlingRights, pos.fiftyMovePlyCount, ZobristHash);
            GameHistory.Push(CurrentGameState);
        }

        public bool IsThreefoldRepetition()
        {
            // Two-fold repetition
            if (RepetitionHistory.Count(pos => pos == ZobristHash) >= 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsTwofoldRepetition()
        {
            // Two-fold repetition
            if (RepetitionHistory.Count(pos => pos == ZobristHash) >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InEndgame(int GamePhase) => GamePhase > 200;

        public string Fen => FenUtility.GetFen(this);

        public ulong AllPieceBitBoard => BitBoards[Piece.WhitePawn] | BitBoards[Piece.BlackPawn] | BitBoards[Piece.WhiteKnight] | BitBoards[Piece.BlackKnight] | BitBoards[Piece.WhiteBishop] | BitBoards[Piece.BlackBishop] | BitBoards[Piece.WhiteRook] | BitBoards[Piece.BlackRook] | BitBoards[Piece.WhiteQueen] | BitBoards[Piece.BlackQueen] | BitBoards[Piece.WhiteKing] | BitBoards[Piece.BlackKing];
        public ulong WhitePieceBitBoard => BitBoards[Piece.WhitePawn] | BitBoards[Piece.WhiteKnight] | BitBoards[Piece.WhiteBishop] | BitBoards[Piece.WhiteRook] | BitBoards[Piece.WhiteQueen] | BitBoards[Piece.WhiteKing];
        public ulong BlackPieceBitBoard => BitBoards[Piece.BlackPawn] | BitBoards[Piece.BlackKnight] | BitBoards[Piece.BlackBishop] | BitBoards[Piece.BlackRook] | BitBoards[Piece.BlackQueen] | BitBoards[Piece.BlackKing];
    }
}
