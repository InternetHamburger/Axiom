using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Board
{
    static class Zobrist
    {
        public static ulong PseudoRandomNumber(ref ulong value)
        {
            value = value * 74779667405 + 2891398636453;
            value = ((value >> (int)((value >> 28) + 4)) ^ value) * 277803576737u;
            value = (value >> 22) ^ value;
            return value;
        }

        public static readonly ulong[,] ZobristPieceValues = new ulong[Piece.MaxPieceIndex + 1, 64];
        public static readonly ulong[] EnPassantFiles = new ulong[8];
        public static readonly ulong WhiteCastleShort;
        public static readonly ulong WhiteCastleLong;
        public static readonly ulong BlackCastleShort;
        public static readonly ulong BlackCastleLong;
        public static readonly ulong WhiteToMove;
        

        static Zobrist()
        {
            ulong Seed = 4161232025416123;
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
            for (int i = 0; i < 8; i++)
            {
                EnPassantFiles[i] = PseudoRandomNumber(ref Seed);
            }
            WhiteCastleShort = PseudoRandomNumber(ref Seed);
            WhiteCastleLong = PseudoRandomNumber(ref Seed);
            BlackCastleShort = PseudoRandomNumber(ref Seed);
            BlackCastleLong = PseudoRandomNumber(ref Seed);
            WhiteToMove = PseudoRandomNumber(ref Seed);
        }
    }
}
