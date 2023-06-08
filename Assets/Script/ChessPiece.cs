using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public abstract class ChessPiece : MonoBehaviour
{
    public int team; // 0 = white, 1 = black
    public int curX;
    public int curY;
    public ChessPieceType type;
    public int alive = 1;
    
    private Vector3 _desiredPos;
    private Vector3 _desiredScale;

    public abstract List<Vector2Int> GetMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY);
}
