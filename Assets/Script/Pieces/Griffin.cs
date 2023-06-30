using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Griffin : ChessPiece
{
    public override List<Vector2Int> GetMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        var moves = new List<Vector2Int>();
        var rawMoves = new List<Vector2Int>
        {
            new Vector2Int(curX + 1, curY + 1),
            new Vector2Int(curX + 1, curY - 1),
            new Vector2Int(curX - 1, curY + 1),
            new Vector2Int(curX - 1, curY - 1)
        };


        foreach (var move in rawMoves)
        {
            if (move.x >= 0 && move.y >= 0 && move.x<tileCountX && move.y<tileCountY)
            {
                if (board[move.x, move.y] == null || board[move.x, move.y].team != team) {
                    moves.Add(new Vector2Int(move.x, move.y));
                    
                    for (int x = curX + 1; x < tileCountX; x++)
                    {
                        if (board[x, curY] == null) moves.Add(new Vector2Int(x, curY));
                        else
                        {
                            if (board[x, curY].team != team) moves.Add(new Vector2Int(x, curY));
                            break;
                        }
                    }
                    for (int x = curX - 1; x >= 0; x--)
                    {
                        if (board[x, curY] == null) moves.Add(new Vector2Int(x, curY));
                        else
                        {
                            if (board[x, curY].team != team) moves.Add(new Vector2Int(x, curY));
                            break;
                        }
                    }
                    for (int y = curY + 1; y < tileCountY; y++)
                    {
                        if (board[curX, y] == null) moves.Add(new Vector2Int(curX, y));
                        else
                        {
                            if (board[curX, y].team != team) moves.Add(new Vector2Int(curX, y));
                            break;
                        }
                    }
                    for (int y = curY - 1; y >= 0; y--)
                    {
                        if (board[curX, y] == null) moves.Add(new Vector2Int(curX, y));
                        else
                        {
                            if (board[curX, y].team != team) moves.Add(new Vector2Int(curX, y));
                            break;
                        }
                    }
                }
            }
        }
        return moves;
    }


}