using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ConnectionPart
{
    Grass,
    River,
    Street,
    Mountain,
    Void,
}
[Serializable]
public class Tile 
{
    public GameObject tileObject;
    public ConnectionPart northCon;
    public ConnectionPart eastCon;
    public ConnectionPart southCon;
    public ConnectionPart westCon;

    public int skipChance = 0;

    [HideInInspector]
    public bool isRiver;
    [HideInInspector]
    public bool hasGenerated;

    public TileController controller;
    
    public Tile() {
        northCon = ConnectionPart.Void;
        southCon = ConnectionPart.Void;
        eastCon = ConnectionPart.Void;
        westCon = ConnectionPart.Void;

        isRiver = false;
        hasGenerated = false;
    } 
    public Tile Clone()
    {
        return (Tile)this.MemberwiseClone();
    }
}
[Serializable]
public class TileVillage 
{
    public int id;
    public Tile[] villageTiles;
    public int maxX;
    public int maxY;
}

public class MapMaker : MonoBehaviour
{
    public static MapMaker Instance { get; private set; }
    public GameObject debugMarker;
    [SerializeField]
    public List<Tile> Tiles;
    public List<Tile> RiverTiles;
    public List<Tile> PreMapped;
    public List<TileVillage> PreMappedVillages;

    public float tileLength = 16.8f;
    public int riverLength;
    public int mapHeight = 100;

    public GameObject grass;
    public float grassLength = 17.7f;

    private Tile[][] tileMap;

