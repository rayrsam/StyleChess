using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class King : AKing
{
    private List<ChessPieceType> _availablePawns = new() { ChessPieceType.Pawn };
    private new List<ChessPieceType> _availableLightPieces = new()
    {
        ChessPieceType.Rook,
        ChessPieceType.Knight,
        ChessPieceType.Bishop
    };
    private new List<ChessPieceType> _availableHeavyPieces = new(){ ChessPieceType.Queen };
    
    public new static int PowerPointsLimit;
    public new static int StylePointsLimit;

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


    public override List<ChessPieceType> GetAvailablePawns()
    {
        return _availablePawns;
    }

    public override List<ChessPieceType> GetAvailableLight()
    {
        return _availableLightPieces;
    }

    public override List<ChessPieceType> GetAvailableHeavy()
    {
        return _availableHeavyPieces;
    }
}
