using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army
{

    public King ArmyKing;

    public List<List<int>> ArmyPiecesList = new()
    {
        new List<int>
        {
            (int)ChessPieceType.Pawn,
            (int)ChessPieceType.Pawn,
            (int)ChessPieceType.Pawn,
            (int)ChessPieceType.Pawn,
            (int)ChessPieceType.Pawn,
            (int)ChessPieceType.Pawn,
            (int)ChessPieceType.Pawn,
            (int)ChessPieceType.Pawn
        },
        new List<int>
        {
            (int)ChessPieceType.Rook,
            (int)ChessPieceType.Knight,
            (int)ChessPieceType.Bishop,
            (int)ChessPieceType.Queen,
            (int)ChessPieceType.King,
            (int)ChessPieceType.Bishop,
            (int)ChessPieceType.Knight,
            (int)ChessPieceType.Rook
        }
    };
    
    public List<int> PromotionList = new List<int>
    {
        (int)ChessPieceType.Rook,
        (int)ChessPieceType.Knight,
        (int)ChessPieceType.Bishop,
        (int)ChessPieceType.Queen,
    };
    
    
    public void SetArmy(King king, List<int> pawnList, List<int> figureList)
    {
        ArmyKing = king;
        ArmyPiecesList = new List<List<int>>() { pawnList, figureList };
    }
}
