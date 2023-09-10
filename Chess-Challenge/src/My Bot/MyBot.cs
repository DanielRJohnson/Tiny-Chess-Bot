using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using System.Diagnostics;

public class MyBot : IChessBot
{
    bool botIsWhite;

    public Move Think(Board board, Timer timer)
    {
        botIsWhite = board.IsWhiteToMove;

        // Console.WriteLine(Evaluation(board));
        Move[] moves = board.GetLegalMoves();
        var moveScores = new List<float>();

        foreach (var move in moves)
        {
            board.MakeMove(move);
            moveScores.Add(Minimax(board, isMaximizing: !botIsWhite, depth: 3));
            board.UndoMove(move);
        }

        for (int i = 0; i < moves.Length; i++)
        {
            Console.Write(" " + moves[i].ToString().Substring(6) + ": " + moveScores[i] + ",");
        }
        // Console.WriteLine();
        return moves[moveScores.IndexOf(botIsWhite ? moveScores.Max() : moveScores.Min())];
    }

    public float Minimax(Board board, bool isMaximizing, int depth)
    {
        if (board.IsDraw() || board.IsInCheckmate() || depth == 0)
        {
            return Evaluation(board);
        }

        var scores = new List<float>();
        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            scores.Add(Minimax(board, !isMaximizing, depth-1));
            board.UndoMove(move);
        }
        return isMaximizing ? scores.Max() : scores.Min();
    }

    public float Evaluation(Board board)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        if (board.IsDraw())
        {
            return 0.0F;
        }
        else if (board.IsInCheckmate())
        {
            return 200.0F * (botIsWhite ? 1 : -1);
        }

        // wp, wn, wb, wr, wq, wk, bp, bn, bb, br, bq, bk
        var pieceLists = board.GetAllPieceLists();

        float evaluation = 0.0F;
        evaluation += 1.0F * (pieceLists[0].Count - pieceLists[6].Count); // Pawns
        evaluation += 3.0F * (pieceLists[1].Count - pieceLists[7].Count); // Knights
        evaluation += 3.0F * (pieceLists[2].Count - pieceLists[8].Count); // Bishops
        evaluation += 5.0F * (pieceLists[3].Count - pieceLists[8].Count); // Rooks
        evaluation += 9.0F * (pieceLists[4].Count - pieceLists[10].Count); // Queens

        stopwatch.Stop();

        // Get the elapsed time in milliseconds
        TimeSpan elapsedTime = stopwatch.Elapsed;
        double milliseconds = elapsedTime.TotalMilliseconds;

       // Console.WriteLine($"Elapsed Time during start of evaluation: {milliseconds} ms");


        stopwatch.Reset();
        if (board.IsWhiteToMove)
        {
            evaluation += 0.1F * board.GetLegalMoves().Length;
            board.ForceSkipTurn();
            evaluation -= 0.1F * board.GetLegalMoves().Length;
            board.UndoSkipTurn();
        }
        else
        {
            
            evaluation -= 0.1F * board.GetLegalMoves().Length;
            
            board.ForceSkipTurn();
            evaluation += 0.1F * board.GetLegalMoves().Length;
            board.UndoSkipTurn();
        }

        // Get the elapsed time in milliseconds
        elapsedTime = stopwatch.Elapsed;
        milliseconds = elapsedTime.TotalMilliseconds;

        //Console.WriteLine($"Elapsed Time during mobility evaluation: {milliseconds} ms");

        return evaluation;
    }
}