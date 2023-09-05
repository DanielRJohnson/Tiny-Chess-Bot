using ChessChallenge.API;
using System.Numerics;
using System.Collections.Generic;
using System;


public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();
        Dictionary<Move, int> moveVals = new Dictionary<Move, int>();

        foreach (Move move in board.GetLegalMoves())
        {
            moveVals.Add(move, 0);
        }

        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];

        foreach (Move move in allMoves)
        {
            // Always play checkmate in one
            if (MoveIsCheckmate(board, move))
            {
                return move;
            }

            // Find highest value capture
            moveVals[move] += pieceValues[(int)board.GetPiece(move.TargetSquare).PieceType];

            board.MakeMove(move);

            foreach (Move oppMove in board.GetLegalMoves())
            {
                // Always avoid
                if (MoveIsCheckmate(board, oppMove))
                {
                    moveVals[move] = moveVals[move] - 100000000;
                }

                // If losing a piece on the next move, subtract the lost piece's value
                moveVals[move] += -pieceValues[(int)board.GetPiece(oppMove.TargetSquare).PieceType];

                
            }

            board.UndoMove(move);
        }

        int maxMoveVal = 0;

        foreach (KeyValuePair<Move, int> ele in moveVals)
        {
            if (ele.Value > maxMoveVal)
            {
                moveToPlay = ele.Key;
                maxMoveVal = ele.Value;
            }
        }
        return moveToPlay;
    }

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }
}
