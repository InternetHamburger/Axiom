using Axiom.src.core.Board;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axiom.src.core.Utility
{
    public static class FenUtility
    {
        public static string GetMainPart(string fen)
        {
            return fen.Split(" ")[0];
        }


        public readonly struct PositionInfo
        {
            public readonly string fen;
            public readonly ReadOnlyCollection<byte> Squares;

            // Castling rights
            public readonly bool whiteCastleKingside;
            public readonly bool whiteCastleQueenside;
            public readonly bool blackCastleKingside;
            public readonly bool blackCastleQueenside;
            // En passant file (1 is a-file, 8 is h-file, 0 means none)
            public readonly int epFile;
            public readonly bool whiteToMove;
            // Number of half-moves since last capture or pawn advance
            // (starts at 0 and increments after each player's move)
            public readonly int fiftyMovePlyCount;
            // Total number of moves played in the game
            // (starts at 1 and increments after black's move)
            public readonly int moveCount;

            public PositionInfo(string fen)
            {
                this.fen = fen;
                byte[] squarePieces = new byte[64];

                string[] sections = fen.Split(' ');

                string mainFen = GetMainPart(fen);

                int squareIndex = 0;
                int fenIndex = 0;

                while (fenIndex < mainFen.Length)
                {
                    char c = mainFen[fenIndex];

                    if (int.TryParse(c.ToString(), out int num))
                    {
                        squareIndex += num;
                        fenIndex++;
                    }

                    byte? piece = Piece.GetType(c);

                    if (piece == null)
                    {
                        fenIndex++;
                    }
                    else
                    {
                        squarePieces[squareIndex] = (byte)piece;
                        squareIndex++;
                        fenIndex++;
                    }
                }

                Squares = new(squarePieces);

                whiteToMove = (sections[1] == "w");

                string castlingRights = sections[2];
                whiteCastleKingside = castlingRights.Contains('K');
                whiteCastleQueenside = castlingRights.Contains('Q');
                blackCastleKingside = castlingRights.Contains('k');
                blackCastleQueenside = castlingRights.Contains('q');

                // Default values
                epFile = -1;
                fiftyMovePlyCount = 0;
                moveCount = 0;

                if (sections.Length > 3)
                {
                    string enPassantFileName = sections[3][0].ToString();
                    if (BoardUtility.files.Contains(enPassantFileName))
                    {
                        epFile = BoardUtility.files.IndexOf(enPassantFileName);
                    }
                }

                // Half-move clock
                if (sections.Length > 4)
                {
                    int.TryParse(sections[4], out fiftyMovePlyCount);
                }
                // Full move number
                if (sections.Length > 5)
                {
                    int.TryParse(sections[5], out moveCount);
                }
            }
        }
    }
}
