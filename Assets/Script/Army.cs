using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army
{
    public AKing ArmyKing;

    public List<List<ChessPieceType>> ArmyPiecesList = new()
    {
        new List<ChessPieceType>
        {
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn
        },
        new List<ChessPieceType>
        {
            ChessPieceType.Rook,
            ChessPieceType.Knight,
            ChessPieceType.Bishop,
            ChessPieceType.Queen,
            ChessPieceType.King,
            ChessPieceType.Bishop,
            ChessPieceType.Knight,
            ChessPieceType.Rook
        }
    };
    
    public List<ChessPieceType> PromotionList = new()
    {
        ChessPieceType.Rook,
        ChessPieceType.Knight,
        ChessPieceType.Bishop,
        ChessPieceType.Queen,
    };
    
    
    public void SetArmy(List<ChessPieceType> pawnList, List<ChessPieceType> figureList)
    {
        ArmyPiecesList = new List<List<ChessPieceType>> { pawnList, figureList };
        SetPromotionList();
    }

    public void SetKing(King king)
    {
        ArmyKing = king;
    }

    private void SetPromotionList()
    {
        PromotionList.Clear();
        foreach (var piece in ArmyPiecesList[1])
        {
            if (!PromotionList.Contains(piece) && piece != ChessPieceType.King) PromotionList.Add(piece);
        }
    }
}
