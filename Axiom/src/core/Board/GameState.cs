namespace Axiom.src.core.Board
{
    public readonly struct GameState
    {
        public readonly byte capturedPiece;
        public readonly int enPassantFile;
        public readonly int castlingRights;
        public readonly int fiftyMoveCounter;
        public readonly ulong zobristKey;

        public const int ClearWhiteKingsideMask = 0b1110;
        public const int ClearWhiteQueensideMask = 0b1101;
        public const int ClearBlackKingsideMask = 0b1011;
        public const int ClearBlackQueensideMask = 0b0111;

        public GameState(byte capturedPiece, int enPassantFile, int castlingRights, int fiftyMoveCounter, ulong zobristKey)
        {
            this.capturedPiece = capturedPiece;
            this.enPassantFile = enPassantFile;
            this.castlingRights = castlingRights;
            this.fiftyMoveCounter = fiftyMoveCounter;
            this.zobristKey = zobristKey;
        }

        public bool HasKingsideCastleRight(bool white)
        {
            int mask = white ? 1 : 4;
            return (castlingRights & mask) != 0;
        }

        public bool HasQueensideCastleRight(bool white)
        {
            int mask = white ? 2 : 8;
            return (castlingRights & mask) != 0;
        }

    }
}
