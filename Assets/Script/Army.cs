using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army {
    
    public List<int> ArmyPawnList = new List<int>
    {
        (int)ChessPieceType.Pawn,
        (int)ChessPieceType.Pawn,
        (int)ChessPieceType.Pawn,
        (int)ChessPieceType.Pawn,
        (int)ChessPieceType.Pawn,
        (int)ChessPieceType.Pawn,
        (int)ChessPieceType.Pawn,
        (int)ChessPieceType.Pawn
    };
    public List<int> ArmyFigureList = new List<int>
    {
        (int)ChessPieceType.Rook,
        (int)ChessPieceType.Knight,
        (int)ChessPieceType.Bishop,
        (int)ChessPieceType.Queen,
        (int)ChessPieceType.King,
        (int)ChessPieceType.Bishop,
        (int)ChessPieceType.Knight,
        (int)ChessPieceType.Rook
    };
    
    
    public void SetArmy(List<int> pawnList, List<int> figureList)
    {
        ArmyPawnList = pawnList;
        ArmyFigureList = figureList;
    }
}
