using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

public class ChessBoard : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private GameObject boardObject;
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject endScreen;
    

        [Header("Prefabs && Materials")] 
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;


    private const int TileCountX = 8;
    private const int TileCountY = 8;
    
    private ChessPiece[,] _chessPieces;
    
    private ChessPiece _selectedPiece;
    private List<Vector2Int> _selectedMoves = new List<Vector2Int>();
    
    private GameObject[,] _tiles; 
    private Camera _curCamera;
    private Vector2Int _curHover;
    private readonly List<Vector2Int> _hoverMoves = new List<Vector2Int>();
    
    private Vector3 _bounds;
    private List<ChessPiece> _whitePieces = new List<ChessPiece>();
    private List<ChessPiece> _blackPieces = new List<ChessPiece>();
    private const int White = 0;
    private const int Black = 1;
    private int _turn = -1;
    
    private readonly Army _whiteArmy = new Army();
    private readonly Army _blackArmy = new Army();
    
    private void Awake()
    {
        GenerateTiles(TileCountX, TileCountY);
        SpawnPieces();
    }
    private void Update()
    {
        if (_turn == -1) return;
        if (!_curCamera) //|| !boardActive)
        {
            _curCamera = Camera.main;
            return;
        }
        Ray ray = _curCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var info, 100000, LayerMask.GetMask("Tile", "Hover", "Move", "Attack")))
        {
            var hitPosition = LookupTile(info.transform.gameObject);

            if (_selectedPiece == null)
            {
                if (_curHover == -Vector2Int.one) OnSelectTile(hitPosition);
                if (_curHover != hitPosition)
                {
                    OnSelectionDiscard();
                    OnSelectTile(hitPosition);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log($"x: {hitPosition.x}, y: {hitPosition.y}");
                if (_selectedPiece == null)
                {
                    if (_curHover != -Vector2Int.one && _chessPieces[_curHover.x, _curHover.y] != null)
                    {
                        if (_chessPieces[_curHover.x, _curHover.y].team == White && _turn == 0 ||
                            _chessPieces[_curHover.x, _curHover.y].team == Black && _turn == 1)
                        {
                            _selectedPiece = _chessPieces[_curHover.x, _curHover.y];
                            _selectedMoves = _hoverMoves;
                        }
                    }
                }
                else
                {
                    if (MoveTo(_selectedPiece, hitPosition.x, hitPosition.y)) _turn = (_turn + 1) % 2;
                    CheckForCheckMate(_selectedPiece.team);
                    _selectedPiece = null;
                    _selectedMoves.ForEach(
                        move => _tiles[move.x, move.y].layer = LayerMask.NameToLayer("Tile"));
                    _selectedMoves.Clear();
                }
            }
        }
        else if (_curHover != -Vector2Int.one && _selectedPiece == null) OnSelectionDiscard();
        
    }
    
    //set layers
    private void OnSelectTile(Vector2Int hitPosition)
    {
        _curHover = hitPosition;
        _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
        
        if (_chessPieces[hitPosition.x, hitPosition.y] != null)
        {
            var moves = _chessPieces[hitPosition.x, hitPosition.y].GetMoves(ref _chessPieces, TileCountX, TileCountY);
            moves = PreventCheck(_chessPieces[hitPosition.x, hitPosition.y], moves);
                
            moves.ForEach(
                move => {
                    _tiles[move.x, move.y].layer = LayerMask.NameToLayer(_chessPieces[move.x, move.y] != null ? "Attack" : "Move");
                    _hoverMoves.Add(new Vector2Int(move.x, move.y));
                });
        }
            
    }
    private void OnSelectionDiscard()
    {
        _tiles[_curHover.x, _curHover.y].layer = LayerMask.NameToLayer("Tile");
        _hoverMoves.ForEach(
            move => _tiles[move.x, move.y].layer = LayerMask.NameToLayer("Tile"));
        _hoverMoves.Clear();
        _curHover = -Vector2Int.one;
    }

    //generate
    private void GenerateTiles(int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        _bounds = new Vector3(tileCountX * tileSize / 2, 0, tileCountY * tileSize / 2) + boardCenter;
        
        _tiles = new GameObject[tileCountX, tileCountY];
        for (var x = 0; x < tileCountX; x++) for (var y = 0; y < tileCountY; y++) _tiles[x, y] = GenerateSingleTile(x, y);
    }
    private GameObject GenerateSingleTile(int x, int y)
    {
        var tile = new GameObject($"X:{x} Y:{y}")
        {
            transform =
            {
                parent = transform
            },
            layer = LayerMask.NameToLayer("Tile")
        };

        var mesh = new Mesh();
        tile.AddComponent<MeshFilter>().mesh = mesh;
        tile.AddComponent<MeshRenderer>().material = tileMaterial;
        
        var vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - _bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - _bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - _bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - _bounds;
        
        var tris = new[] {0, 1, 2, 1, 3, 2};
        
        mesh.vertices = vertices;
        mesh.triangles = tris;
        
        mesh.RecalculateNormals();

        
        tile.AddComponent<BoxCollider>();
        
        return tile;
    }
    
    //Spawning
    private void SpawnPieces()
    {
        _chessPieces = new ChessPiece[TileCountX, TileCountY];
        
        for (var x = 0; x < TileCountX; x++)
        {
            //white
            _chessPieces[x, 1] = SpawnSinglePiece((ChessPieceType)_whiteArmy.ArmyPawnList[x], White);
            _chessPieces[x, 0] = SpawnSinglePiece((ChessPieceType)_whiteArmy.ArmyFigureList[x], White); 
            _whitePieces.Add(_chessPieces[x, 1]);
            _whitePieces.Add(_chessPieces[x, 0]);
            
            //black
            _chessPieces[x, 6] = SpawnSinglePiece((ChessPieceType)_blackArmy.ArmyPawnList[x], Black);
            _chessPieces[x, 7] = SpawnSinglePiece((ChessPieceType)_blackArmy.ArmyFigureList[x], Black);
            _blackPieces.Add(_chessPieces[x, 6]);
            _blackPieces.Add(_chessPieces[x, 7]);
        }
        
        
        PosAll();
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        var piece = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        
        piece.type = type;
        piece.team = team;
        piece.GetComponent<MeshRenderer>().material = teamMaterials[team];
        return piece;
    }
    
    //Pos
    private void PosAll()
    {
        for (int x = 0; x < TileCountX; x++)
            for (int y = 0; y < TileCountY; y++)
                if(_chessPieces[x, y] != null) PosSingle(x, y);
    }
    private void PosSingle(int x, int y)    
    {
        var curPiece = _chessPieces[x, y]; 
        curPiece.curX = x;
        curPiece.curY = y;
        curPiece.transform.position = GetPos(x, y);
    }
    private Vector3 GetPos(int x, int y)
    {
        return new Vector3((x - 3.5f) * tileSize , yOffset, (y-3.5f) * tileSize);
    }

    //Operations
    private Vector2Int LookupTile(Object hitInfo)
    {
        try
        {
            for (var x = 0; x < TileCountX; x++)
            for (var y = 0; y < TileCountY; y++)
                if (_tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
            return -Vector2Int.one; //[-1, -1]
        } catch (Exception) { return -Vector2Int.one;}
    }
    private bool MoveTo(ChessPiece piece, int x, int y)
    {
        if (piece.curX == x && piece.curY == y || !_selectedMoves.Contains(new Vector2Int(x, y))) return false;
        
        if (_chessPieces[x, y] != null) OnPieceDestroy(x, y);
        
        _chessPieces[piece.curX, piece.curY] = null;
        
        if (piece.type == ChessPieceType.Pawn)
            if (piece.team == 0 && y == 7 || piece.team == 1 && y == 0) piece = Promotion(piece);

        _chessPieces[x, y] = piece;
        
        PosSingle(x, y);
        
        //CheckForCheckMate(piece.team);
        
        return true;
    }
    private void OnPieceDestroy(int x, int y)
    {
        var piece = _chessPieces[x, y];
        piece.curX = -1;
        piece.curY = -1;
        piece.alive = 0;
        if (piece.team == White) _whitePieces.Remove(piece);
        else _blackPieces.Remove(piece);;

        piece.transform.localScale = Vector3.zero;
    }
    private ChessPiece Promotion(ChessPiece pawn)
    {
        var newPiece = SpawnSinglePiece(ChessPieceType.Queen, pawn.team);
        Destroy(pawn.gameObject);
        return newPiece;
    }

    private List<Vector2Int> PreventCheck(ChessPiece piece, List<Vector2Int> possibleMoves)
    {
        var targetKing = FindKing(piece.team);
        int x = piece.curX;
        int y = piece.curY;
        int simX;
        int simY;
        
        var unavailableMoves = new List<Vector2Int>();
        Vector2Int targetKingPos;
        ChessPiece[,] sim;
        List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
        
        foreach (var move in possibleMoves)
        {
            simX = move.x;
            simY = move.y;
            
            targetKingPos = piece.type == ChessPieceType.King ? new Vector2Int(simX, simY) : new Vector2Int(targetKing.curX, targetKing.curY);
            
            simAttackingPieces.Clear();
            
            sim = new ChessPiece[TileCountX, TileCountY];
            for (var xTile = 0; xTile < TileCountX; xTile++)
                for (var yTile = 0; yTile < TileCountY; yTile++)
                    if (_chessPieces[xTile, yTile] != null)
                    {
                        sim[xTile, yTile] = _chessPieces[xTile, yTile];
                        if (sim[xTile, yTile].team != piece.team)
                            simAttackingPieces.Add(sim[xTile, yTile]);
                    }

            sim[x, y] = null;
            piece.curX = simX;
            piece.curY = simY;
            sim[simX, simY] = piece;

            var deadPiece = simAttackingPieces.Find(dead => dead.curX == simX && dead.curY == simY);
            if (deadPiece != null) simAttackingPieces.Remove(deadPiece);

            simAttackingPieces.ForEach(
                ap => ap.GetMoves(ref sim, TileCountX, TileCountY).ForEach(
                    m =>
                    {
                        if (m.x == targetKingPos.x && m.y == targetKingPos.y) 
                            unavailableMoves.Add(move);
                    }));
        }

        piece.curX = x;
        piece.curY = y;
        
        foreach (var move in unavailableMoves) possibleMoves.Remove(move);
        return possibleMoves;
    }

    private void CheckForCheckMate(int team) //0 - OK, 1 - StaleMate 2 - CheckMate
    {
        var attackingTeam = new List<ChessPiece>();
        var defendingTeam = new List<ChessPiece>();
        SetTeams(team, ref attackingTeam, ref defendingTeam);

        foreach (var piece in defendingTeam)
        {
            var moves = piece.GetMoves(ref _chessPieces, TileCountX, TileCountY);
            moves = PreventCheck(piece, moves);
            if (moves.Count != 0) return;
        }
        
        var targetKing = FindKing((team + 1) % 2);
        var targetKingPos = new Vector2Int(targetKing.curX, targetKing.curY);
        
        foreach (var piece in attackingTeam)
        {
            var moves = piece.GetMoves(ref _chessPieces, TileCountX, TileCountY);
            if (moves.Contains(targetKingPos))
            {
                CheckMate(team);
                return;
            }
        }
        CheckMate(2);
    }

    private ChessPiece FindKing(int team)
    {
        ChessPiece targetKing = null;
        for (var x = 0; x < TileCountX; x++)
            for (var y = 0; y < TileCountY; y++)
                if (_chessPieces[x, y] != null)
                    if (_chessPieces[x, y].type == ChessPieceType.King &&
                        _chessPieces[x, y].team == team)
                    {
                        targetKing = _chessPieces[x, y];
                        return targetKing;
                    }
        return targetKing;
    }

    private void SetTeams(int attackingTeam, ref List<ChessPiece> attacking, ref List<ChessPiece> defending)
    {
        if (attackingTeam == White)
        {
            attacking = _whitePieces;
            defending = _blackPieces;
        }
        else
        {
            attacking = _blackPieces;
            defending = _whitePieces;
        }
    }

    //UI
    public void Start()
    {
        boardObject.SetActive(true);
        startScreen.SetActive(false);
        _turn = 0;
    }
    
    public void Reset()
    {
        endScreen.transform.GetChild(0).gameObject.SetActive(false);
        endScreen.transform.GetChild(1).gameObject.SetActive(false);
        endScreen.transform.GetChild(2).gameObject.SetActive(false);
        endScreen.SetActive(false);
        
        for (int i = 0; i < boardObject.transform.childCount; i++)
            Destroy(boardObject.transform.GetChild(i).gameObject);
        
        Awake();
        Start();
    }

    private void CheckMate(int team)
    {
        _turn = -1;
        boardObject.SetActive(false);
        endScreen.SetActive(true);
        endScreen.transform.GetChild(team).gameObject.SetActive(true);
    }
    
    public void Exit()
    {
        Application.Quit();
    }
}
