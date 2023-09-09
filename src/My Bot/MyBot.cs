using System.Runtime.InteropServices;
using System.Security.Cryptography;
using ChessChallenge.API;
using ChessChallenge.Application.APIHelpers;
using System;
using System.Numerics;

public class MyBot : IChessBot
{
    // ======================== DIETRICH-1 ========================
    //
    //         A basic (crappy) minimax chess bot
    //         Likes: brute force, breadth-first searching
    //         Hates: Evilbot, losing
    //
    // ============================================================
    public Move Think(Board board, Timer timer)
    {
        // score values taken from evilbot, except for kings which are now worth infinite points
        // Kings are never really captured, so positive infinity is there just to remind
        // that positive infinity will be returned if a move is checkmate.
        // 
        // scores in minimax are returned in a (float, float) tuple with order (white, black)
        // fields are not named to save token space
        // the move with the minimum score for the opposing team is picked
        // and if multiple moves have the same minimum score, the move with the highest score
        // for the bot's team is picked
        float[] pieceValues = { 0, 100f, 300f, 300f, 500f, 900f, float.PositiveInfinity };

        bool playerIsWhite = board.IsWhiteToMove;

        // selects the recursion depth -- usually times out above 3
        int MAX_DEPTH = 3;

        Move bestMove = Minimax(board);

        return bestMove;

        // save token space
        (float, float) addScores((float, float) score1, (float, float) score2) {
            return (score1.Item1 + score2.Item1, score1.Item2 + score2.Item2);
        }

        // taken from evilbot
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }

        (float, float) RecursiveScore(Board board, Move move, int depth) {
            // base case for when max recursion depth is reached
            if (depth == MAX_DEPTH) {
                return board.IsWhiteToMove ? (EvalMove(board, move), 0f) : (0f, EvalMove(board, move));
            }
            // have to initialize for the next board state, not current
            (float, float) bestScore = board.IsWhiteToMove ? (float.PositiveInfinity, float.NegativeInfinity) : (float.NegativeInfinity, float.PositiveInfinity);
            // eval current state
            // in later iterations we could only evaluate the leaf nodes
            float curMoveScore = EvalMove(board, move);
            // if it's checkmate, no need to eval further
            if (float.IsPositiveInfinity(curMoveScore)) {
                return board.IsWhiteToMove ? (float.PositiveInfinity, 0f) : (0f, float.PositiveInfinity);
            }
            board.MakeMove(move);
            // this is somewhat wasteful with tokens, but i thought it would be more efficient to branch here than
            // putting a ternary statement within a for loop
            (float, float) nextMovesScore;
            if (board.IsWhiteToMove) {
                foreach (Move next_move in board.GetLegalMoves()) {
                    // final scores are from all the possible pieces that can be taken
                    nextMovesScore = addScores((0f, curMoveScore) , RecursiveScore(board, next_move, depth + 1));
                    if (nextMovesScore.Item1 > bestScore.Item1) {
                        bestScore = nextMovesScore;
                    }
                    if (float.IsPositiveInfinity(nextMovesScore.Item1)) {
                        board.UndoMove(move);
                        return addScores((0f, curMoveScore), nextMovesScore);
                    }
                }
            }

            else {
                foreach (Move next_move in board.GetLegalMoves()) {
                    nextMovesScore = addScores((curMoveScore, 0f) , RecursiveScore(board, next_move, depth + 1));
                    if (nextMovesScore.Item2 > bestScore.Item2) {
                        bestScore = nextMovesScore;
                    }
                    if (float.IsPositiveInfinity(nextMovesScore.Item2)) {
                        board.UndoMove(move);
                        return addScores((curMoveScore, 0f), nextMovesScore);
                    }
                }
            }

            board.UndoMove(move);
            return bestScore;
        } 

        // can be replaced with some other heuristic later
        float EvalMove(Board board, Move move) {
            float score = 0f;
            if (MoveIsCheckmate(board, move)) {return float.PositiveInfinity;}
            if (move.IsPromotion) {
                    score = pieceValues[(int)move.PromotionPieceType];
                }
            if (!move.IsCapture) {
                return score;
            }
            else {
                return score + pieceValues[(int)move.CapturePieceType];
            }
        }

        Move Minimax(Board board) {
            Random rng = new();
            Move[] allMoves = board.GetLegalMoves();
            // just in case...
            Move bestMove = allMoves[rng.Next(allMoves.Length)];
            (float, float) bestMoveScore = playerIsWhite ? (float.NegativeInfinity, float.PositiveInfinity) : (float.PositiveInfinity, float.NegativeInfinity);
            foreach (Move move in allMoves) {
                if (MoveIsCheckmate(board,move)) {
                    return move;
                }
                (float, float) curMoveScore = RecursiveScore(board, move, 0);
                if (playerIsWhite) {
                    if (curMoveScore.Item2 < bestMoveScore.Item2 || 
                        (curMoveScore.Item2 ==  bestMoveScore.Item2 && curMoveScore.Item1 >  bestMoveScore.Item1)) {
                        bestMove = move;
                        bestMoveScore = curMoveScore;
                    }
                }
                else {
                    if (curMoveScore.Item1 < bestMoveScore.Item1 || 
                        (curMoveScore.Item1 ==  bestMoveScore.Item1 && curMoveScore.Item2 >  bestMoveScore.Item2)) {
                        bestMove = move;
                        bestMoveScore = curMoveScore;
                    }
                }
            }
            return bestMove;
        }
    }
}