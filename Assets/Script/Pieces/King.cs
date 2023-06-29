using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        var moves = new List<Vector2Int>();
        var rawMoves = new List<Vector2Int>
        {
            new Vector2Int(curX + 1, curY),
            new Vector2Int(curX - 1, curY),
            new Vector2Int(curX, curY + 1),
            new Vector2Int(curX, curY - 1),
            new Vector2Int(curX + 1, curY + 1),
            new Vector2Int(curX + 1, curY - 1),
            new Vector2Int(curX - 1, curY + 1),
            new Vector2Int(curX - 1, curY - 1)
        };
        
        
        foreach (var move in rawMoves)
        {
            if (move.x >= 0 && move.y >= 0 && move.x < tileCountX && move.y < tileCountY)
            {
                if (board[move.x, move.y] == null || board[move.x, move.y].team != team) moves.Add(new Vector2Int(move.x, move.y));
            }
        }
        return moves;
    }

    
}
