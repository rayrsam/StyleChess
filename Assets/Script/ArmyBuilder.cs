using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;
using TMPro;

public class ArmyBuilder : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Screens")] 
    [SerializeField] private GameObject piecesScreen;
    [SerializeField] private GameObject piecesList;
    [SerializeField] private GameObject promotionScreen;
    
    [Header("Prefabs && Materials")] 
    [SerializeField] private GameObject[] piecesPrefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private GameObject promotionButtonPrefab;
    [SerializeField] private GameObject builderButtonPrefab;
    
    private Camera _curCamera;
    private Vector3 _bounds;
    
    
    private const int TileCountX = 8;
    private const int TileCountY = 8;
    
    private GameObject[,] _tiles; 
    private ChessPiece[,] _chessPieces;
    
    private ChessPiece _selectedPiece;
    private List<Vector2Int> _selectedMoves = new List<Vector2Int>();
    
    private Vector2Int _curHover = -Vector2Int.one;
    private readonly List<Vector2Int> _hoverMoves = new List<Vector2Int>();
    
    private const int White = 0;
    private const int Black = 1;

    private readonly Army _whiteArmy = new Army();

    private bool _paused;
    private ChessPiece _promoting;
    private bool _isPromoting = false;

    private ChessPieceType _pickedPieceType = 0;
    private Vector2Int _pickedTile = -Vector2Int.one;
    private bool _isBuilding = true;

    private void Awake()
    {
        GenerateTiles(TileCountX, TileCountY);
        SetArmy();
        SpawnPieces();
        CreatePiecesList(0);
    }
    private void Update()
    {
        if (!_curCamera)
        {
            _curCamera = Camera.main;
            return;
        }

        Ray ray = _curCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var info, 100000, LayerMask.GetMask("Tile", "Hover", "Move", "Attack")) &&
            !_paused)
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
                
                if (Input.GetMouseButtonDown(0))
                {
                    if (_isBuilding) InBuildAction(hitPosition);
                    else InTestAction(hitPosition);
                }
            }
            else if (_curHover != -Vector2Int.one && _selectedPiece == null) OnSelectionDiscard();
        }
    }

    

    private void SetArmy()
    {
        List<ChessPieceType> pawnList = new (){
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn,
            ChessPieceType.Pawn
        };
        List <ChessPieceType> figureList = new()
        {
            ChessPieceType.Rook,
            ChessPieceType.Knight,
            ChessPieceType.Bishop,
            ChessPieceType.Queen,
            ChessPieceType.King,
            ChessPieceType.Bishop,
            ChessPieceType.Knight,
            ChessPieceType.Rook
        };
        
        _whiteArmy.SetArmy(pawnList, figureList);
    }

    private void InTestAction(Vector2Int hitPosition)
    {
        if (_selectedPiece == null)
        {
            if (_curHover != -Vector2Int.one && _chessPieces[_curHover.x, _curHover.y] != null)
            {
                if (_chessPieces[_curHover.x, _curHover.y].team == White)
                {
                    _selectedPiece = _chessPieces[_curHover.x, _curHover.y];
                    _selectedMoves = _hoverMoves;
                }
            }
        }
        else
        {
            Move(_selectedPiece, hitPosition.x, hitPosition.y);
            _selectedPiece = null;
            _selectedMoves.ForEach(
                move => _tiles[move.x, move.y].layer = LayerMask.NameToLayer("Tile"));
            _selectedMoves.Clear();
        }
    }
    private void InBuildAction(Vector2Int hitPosition)
    {
        if (hitPosition.y > 1 || hitPosition == -Vector2Int.one) return;

        _pickedTile = new Vector2Int(hitPosition.x, hitPosition.y);
        if (_pickedTile != -Vector2Int.one && _pickedPieceType != 0)
        {
            var tile = _chessPieces[_pickedTile.x, _pickedTile.y];
            if (tile != null) OnPieceDestroy(tile);
            _chessPieces[_pickedTile.x, _pickedTile.y] = SpawnSinglePiece(_pickedPieceType, White);
            PosSingle(_pickedTile.x, _pickedTile.y);
            _pickedTile = -Vector2Int.one;
            _pickedPieceType = 0;
        }
    }
    
    
    //set layers
    private void OnSelectTile(Vector2Int hitPosition)
    {
        _curHover = hitPosition;
        _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
        
        if (_chessPieces[hitPosition.x, hitPosition.y] != null)
            if(_chessPieces[hitPosition.x, hitPosition.y].team == White)
            {
                var moves = _chessPieces[hitPosition.x, hitPosition.y].GetMoves(ref _chessPieces, TileCountX, TileCountY);

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
            _chessPieces[x, 1] = SpawnSinglePiece(_whiteArmy.ArmyPiecesList[0][x], White);
            _chessPieces[x, 0] = SpawnSinglePiece(_whiteArmy.ArmyPiecesList[1][x], White);
            //black
            _chessPieces[x, 6] = SpawnSinglePiece(ChessPieceType.Pawn, Black);
            _chessPieces[x, 7] = SpawnSinglePiece(ChessPieceType.Pawn, Black);
        }
        PosAll();
        
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        var piece = Instantiate(piecesPrefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        
        piece.type = type;
        piece.team = team;

        if (type == ChessPieceType.King)
        {
            _whiteArmy.SetKing((King)piece);
            Debug.Log("Assigning");
        }

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

    private void Move(ChessPiece piece, int x, int y)
    {
        if (_isPromoting) return;
        if (piece.curX == x && piece.curY == y || !_selectedMoves.Contains(new Vector2Int(x, y))) return;

        if (piece.type == ChessPieceType.Pawn)
            if (piece.team == 0 && y == 7 || piece.team == 1 && y == 0)
            {
                Promotion(piece, x, y);
                return;
            }

        MoveTo(piece, x, y);
    }
    private void MoveTo(ChessPiece piece, int x, int y)
    {
        if (_chessPieces[x, y] != null) OnPieceDestroy(_chessPieces[x, y]);
        
        _chessPieces[piece.curX, piece.curY] = null;
        _chessPieces[x, y] = piece;
        PosSingle(x, y);
    }
    private void OnPieceDestroy(ChessPiece piece)
    {
        piece.curX = -1;
        piece.curY = -1;
        piece.alive = 0;

        Destroy(piece.gameObject);
    }
    private void Promotion(ChessPiece pawn, int x, int y)
    {
        promotionScreen.SetActive(true);
        _promoting = pawn;
        var promotionList = _whiteArmy.PromotionList;
        
        for (int i = 0; i < promotionList.Count; i++)
        {
            CreatePromotionButton(i, Enum.GetName(typeof(ChessPieceType), promotionList[i]), x, y);
        }

        _isPromoting = true;
    }
    private void GetPromotedPiece(ChessPieceType pieceType, int x, int y)
    {
        var newPiece = SpawnSinglePiece(pieceType, _promoting.team);
        newPiece.curX = _promoting.curX;
        newPiece.curY = _promoting.curY;
        
        MoveTo(newPiece, x, y);
        
        OnPieceDestroy(_promoting);
        _promoting = null;
        _isPromoting = false;
    }
    

    //UI
    
    private void CreatePromotionButton(int num, string text, int x, int y)
    {
        var offset = num * 50;
        
        var newButton = Instantiate(promotionButtonPrefab, transform);
        newButton.transform.SetParent(promotionScreen.transform);
        newButton.transform.position = promotionScreen.transform.position + new Vector3(0, 350 - offset, 0);
        newButton.transform.rotation = new Quaternion(0, 0, 0, 0);
        
        newButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        
        newButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            GetPromotedPiece((ChessPieceType)(num + 2), x, y);
            
            for (int i = 0; i < promotionScreen.transform.childCount; i++)
                Destroy(promotionScreen.transform.GetChild(i).gameObject);
            
            promotionScreen.SetActive(false);
        } );
    }
    public void CreatePiecesList(int type)
    {
        for (int i = 0; i < piecesList.transform.childCount; i++) 
            Destroy(piecesList.transform.GetChild(i).gameObject);
            
        var king = _whiteArmy.ArmyKing;
        
        List<ChessPieceType> targetPool;
        switch (type)
        {
            case 0:
                targetPool = king.GetAvailablePawns();
                //Debug.Log($"{targetPool.Count}");
                break;
            case 1:
                targetPool = king.GetAvailableLight();
                break;
            case 2:
                targetPool = king.GetAvailableHeavy();
                break;
            default: return;
        }
        
        for (int i = 0; i < targetPool.Count; i++)
        {
            CreateBuilderButton(i, targetPool[i]);
        }
        
    }
    private void CreateBuilderButton(int num, ChessPieceType piece)
    {
        var offset = num * 140;
        
        var newButton = Instantiate(builderButtonPrefab, transform);
        newButton.transform.SetParent(piecesList.transform);
        newButton.transform.position = piecesList.transform.position + new Vector3(0, 330 - offset, 0);
        newButton.transform.rotation = new Quaternion(0, 0, 0, 0);

        newButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            Enum.GetName(typeof(ChessPieceType), piece);
        
        newButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            _pickedPieceType = piece;
        } );
    }
    
    public void Exit()
    {
        SceneManager.LoadScene(0);
    }
}
