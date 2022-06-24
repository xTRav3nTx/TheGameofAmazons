using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] public int _width;
    [SerializeField] public int _height;
    [SerializeField] private float pieceOffset;
    [SerializeField] private float fireOffset;
    [SerializeField] private float dragOffset;

    public bool isWhiteTurn;
    private bool readytoFire = false;

    public int whiteTeam = 0;
    public int blackTeam = 1;

    private bool gameover;

    public List<Vector2Int> availableMoves = new List<Vector2Int>();
    public List<Vector2Int> availableFires = new List<Vector2Int>();
    public List<Vector2Int> AIavailableMoves = new List<Vector2Int>();
    public List<Vector2Int> AIavailableFires = new List<Vector2Int>();

    [SerializeField] private GameObject AIPrefab;
    private AI aiplayer;
    private Amazons ai_amazon;
    
    [SerializeField] private Camera _cam;
    [SerializeField] private GameObject _boardcontainer;
    [SerializeField] private GameObject playercontainer;
    [SerializeField] private GameObject firecontainer;

    public Amazons[,] _amazons;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Material[] playerMaterials;

    public Fire[,] firePiece;
    private bool waitforFire;
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private Material fireMaterial;

    private GameObject[,] _board;
    private Vector2Int mousehover;
    private Amazons movingPiece;

    [SerializeField] private Material tilematerialWhite;
    [SerializeField] private Material tilematerialBlack;

    [SerializeField] private Text[] winner_text;

    void Start()
    {
        gameover = false;
        winner_text[0].gameObject.SetActive(false);
        winner_text[1].gameObject.SetActive(false);

        if (Main_Menu.isPlayerAI)
        {
            aiplayer = Instantiate(AIPrefab, transform).GetComponent<AI>();
        }
        
        isWhiteTurn = true;
        GenerateGrid(_width, _height);
        spawnAmazons();
        postionAllPieces();
        
        
    }
    // Update is called once per frame
    void Update()
    {
        getWinner();
        if (!_cam)
        {
            _cam = Camera.main;
            return;
        }
        if(!gameover)
        {
            hoverTile();
        }
       
    }
    private void FixedUpdate()
    {
        if(Main_Menu.isPlayerAI)
        {
            if (!isWhiteTurn)
            {
                RandomAITurn();
            }
        }
        
    }

    void destroyStuffonWin()
    {
        GameObject[] destroyFire = GameObject.FindGameObjectsWithTag("Fire");
        GameObject[] destroyPlayer = GameObject.FindGameObjectsWithTag("Amazon");
        GameObject[] destroyTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach(GameObject fire in destroyFire)
        {
            GameObject.Destroy(fire);
        }
        foreach(GameObject player in destroyPlayer)
        {
            GameObject.Destroy(player);
        }
        foreach(GameObject tile in destroyTiles)
        {
            GameObject.Destroy(tile);
        }
    }

   public bool hasFire(int x, int y)
    {
        if (firePiece[x, y] != null)
        {
            return true;
        }
        else return false;
    }

   public bool hasAmazon(int x, int y)
    {
        if (_amazons[x, y] != null)
        {
            return true;
        }
        else return false;
    }

    void RandomAITurn()
    {
        aiplayer.AI_amazons.Clear();
        if (!readytoFire)
        {
            RandomAIMove();
        }
        StartCoroutine(waitforAnimation());
    }
    
    IEnumerator waitforAnimation()
    {
        readytoFire = true;
        yield return new WaitForSeconds(1.5f);
        RandomAIFire();
        StopCoroutine(waitforAnimation());
    }

    void RandomAIMove()
    {
        ai_amazon = aiplayer.selectPiecetoMove(ref _amazons, blackTeam, _width, _height);
        Vector2Int lastPos = new Vector2Int(ai_amazon.CurrentX, ai_amazon.CurrentY);
        AIavailableMoves = ai_amazon.GetAvailableMoves(ref _amazons, ref firePiece, _width, _height);
        if (AIavailableMoves.Count > 0)
        {
            Vector2Int moveTo = AIavailableMoves[Random.Range(0, AIavailableMoves.Count)];
            aiplayer.positionAIpiece(ai_amazon, moveTo.x, moveTo.y);
            _amazons[lastPos.x, lastPos.y] = null;
            _amazons[moveTo.x, moveTo.y] = ai_amazon;
        }
        else
        {
            RandomAIMove();
        }
    }

   void RandomAIFire()
    {
        AIavailableFires = ai_amazon.GetAvailableMoves(ref _amazons, ref firePiece, _width, _height);
        if (AIavailableFires.Count > 0)
        {
            Vector2Int fireTo = AIavailableFires[Random.Range(0, AIavailableFires.Count)];
            positionFire(fireTo.x, fireTo.y);
            isWhiteTurn = true;
            readytoFire = false;
        }
    }

    IEnumerator FireArrow()
    {
        HighlightFire();
        while (true)
        {
            if(Input.GetMouseButtonDown(0))
            {
                bool validFire = FireTo(mousehover.x, mousehover.y);
                if (validFire)
                {
                    positionFire(mousehover.x, mousehover.y);
                    waitforFire = false;
                    isWhiteTurn = !isWhiteTurn;
                    RemoveHighlightFire();
                    break;
                }  
            }
            yield return null;
        }
    }
    bool FireTo(int x, int y)
    {
        if(!ContainsValidMove(ref availableFires, new Vector2(x,y)))
        {
            return false;
        }
        if(firePiece[x, y] != null || _amazons[x, y] != null)
        {
            return false;
        }
        
        return true;
    }
    //spawnFire
    Fire spawnFire()
    {
        Fire fire = Instantiate(firePrefab, transform).GetComponent<Fire>();
        fire.transform.parent = firecontainer.transform;
        fire.GetComponent<MeshRenderer>().material = fireMaterial;
        return fire;
    }
    //positionFire 
    void positionFire(int x, int y, bool force = false)
    {
        firePiece[x, y] = spawnFire();
        firePiece[x, y].transform.position = GetTileCenterFire(x, y);
        
        
    }
    //position player pieces at game start
    void postionAllPieces()
    {
        for(int x = 0; x < _width; x++)
        {
            for(int y = 0; y < _height; y++)
            {
                if(_amazons[x,y] != null)
                {
                    positionPiece(x, y);
                }
            }
        }
        
    }
    //sets starting position for each player piece
    public void positionPiece(int x, int y, bool force = false)
     {
        _amazons[x, y].CurrentX = x;
        _amazons[x, y].CurrentY = y;
        _amazons[x, y].setPosition(GetTileCenter(x, y), force);
     }
    //create offset for player pieces
    public Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x + .5f, y + .5f, pieceOffset);
    }
    //create offset for fire pieces
    Vector3 GetTileCenterFire(int x, int y)
    {
        return new Vector3(x + .5f, y + .5f, fireOffset);
    }
    //spawn all players
    void spawnAmazons()
    {
        firePiece = new Fire[_width, _height];
        _amazons = new Amazons[_width, _height];

        _amazons[0, 2] = singlePiece(whiteTeam);
        _amazons[3, 0] = singlePiece(whiteTeam);
        _amazons[7, 2] = singlePiece(whiteTeam);
        
        _amazons[0, 5] = singlePiece(blackTeam);
        _amazons[4, 7] = singlePiece(blackTeam);
        _amazons[7, 5] = singlePiece(blackTeam);
    }
    //create single player
    Amazons singlePiece(int team)
    {
        
        Amazons amazon = Instantiate(playerPrefab, transform).GetComponent<Amazons>();
        amazon.transform.parent = playercontainer.transform;
        amazon.team = team;
        amazon.GetComponent<MeshRenderer>().material = playerMaterials[team];
        return amazon;
    }
    //tile coords
    Vector2Int lookupTileIndex(GameObject tileinfo)
    {
        for(int x = 0; x < _width; x++)
        {
            for(int y = 0; y < _height; y++)
            {
                if(_board[x,y] == tileinfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return -Vector2Int.one;
    }
   //create white tiles
    private GameObject GeneratetileWhite(int x, int y)
    {
        GameObject tile = new GameObject(string.Format("({0},{1})", x, y));
        tile.transform.parent = _boardcontainer.transform;
        tile.tag = "Tile";

        Mesh mesh = new Mesh();
        tile.AddComponent<MeshFilter>().mesh = mesh;
        tile.AddComponent<MeshRenderer>().material = tilematerialWhite;



        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x, y);
        vertices[1] = new Vector3(x, y + 1);
        vertices[2] = new Vector3(x + 1, y);
        vertices[3] = new Vector3(x + 1, y + 1);

        int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        tile.AddComponent<BoxCollider>();
        tile.layer = LayerMask.NameToLayer("Tile");
        

        mesh.RecalculateNormals();

        return tile;
    }
   //create black tiles
    private GameObject GeneratetileBlack(int x, int y)
    {
        GameObject tile = new GameObject(string.Format("({0},{1})", x, y));
        tile.transform.parent = _boardcontainer.transform;
        tile.tag = "Tile";

        Mesh mesh = new Mesh();
        tile.AddComponent<MeshFilter>().mesh = mesh;
        tile.AddComponent<MeshRenderer>().material = tilematerialBlack;
        


        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x, y);
        vertices[1] = new Vector3(x, y + 1);
        vertices[2] = new Vector3(x + 1, y);
        vertices[3] = new Vector3(x + 1, y + 1);

        int[] triangles = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        tile.AddComponent<BoxCollider>();
        tile.layer = LayerMask.NameToLayer("Tile");
        

        mesh.RecalculateNormals();

        return tile;
    }
    //create board
    void GenerateGrid(int tileCountX, int tileCountY)
    {
        _cam.transform.position = new Vector3(_width / 2, _height / 2, -8f);
        _board = new GameObject[tileCountX, tileCountY]; 
        
        for(int x = 0; x < _width; x++)
        {
            for(int y = 0; y < _height; y++)
            {
                if(x % 2 == 0)
                {
                    if(y % 2 == 0)
                    {
                        _board[x, y] = GeneratetileWhite(x, y);
                    }
                    else
                    {
                        _board[x, y] = GeneratetileBlack(x, y);
                    }
                }
                else
                {
                    if (y % 2 == 0)
                    {
                        _board[x, y] = GeneratetileBlack(x, y);
                    }
                    else
                    {
                        _board[x, y] = GeneratetileWhite(x, y);
                    }
                }
            }
        }
    }
    //highlights mouse over tile
    void hoverTile()
    {
        RaycastHit info;
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight","HighlightFire")))
        {
            Vector2Int hitposition = lookupTileIndex(info.transform.gameObject);
            //moving from offboard to tile
            if (mousehover == -Vector2Int.one)
            {
                mousehover = hitposition;
                _board[hitposition.x, hitposition.y].layer = LayerMask.NameToLayer("Hover");
            }
            //moving from tile to tile
            if (mousehover != hitposition)
            {
                changeLayerAfterHover();
                mousehover = hitposition;
                _board[hitposition.x, hitposition.y].layer = LayerMask.NameToLayer("Hover");
            }
            if(!waitforFire)
            {
                //click down on mouse
                OnMouseDown(hitposition.x, hitposition.y);
            }
            //releasing click
            OnMouseUp(hitposition.x, hitposition.y);
        }
        //from board to offboard
        else
        {
            if (mousehover != -Vector2Int.one)
            {
                changeLayerAfterHover();
                mousehover = -Vector2Int.one;
            }
            if(movingPiece && Input.GetMouseButtonUp(0))
            {
                movingPiece.setPosition(GetTileCenter(movingPiece.CurrentX, movingPiece.CurrentY));
                movingPiece = null;
                RemoveHighlightTiles();
            }
        }
        dragPlayer(ray);   
    }
    //decides if tile being moved too is part of a valid move
    public bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for(int i = 0; i < moves.Count; i++)
        {
            if(moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }
    //highlights valid moves 
    void HighlightTiles()
    {
        for(int i = 0; i < availableMoves.Count; i++)
        {
            _board[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            _board[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
    }
    //highlight valid fires
    void HighlightFire()
    {
        for (int i = 0; i < availableFires.Count; i++)
        {
            _board[availableFires[i].x, availableFires[i].y].layer = LayerMask.NameToLayer("HighlightFire");
        }
    }
    void RemoveHighlightFire()
    {
        for (int i = 0; i < availableFires.Count; i++)
        {
            _board[availableFires[i].x, availableFires[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableFires.Clear();
    }
    //decides if tile being moved too is occupied or not
    public bool MoveTo(Amazons movingPiece, int x, int y)
    {
        //only move to valid highlighted areas
        if(!ContainsValidMove(ref availableMoves, new Vector2(x,y)))
        {
            return false;
        }

        //is tile occupied  
        if(_amazons[x,y] != null || firePiece[x,y] != null)
        {
            return false;
        }
        //not occupied
        Vector2Int lastPos = new Vector2Int(movingPiece.CurrentX, movingPiece.CurrentY);
        _amazons[x, y] = movingPiece;
        _amazons[lastPos.x, lastPos.y] = null;

        positionPiece(x, y);

        return true;
    }
    //current win condition...no movement possible
    void getWinner()
    {
        int whitecount = 0;
        int blackcount = 0;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_amazons[x, y] != null)
                {
                    _amazons[x,y].currentmoves = _amazons[x, y].GetAvailableMoves(ref _amazons, ref firePiece, _width, _height);
                }
            }
        }

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_amazons[x, y] != null)
                {
                    if(_amazons[x,y].team == 0 && _amazons[x,y].currentmoves.Count == 0)
                    {
                        whitecount += 1;
                    }
                    if (_amazons[x, y].team == 1 && _amazons[x, y].currentmoves.Count == 0)
                    {
                        blackcount += 1;
                    }
                }
            }
        }
        
        if (whitecount == 3)
        {
            winner_text[1].gameObject.SetActive(true);
            destroyStuffonWin();
            isWhiteTurn = true;
            gameover = true;
            StopAllCoroutines();
        }
        if (blackcount == 3)
        {
            winner_text[0].gameObject.SetActive(true);
            destroyStuffonWin();
            isWhiteTurn = true;
            gameover = true;
            StopAllCoroutines();
        }
    }
    //mouse click down for movement
    void OnMouseDown(int x, int y)
    {
        if(!Main_Menu.isPlayerAI)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_amazons[x, y] != null)
                {
                    //is it my turn? 
                    if ((_amazons[x, y].team == 0 && isWhiteTurn) || (_amazons[x, y].team == 1 && !isWhiteTurn))
                    {
                        movingPiece = _amazons[x, y];
                        availableMoves = movingPiece.GetAvailableMoves(ref _amazons, ref firePiece, _width, _height);
                        HighlightTiles();
                    }
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_amazons[x, y] != null)
                {
                    //is it my turn? 
                    if ((_amazons[x, y].team == 0 && isWhiteTurn))
                    {
                        movingPiece = _amazons[x, y];
                        availableMoves = movingPiece.GetAvailableMoves(ref _amazons, ref firePiece, _width, _height);
                        HighlightTiles();
                    }
                }
            }
        }
        
    }
    //mouse click up for placement
    void OnMouseUp(int x, int y)
    {
        if (movingPiece != null && Input.GetMouseButtonUp(0))
        {
            Vector2Int lastPos = new Vector2Int(movingPiece.CurrentX, movingPiece.CurrentY);
            bool validMove = MoveTo(movingPiece, x, y);
            if (!validMove)
            {

                movingPiece.setPosition(GetTileCenter(lastPos.x, lastPos.y));
                movingPiece = null;
                RemoveHighlightTiles();
            }
            else
            {
                movingPiece = null;
                RemoveHighlightTiles();
                waitforFire = true;
                availableFires = _amazons[x, y].GetAvailableMoves(ref _amazons, ref firePiece, _width, _height);
                StartCoroutine(FireArrow());
                StopCoroutine(FireArrow());

            }

        }
    }
    //drags the player piece when moving
    void dragPlayer(Ray ray)
    {
        if (movingPiece)
        {
            Plane horizPlane = new Plane(Vector3.forward, Vector3.forward * pieceOffset);

            float distance = 0.0f;
            if (horizPlane.Raycast(ray, out distance))
            {
                movingPiece.setPosition(ray.GetPoint(distance) + Vector3.back * dragOffset);
            }
        }
    }
    //puts previous tiles back on appropriate turn phase layer
    void changeLayerAfterHover()
    {
        if (movingPiece != null)
        {
            _board[mousehover.x, mousehover.y].layer = (ContainsValidMove(ref availableMoves, mousehover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
        }
        else
        {
            _board[mousehover.x, mousehover.y].layer = (ContainsValidMove(ref availableFires, mousehover)) ? LayerMask.NameToLayer("HighlightFire") : LayerMask.NameToLayer("Tile");
        }
    }
}
