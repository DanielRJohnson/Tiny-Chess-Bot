using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using static System.Formats.Asn1.AsnWriter;

public class MyBot : IChessBot
{
    bool botIsWhite;

    string[] pawnPieceSquareTable = "0,0,0,0,0,0,0,0,5,10,10,-20,-20,10,10,5,5,-5,-10,0,0,-10,-5,5,0,0,0,20,20,0,0,0,5,5,10,25,25,10,5,5,10,10,20,30,30,20,10,10,50,50,50,50,50,50,50,50,0,0,0,0,0,0,0,0".Split(',');


    //Alpha Beta Search at Depth 2
    public Move Think(Board board, Timer timer)
    {
        botIsWhite = board.IsWhiteToMove;

        Move[] moves = board.GetLegalMoves();
        var moveScores = new List<float>();

        foreach (var move in moves)
        {
            board.MakeMove(move);
            Console.WriteLine("Finding score for: " + move.ToString() + "is Maximizing is " + !botIsWhite);
            float score = alphaBeta(board, isMaximizing: !botIsWhite, -300, 300, depth: 3);

            //if (move.MovePieceType == PieceType.Pawn)
            //{
            //    //add to score the difference between the value of the pawn at the old value compared to the new value
            //    Console.WriteLine(pawnPieceSquareTable[move.TargetSquare.Index] + " " + pawnPieceSquareTable[move.StartSquare.Index]);
            //    score += 0.01f * (int.Parse(pawnPieceSquareTable[move.TargetSquare.Index]) - int.Parse(pawnPieceSquareTable[move.StartSquare.Index]));
            //}
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

        if (isMaximizing)
        {
            float value = -300;

            foreach (var move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                value = Math.Max(value, alphaBeta(board, false, alpha, beta, depth - 1));
                board.UndoMove(move);
                alpha = Math.Max(alpha, value);
                if (value >= beta)
                {
                    break;
                }
            }
            return value;
        }

        else
        {
            float value = 300;

            foreach (var move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                value = Math.Min(value, alphaBeta(board, true, alpha, beta, depth - 1));
                board.UndoMove(move);
                beta = Math.Min(beta, value);
                if (value <= alpha)
                {
                    break;
                }
            }
            return value;

        }

    //    foreach (var move in board.GetLegalMoves())
    //    {
    //        board.MakeMove(move);
    //        //Console.WriteLine("Finding score for: " + move.ToString() + "is Maximizing is " + isMaximizing);
    //        float score = alphaBeta(board, !isMaximizing, alpha, beta, depth - 1);
    //        string indent = new string(' ', depth);

    //       // Console.WriteLine(indent + "Depth: " + depth + " Score: " + score + " " + move);

    //        if (depth > 1)
    //        {
    //            //Console.ReadLine();
    //        }

    //        board.UndoMove(move);

    //        if (isMaximizing) {
    //            if (score >= beta)
    //            {
    //                return beta;
    //            }
    //            if (score > alpha)
    //            {
    //                alpha = score;
    //            }
    //        }
    //        else {
    //            if (score <= alpha) {
    //                return alpha;
    //            }
    //            if (score < beta) {
    //                beta = score;
    //            }
    //        }
    //    }
    //    return isMaximizing ? alpha : beta;
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