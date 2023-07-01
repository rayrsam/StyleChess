using System.Collections.Generic;

public abstract class AKing : ChessPiece
{
    private List<ChessPieceType> _availableLightPieces;
    private List<ChessPieceType> _availableHeavyPieces;

    public static int PowerPointsLimit;
    public static int StylePointsLimit;
    
    public abstract List<ChessPieceType> GetAvailableLight();
    public abstract List<ChessPieceType> GetAvailableHeavy();
}