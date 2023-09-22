using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using static System.Formats.Asn1.AsnWriter;

public class MyBot : IChessBot
{
    bool botIsWhite;
    //Alpha Beta Search at Depth 2
    public Move Think(Board board, Timer timer)
    {
        botIsWhite = board.IsWhiteToMove;

        Move[] moves = board.GetLegalMoves();
        var moveScores = new List<float>();

        foreach (var move in moves)
        {
            board.MakeMove(move);
            float score = alphaBeta(board, isMaximizing: !botIsWhite, -300, 300, depth: 4);
            //Console.WriteLine("Depth: " + 3 + " Score: " + score + " " + move);
            moveScores.Add(score);
            board.UndoMove(move);
        }

        

        for (int i = 0; i < moves.Length; i++)
        {
        //Console.Write(" " + moves[i].ToString().Substring(6) + ": " + moveScores[i] + ",");
        }
        //Console.WriteLine();
        return moves[moveScores.IndexOf(botIsWhite ? moveScores.Max() : moveScores.Min())];
    }

    public float alphaBeta(Board board, bool isMaximizing, float alpha, float beta, int depth)
    {
        
        if (board.IsDraw() || board.IsInCheckmate() || depth == 0)
        {
            //Console.Write(board.CreateDiagram(false, false, false));
            //Console.WriteLine(Evaluation(board));
            return Evaluation(board);
        } 

        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            float score = alphaBeta(board, !isMaximizing, alpha, beta, depth - 1);
            string indent = new string(' ', depth);
            //Console.WriteLine(indent + "Depth: " + depth + " Score: " + score + " " + move);
            board.UndoMove(move);

            if (isMaximizing) {
                if (score >= beta)
                {
                    return beta;
                }
                if (score > alpha)
                {
                    alpha = score;
                }
            }
            else {
                if (score <= alpha) {
                    return alpha;
                }
                if (score < beta) {
                    beta = score;
                }
            }
        }
        return isMaximizing ? alpha : beta;
    }

    public float Evaluation(Board board)
    {
        if (board.IsDraw())
        {
            return 0;
        }
        else if (board.IsInCheckmate())
        {
            return 200.0F * (board.IsWhiteToMove ? -1 : 1);
        }

        // wp, wn, wb, wr, wq, wk, bp, bn, bb, br, bq, bk
        var pieceLists = board.GetAllPieceLists();

        float evaluation = 0.0F;
        evaluation += 1.0F * (pieceLists[0].Count - pieceLists[6].Count); // Pawns
        evaluation += 3.0F * (pieceLists[1].Count - pieceLists[7].Count); // Knights
        evaluation += 3.0F * (pieceLists[2].Count - pieceLists[8].Count); // Bishops
        evaluation += 5.0F * (pieceLists[3].Count - pieceLists[8].Count); // Rooks
        evaluation += 9.0F * (pieceLists[4].Count - pieceLists[10].Count); // Queens

        return evaluation;
    }
}