using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class TableEntry
{
    public ulong zobristKey;
    public Move chosenMove;
    public float chosenMoveEvaluation;
    public int depth;

    public TableEntry(ulong key, Move move, float eval, int _depth)
    {
        zobristKey = key;
        chosenMove = move;
        chosenMoveEvaluation = eval;
        depth = _depth;
    }
}

public class MyBot : IChessBot
{
    bool botIsWhite;
    Move bestMove;
    int numPositionsVisited;
    int numPositionsVisitedTotal;

    const int numTTEntries = 1 << 20;
    TableEntry[] tTable = new TableEntry[numTTEntries];

    public Move Think(Board board, Timer timer)
    {
        botIsWhite = board.IsWhiteToMove;
        numPositionsVisited = 0;

        var bestScore = -30000.0F * (botIsWhite ? 1 : -1);
        bestMove = Move.NullMove;

        int depth;
        for (depth = 1; depth <= 50; depth++)
        {
            float score = Minimax(board, timer, botIsWhite, depth, 0);

            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
                break;
        }

        Console.WriteLine("Positions Evaluated: {0} Time to Move: {1} (seconds) TT size: {2} Max depth: {3}",
            numPositionsVisited, timer.MillisecondsElapsedThisTurn / 1000.0, numPositionsVisitedTotal, depth);

        return bestMove.IsNull ? board.GetLegalMoves()[0] : bestMove;
    }

    public float Minimax(Board board, Timer timer, bool isMaximizing, int depth, int ply)
    {
        var playerSign = isMaximizing ? 1 : -1;
        //if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
        //    return 30000 * playerSign; // Timed out, assure this path isn't taken

        ulong key = board.ZobristKey;
        TableEntry entry = tTable[key % numTTEntries];

        if (entry is not null && entry.zobristKey == key && entry.depth >= depth)
        {
            return entry.chosenMoveEvaluation;
        }

        numPositionsVisited += 1;
        numPositionsVisitedTotal += 1;

        if (depth == 0) return Evaluation(board);

        if (board.IsRepeatedPosition()) return 0;

        if (board.IsInCheckmate())
        {
            if (board.IsInCheck()) return -30000 * playerSign; // bot is mated, bad
            else return 30000 * playerSign; // opponent is mated, good
        }
      
        var scores = new List<float>();
        var moves = board.GetLegalMoves();

        foreach (var move in moves)
        {
            board.MakeMove(move);
            scores.Add(Minimax(board, timer, !isMaximizing, depth-1, ply+1));
            board.UndoMove(move);
        }

        var chosenScore = isMaximizing ? scores.Max() : scores.Min();
        var chosenMove = moves[scores.IndexOf(chosenScore)];
        if (ply == 0)
            bestMove = chosenMove;
        tTable[key % numTTEntries] = new TableEntry(key, chosenMove, chosenScore, depth);

        return chosenScore;
    }

    public float Evaluation(Board board)
    {
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