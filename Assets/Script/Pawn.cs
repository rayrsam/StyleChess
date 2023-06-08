using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        var moves = new List<Vector2Int>();

        var direction = (team == 0) ? 1 : -1;

        if (curY < tileCountY - 1 && curY > 0)
        {
            if (board[curX, curY + direction] == null)
            {
                moves.Add(new Vector2Int(curX, curY + direction));
                if ((team == 0 && curY == 1 || team == 1 && curY == 6) && board[curX, curY + 2 * direction] == null)
                    moves.Add(new Vector2Int(curX, curY + 2 * direction));
            }

            if (curX != tileCountX - 1)
                if (board[curX + 1, curY + direction] != null )
                    if (board[curX + 1, curY + direction].team != team) 
                        moves.Add(new Vector2Int(curX + 1, curY + direction));

            if (curX != 0)
                if (board[curX - 1, curY + direction] != null)
                    if (board[curX - 1, curY + direction].team != team)
                        moves.Add(new Vector2Int(curX - 1, curY + direction));
        }

        return moves;
    }
    
}
