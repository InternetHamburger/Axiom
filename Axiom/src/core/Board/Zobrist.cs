namespace Axiom.src.core.Board
{
    static class Zobrist
    {
        public static ulong PseudoRandomNumber(ref ulong value)
        {
            value ^= ((value + 92875224317UL) << 48) ^ ((16875224059UL * (value + 92875223911UL)) >> 32) + 92875224317UL;
            value ^= ((value + 52875224453UL) >> 30) ^ ((92875223911UL * (value + 92875223911UL)) << 48) + 52875224453UL;
            value ^= (value >> 30) * 52875224453UL * value + 92875223911UL;
            return value * value;
        }


        public static readonly ulong[,] ZobristPieceValues = new ulong[Piece.MaxPieceIndex + 1, 64];
        public static readonly ulong[] CastlingRights = new ulong[16];
        public static readonly ulong[] EnPassantFiles = new ulong[8];
        public static readonly ulong WhiteToMove;


        static Zobrist()
        {
            ulong Seed = 4161232026754816123;
            for (int i = 0; i < 64; i++)
            {
                ZobristPieceValues[Piece.WhitePawn, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.WhiteKnight, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.WhiteBishop, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.WhiteRook, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.WhiteQueen, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.WhiteKing, i] = PseudoRandomNumber(ref Seed);

                ZobristPieceValues[Piece.BlackPawn, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.BlackKnight, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.BlackBishop, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.BlackRook, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.BlackQueen, i] = PseudoRandomNumber(ref Seed);
                ZobristPieceValues[Piece.BlackKing, i] = PseudoRandomNumber(ref Seed);
            }
            for (int i = 0; i < 16; i++)
            {
                CastlingRights[i] = PseudoRandomNumber(ref Seed);
            }
            for (int i = 0; i < 8; i++)
            {
                EnPassantFiles[i] = PseudoRandomNumber(ref Seed);
            }
            WhiteToMove = PseudoRandomNumber(ref Seed);
        }
    }
}
