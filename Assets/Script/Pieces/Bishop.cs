using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        var moves = new List<Vector2Int>();
        
        for (int x = curX + 1, y = curY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (board[x, y] == null) moves.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team) moves.Add(new Vector2Int(x, y));
                break;
            }
        }
        for (int x = curX + 1, y = curY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] == null) moves.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team) moves.Add(new Vector2Int(x, y));
                break;
            }
        }
        for (int x = curX - 1, y = curY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] == null) moves.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team) moves.Add(new Vector2Int(x, y));
                break;
            }
        }
        for (int x = curX - 1, y = curY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null) moves.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team) moves.Add(new Vector2Int(x, y));
                break;
            }
        }
        
        return moves;
    }

    
}
