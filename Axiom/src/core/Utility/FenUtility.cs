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
            public readonly int fullCastlingRights;
            // En passant file (0 is a-file, 7 is h-file, -1 means none)
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
                        continue;
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


                fullCastlingRights = (whiteCastleKingside ? 1 : 0)
                                   | (whiteCastleQueenside ? 2 : 0)
                                   | (blackCastleKingside ? 4 : 0)
                                   | (blackCastleQueenside ? 8 : 0);



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

        public static string GetFen(Board.Board board)
        {
            string CastlingRights = "";
            string Position = "";
            string EnPassantSquare;
            string SideToMove;


            // Castling
            bool whiteKingside = (board.CurrentGameState.castlingRights & 1) == 1;
            bool whiteQueenside = ((board.CurrentGameState.castlingRights >> 1) & 1) == 1;
            bool blackKingside = (board.CurrentGameState.castlingRights >> 2 & 1) == 1;
            bool blackQueenside = (board.CurrentGameState.castlingRights >> 3 & 1) == 1;

            CastlingRights += (whiteKingside) ? "K" : "";
            CastlingRights += (whiteQueenside) ? "Q" : "";
            CastlingRights += (blackKingside) ? "k" : "";
            CastlingRights += (blackQueenside) ? "q" : "";
            CastlingRights += ((board.CurrentGameState.castlingRights) == 0) ? "-" : "";


            SideToMove = board.WhiteToMove ? "w" : "b";

            if (board.CurrentGameState.enPassantFile != -1)
            {
                if (board.WhiteToMove)
                {
                    EnPassantSquare = BoardUtility.NameOfSquare(board.CurrentGameState.enPassantFile + 8 * 2);
                }
                else
                {
                    EnPassantSquare = BoardUtility.NameOfSquare(board.CurrentGameState.enPassantFile + 8 * 5);
                }
            }
            else
            {
                EnPassantSquare = "-";
            }
            
            
            int absIndex = 0;
            int addIndex = 0;
            bool hasSeenPiece = false;
            for (int rank = 0; rank < 8; rank++)
            {
                addIndex = 0;
                for (int file = 0; file < 8; file++)
                {
                    if (board.Squares[absIndex] != 0)
                    {

                        if (addIndex != 0)
                        {
                            Position += addIndex.ToString();
                        }

                        hasSeenPiece = true;
                        Position += Piece.GetSymbol(board.Squares[absIndex]);
                        addIndex = 0;
                    }
                    else
                    {
                        addIndex++;
                    }
                    absIndex++;
                    if (file == 7)
                    {
                        if (!hasSeenPiece)
                        {
                            Position += "8";
                        }
                        else if (addIndex != 0)
                        {
                            Position += addIndex.ToString();
                        }
                    }

                }
                hasSeenPiece = false;
                if (rank != 7)
                {
                    Position += "/";
                }
            }
            return Position + " " + SideToMove + " " + CastlingRights + " " + EnPassantSquare + " 0 1";
        }
    }
}