    // Singleton stuff
    private void Awake() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }
    void Start()
    {
        tileMap = new Tile[mapHeight][];
        
        MapLine(riverLength, ConnectionPart.River, true, 0, 0);
        MapLine(riverLength, ConnectionPart.River, true, 0, 20);
        MapLine(100, ConnectionPart.Street, false, 0, 30);
        for (int i = 0; i < PreMapped.Count; i++)
        {
            MapSingleTile(PreMapped[i]);
        }
        for (int i = 0; i < PreMappedVillages.Count; i++)
        {
            MapTileGroup(PreMappedVillages[i].villageTiles, PreMappedVillages[i].maxX, PreMappedVillages[i].maxY);
        }
        GenerateFirstTile();
        BerkeleyManager.Instance.DoStart();
    }
    public void DeleteTileAt(int x, int y) {
        tileMap[x][y].tileObject = null;
    }
    void MapLine(int length, ConnectionPart conP, bool isRiver, int startX, int startY) {

        int xPos = startX;
        int yPos = startY;
        bool up = UnityEngine.Random.Range(0, 1) == 0;

        for(int i = 0; i <length; i++){

            bool lastUp = up;
            if (tileMap[xPos] == null) 
                tileMap[xPos] = new Tile[mapHeight];
            else if (tileMap[xPos][yPos]!=null && tileMap[xPos][yPos].isRiver) return;
            // 1/6 to change direction
            if (UnityEngine.Random.Range(0, 5) == 1) up =!up;

            tileMap[xPos][yPos] = new Tile();
            if (up) tileMap[xPos][yPos].northCon = conP;
            if (lastUp) tileMap[xPos][yPos].southCon = conP;
            if (!up) tileMap[xPos][yPos].eastCon = conP;
            if (!lastUp) tileMap[xPos][yPos].westCon = conP;
            if (isRiver)tileMap[xPos][yPos].isRiver = isRiver;

        
            // move
            if (up) yPos++; else xPos++;
        }
        
    }
    void MapSingleTile(Tile tile) {
        int xPos = UnityEngine.Random.Range(0, mapHeight);
        int yPos = UnityEngine.Random.Range(0, mapHeight);
        do {
            xPos = UnityEngine.Random.Range(0, mapHeight);
            yPos = UnityEngine.Random.Range(0, mapHeight);
            if (tileMap[xPos] == null) 
                tileMap[xPos] = new Tile[mapHeight];
        } while (tileMap[xPos][yPos] !=null);
        
        tileMap[xPos][yPos] = tile.Clone();
        tileMap[xPos][yPos].tileObject = SpawnTile(tile, xPos, yPos); 
    }
    void MapTileGroup(Tile[] tile, int maxX, int maxY) {
        int xPos = UnityEngine.Random.Range(0, maxX);
        int yPos = UnityEngine.Random.Range(0, maxY);
        do {
            xPos = UnityEngine.Random.Range(0, maxX);
            yPos = UnityEngine.Random.Range(0, maxY);
            if (tileMap[xPos] == null) tileMap[xPos] = new Tile[mapHeight];
            if (tileMap[xPos+1] == null) tileMap[xPos+1] = new Tile[mapHeight];
        } while (xPos>=mapHeight || yPos<=0 || tileMap[xPos][yPos] !=null || tileMap[xPos+1][yPos] !=null || tileMap[xPos][yPos-1] !=null || tileMap[xPos+1][yPos-1] !=null);
        
        tileMap[xPos][yPos] = tile[0].Clone();
        tileMap[xPos][yPos].tileObject = SpawnTile(tile[0],xPos, yPos);
        tileMap[xPos+1][yPos] = tile[1].Clone();
        tileMap[xPos+1][yPos].tileObject = SpawnTile(tile[1], (xPos+1), yPos);  
        tileMap[xPos][yPos-1] = tile[2].Clone();
        tileMap[xPos][yPos-1].tileObject = SpawnTile(tile[2], xPos, (yPos-1) ); 
        tileMap[xPos+1][yPos-1] = tile[3].Clone();
        tileMap[xPos+1][yPos-1].tileObject = SpawnTile(tile[3],(xPos+1), (yPos-1));
            
        UIManager.Instance.villageLocations.Add(new Vector2(xPos*tileLength+tileLength/2, (yPos-1)*tileLength+tileLength/2));
    }
    // Expects no tile to have graphics yet, but rivers will be allocated
    void GenerateFirstTile() {
        bool isRiver = false;
        if (tileMap[49] == null) tileMap[49] = new Tile[mapHeight];
        if (tileMap[49][49] != null) {
            // is river
           isRiver = true;
        } else {
            // is random
            tileMap[49][49] = new Tile();
            isRiver = false;
        }
        Tile tile = FindRandomTile(49,49, isRiver);
        tileMap[49][49] = tile;
        tileMap[49][49].tileObject = SpawnTile(tile,49, 49);
    }

    Tile FindRandomTile(int x, int y, bool river = false) {
        List<Tile> source = new List<Tile>(river ? RiverTiles : Tiles);
        // randomly sort array of options
        source.Shuffle();
        Tile skip = new Tile();
        bool skipped = false;
        // go through options and only accept one that matches all existing borders
        for(int i = 0; i <source.Count; i++){
            bool isValid = true;

            // check north
            if (tileMap[x] != null && tileMap[x][y+1] != null
            && tileMap[x][y+1].southCon != source[i].northCon
            && tileMap[x][y+1].southCon != ConnectionPart.Void && source[i].northCon != ConnectionPart.Void) {
                isValid = false;
            }

            // check east
            if (tileMap[x+1] != null && tileMap[x+1][y] != null
            && tileMap[x+1][y].westCon != source[i].eastCon
            && tileMap[x+1][y].westCon != ConnectionPart.Void && source[i].eastCon != ConnectionPart.Void) {
                isValid = false;
            }

            // check south
            if (tileMap[x] != null && y>0 && tileMap[x][y-1] != null
            && tileMap[x][y-1].northCon != source[i].southCon
            && tileMap[x][y-1].northCon != ConnectionPart.Void && source[i].southCon != ConnectionPart.Void ) {
                isValid = false;
            }

            // check west
            if (x>0 && tileMap[x-1] != null && tileMap[x-1][y] != null
            && tileMap[x-1][y].eastCon != source[i].westCon
            && tileMap[x-1][y].eastCon != ConnectionPart.Void && source[i].westCon != ConnectionPart.Void) {
                isValid = false;
            }
            // make sure the already mapped parts are there
            if (tileMap[x] != null && tileMap[x][y] != null && (
            (tileMap[x][y].northCon != null && tileMap[x][y].northCon != ConnectionPart.Void && tileMap[x][y].northCon != source[i].northCon) ||
            (tileMap[x][y].eastCon != null && tileMap[x][y].eastCon != ConnectionPart.Void && tileMap[x][y].eastCon != source[i].eastCon) ||
            (tileMap[x][y].southCon != null && tileMap[x][y].southCon != ConnectionPart.Void && tileMap[x][y].southCon != source[i].southCon) ||
            (tileMap[x][y].westCon != null && tileMap[x][y].westCon != ConnectionPart.Void && tileMap[x][y].westCon != source[i].westCon)
            )) {
                isValid = false;
            }

            // now we know it's good, but we might wanna skip it
             if (isValid && UnityEngine.Random.Range(0, 100) < source[i].skipChance) {
                 isValid = false;
                 if (!skipped) {
                    skip = source[i].Clone();
                    skipped = true;
                 }
             }

            // otherwise go with a clone of this tile
            if (isValid) {
                return source[i].Clone();
            }
        }
        // now we got nothing, so let's take one that we skipped earlier if we did
        if (skipped){
            // Instantiate(debugMarker, new Vector2(x*tileLength, y*tileLength), transform.rotation);
            return skip;
        }
        // k now we really got nothing, this bad
        Debug.LogError("No tile was found to match at " + x +", " + y);
        return null;
    }
    // Update is called once per frame
    void Update()
    {
        WalkMap();
    }
    void WalkMap()
    {
        if (BerkeleyManager.Instance.mapBounds == 0)
        BerkeleyManager.Instance.mapBounds = mapHeight * tileLength;
        // player at a given tile. Generate the ones arend it
        int playerX = (int)Math.Round(Camera.main.transform.position.x/tileLength);
        int playerY = (int)Math.Round(Camera.main.transform.position.y/tileLength);
        if (playerX<0 ||  playerY <0 ) {
            // will not map
            return;
        }
        
        if (!(tileMap[playerX] != null && tileMap[playerX][playerY] !=null && tileMap[playerX][playerY].hasGenerated)) {
            // Debug.Log("Mapping around " + playerX + ", " + playerY);
            if (tileMap[playerX] == null) tileMap[playerX] = new Tile[mapHeight];
            // if (tileMap[playerX][playerY] == null) {
                MapTile(playerX, playerY);
            // }
            tileMap[playerX][playerY].hasGenerated = true;
            MapTile(playerX, playerY + 1);
            MapTile(playerX, playerY - 1);
            MapTile(playerX + 1, playerY);
            MapTile(playerX - 1, playerY);

            MapTile(playerX + 1, playerY + 1);
            MapTile(playerX - 1, playerY - 1);

            MapTile(playerX + 1, playerY -1);
            MapTile(playerX - 1, playerY +1);

            // -- 
            MapTile(playerX, playerY + 2);
            MapTile(playerX + 1, playerY + 2);
            MapTile(playerX + 2, playerY + 2);
            MapTile(playerX + -1, playerY + 2);
            MapTile(playerX + -2, playerY + 2);

            MapTile(playerX, playerY - 2);
            MapTile(playerX + 1, playerY - 2);
            MapTile(playerX + 2, playerY - 2);
            MapTile(playerX + -1, playerY - 2);
            MapTile(playerX + -2, playerY - 2);

            MapTile(playerX + 2, playerY);
            MapTile(playerX - 2, playerY);

            MapTile(playerX + 2, playerY -1);
            MapTile(playerX - 2, playerY -1);

            MapTile(playerX + 2, playerY + 1);
            MapTile(playerX - 2, playerY + 1);
        }
        
        
    }
    void MapTile(int x, int y) {
        if (x <0 || y <0 ) {
            // will not map
            return;
        }
        bool isRiver = false;
        if ( tileMap[x] == null) {
            tileMap[x] = new Tile[mapHeight];
        } 
        if (tileMap[x][y] == null) {
            tileMap[x][y] = new Tile();
        } else {
            // already have something, if it has graphics then return
            if (tileMap[x][y].tileObject != null) return;
            // otherwise it might be a river
            isRiver = tileMap[x][y].isRiver;
        }

        Tile tile = FindRandomTile(x,y, isRiver);
        tileMap[x][y] = tile;
        tileMap[x][y].tileObject = SpawnTile(tile,x , y );
        TileController controller = tileMap[x][y].tileObject.AddComponent<TileController>();
        controller.x = x; controller.y = y; controller.hasRoadOrRiver=isRiver||tile.southCon==ConnectionPart.Street||tile.northCon==ConnectionPart.Street||tile.westCon==ConnectionPart.Street||tile.eastCon==ConnectionPart.Street;
        tile.controller = controller;
    }
    GameObject SpawnTile(Tile tile, float x, float y ){
        GameObject inst = Instantiate(tile.tileObject,
            new Vector3(x * tileLength, y * tileLength, 0), tile.tileObject.transform.rotation);
        inst.transform.SetParent(transform);
        return inst;
    }
    void SpawnGrass(float x, float y ){
        int random = UnityEngine.Random.Range(0, 5);
        float z = 90f;
        if (random == 0) z = 0;
        if (random == 2) z = 270f;
        if (random == 3) z = 180f;
        GameObject inst = Instantiate(grass,
            new Vector3(x * grassLength, y * grassLength, 0), new Quaternion (0,0,z,0));
        inst.transform.SetParent(transform.Find("grass"));
    }
    public Tile GetTileAtCoordinates(float cx, float cy) {
        int x =(int)Math.Round(cx/tileLength);
        int y =(int)Math.Round(cy/tileLength);
        if (tileMap[x]==null || tileMap[x][y]==null) return null;
        return tileMap[x][y];
    }
}
