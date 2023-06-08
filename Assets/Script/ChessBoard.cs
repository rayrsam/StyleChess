using System;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Serialization;
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
    private List<Vector2Int> _hoverMoves = new List<Vector2Int>();
    
    private Vector3 _bounds;
    private List<ChessPiece> _beatenWhite = new List<ChessPiece>();
    private List<ChessPiece> _beatenBlack = new List<ChessPiece>();
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
        if (Physics.Raycast(ray, out var info, 100000, LayerMask.GetMask("Tile", "Hover", "Move")))
        {
            var hitPosition = LookupTile(info.transform.gameObject);

            if (_selectedPiece == null)
            {
                if (_curHover == -Vector2Int.one) OnSelectTile(hitPosition);
                if (_curHover != hitPosition) OnSelectionChange(hitPosition);
            }

            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log($"x: {hitPosition.x}, y: {hitPosition.y}");
                if (_selectedPiece == null)
                {
                    if (_chessPieces[hitPosition.x, hitPosition.y] != null)
                    {
                        if (_chessPieces[hitPosition.x, hitPosition.y].team == White && _turn == 0 ||
                            _chessPieces[hitPosition.x, hitPosition.y].team == Black && _turn == 1)
                        {
                            _selectedPiece = _chessPieces[hitPosition.x, hitPosition.y];
                            _selectedMoves = _selectedPiece.GetMoves(ref _chessPieces, TileCountX, TileCountY);
                        }
                    }
                }
                else
                {
                    if (MoveTo(_selectedPiece, hitPosition.x, hitPosition.y)) _turn = (_turn + 1) % 2;
                    _selectedPiece = null;
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
            _chessPieces[hitPosition.x, hitPosition.y].GetMoves(ref _chessPieces, TileCountX, TileCountY).ForEach(
                delegate(Vector2Int tile)
                {
                    _tiles[tile.x, tile.y].layer = LayerMask.NameToLayer("Move");
                    _hoverMoves.Add(new Vector2Int(tile.x, tile.y));
                });
    }
    private void OnSelectionChange(Vector2Int hitPosition)
    {
        _tiles[_curHover.x, _curHover.y].layer = LayerMask.NameToLayer("Tile");
        _hoverMoves.ForEach(
            delegate(Vector2Int tile)
                {
                    _tiles[tile.x, tile.y].layer = LayerMask.NameToLayer("Tile");
                });
        _hoverMoves.Clear();
        OnSelectTile(hitPosition);
    }
    private void OnSelectionDiscard()
    {
        _tiles[_curHover.x, _curHover.y].layer = LayerMask.NameToLayer("Tile");
        _hoverMoves.ForEach(
            delegate(Vector2Int tile)
            {
                _tiles[tile.x, tile.y].layer = LayerMask.NameToLayer("Tile");
            });
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
        var tile = new GameObject($"X:{x} Y:{y}");
        tile.transform.parent = transform;

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

        tile.layer = LayerMask.NameToLayer("Tile");
        tile.AddComponent<BoxCollider>();
        
        return tile;
    }
    
    //Spawning
    private void SpawnPieces()
    {
        _chessPieces = new ChessPiece[TileCountX, TileCountY];
        //white
        for (var x = 0; x < TileCountX; x++)
        {
            _chessPieces[x, 1] = SpawnSinglePiece((ChessPieceType)_whiteArmy.ArmyPawnList[x], White);
            _chessPieces[x, 0] = SpawnSinglePiece((ChessPieceType)_whiteArmy.ArmyFigureList[x], White); 
        }

        //black
        for (var x = 0; x < TileCountX; x++)
        {
            _chessPieces[x, 6] = SpawnSinglePiece((ChessPieceType)_blackArmy.ArmyPawnList[x], Black);
            _chessPieces[x, 7] = SpawnSinglePiece((ChessPieceType)_blackArmy.ArmyFigureList[x], Black);
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
        return true;
    }
    private void OnPieceDestroy(int x, int y)
    {
        var piece = _chessPieces[x, y];
        piece.curX = -1;
        piece.curY = -1;
        piece.alive = 0;
        if (piece.team == White) _beatenWhite.Add(piece);
        else _beatenBlack.Add(piece);
        if (piece.type == ChessPieceType.King) CheckMate(piece.team);
        
        piece.transform.localScale = Vector3.zero;
    }

    private ChessPiece Promotion(ChessPiece pawn)
    {
        var newPiece = SpawnSinglePiece(ChessPieceType.Queen, pawn.team);
        Destroy(pawn.gameObject);
        return newPiece;
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
        endScreen.SetActive(false);
        for (int i = 0; i < boardObject.transform.childCount; i++)
            Destroy(boardObject.transform.GetChild(i).gameObject);
        Awake();
        Start();
    }
    public void CheckMate(int team)
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
