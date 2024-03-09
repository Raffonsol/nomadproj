using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum RegionPlacement
{
    Central,
    West,
    North,
    South,
    East,
}

public enum ConnectionPart
{
    Grass,
    River,
    Street,
    Mountain,
    Void,
}
[Serializable]
public class RegionTransition
{
    public int regionId;
    public RegionPlacement placement;
    public Color transitionLightColor;

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
    public int minX;
    public int minY;
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

    public RegionTransition[] regionTransitions;
    public int[] regionBounds; // 0=north=+y  1=east=+x  2=south=-y  3=west=-x

    public GameObject grass;
    public float grassLength = 17.7f;

    public GameObject transitionLight;
    public GameObject transitionFog;

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
        DoStart();
    }
    public void DoStart()
    {
        debugMarker = Instantiate(debugMarker);
        tileMap = new Tile[mapHeight][];
        
        MapLine(riverLength, ConnectionPart.River, true, 23, 18);
        MapLine(riverLength, ConnectionPart.River, true, 19, 21);
        MapLine(100, ConnectionPart.Street, false, 0, 30);
        for (int i = 0; i < PreMapped.Count; i++)
        {
            MapSingleTile(PreMapped[i]);
        }
        for (int i = 0; i < PreMappedVillages.Count; i++)
        {
            MapTileGroup(PreMappedVillages[i].villageTiles, PreMappedVillages[i].maxX, PreMappedVillages[i].maxY,PreMappedVillages[i].minX, PreMappedVillages[i].minY);
        }
        // GenerateFirstTile();
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
    void MapTileGroup(Tile[] tile, int maxX, int maxY, int minX, int minY) {
        int xPos = UnityEngine.Random.Range(minX, maxX);
        int yPos = UnityEngine.Random.Range(minY, maxY);
        do {
            xPos = UnityEngine.Random.Range(minX, maxX);
            yPos = UnityEngine.Random.Range(minY, maxY);
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
        Debug.LogError("No tile was found to match at " + x +", " + y );
        try {Debug.Log("\n north="+tileMap[x][y+1].southCon); } catch{}
        try {Debug.Log("\n east="+tileMap[x+1][y].westCon); } catch{}
        try {Debug.Log("\n south="+tileMap[x][y-1].northCon); } catch{}
        try {Debug.Log("\n west="+tileMap[x-1][y].eastCon); } catch{}
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
    void CheckRegionalLights(int x, int y) {
        if (y >= regionBounds[0]) {  
            GameObject light = Instantiate(y==regionBounds[0]? transitionLight:transitionFog, new Vector2(x*tileLength, y*tileLength), Quaternion.Euler(0,0,0));
            light.GetComponent<SpriteRenderer>().color =  regionTransitions[1].transitionLightColor;
            light.GetComponent<TransitionLight>().regionIndex = 1;
        } else if (x >= regionBounds[1]) {   
            GameObject light = Instantiate(x==regionBounds[1]? transitionLight:transitionFog, new Vector2(x*tileLength, y*tileLength), Quaternion.Euler(0,0,270f));
            light.GetComponent<SpriteRenderer>().color =  regionTransitions[2].transitionLightColor;
            light.GetComponent<TransitionLight>().regionIndex = 2;
        } else if (y <= regionBounds[2]) {   
            GameObject light = Instantiate(y==regionBounds[2]? transitionLight:transitionFog, new Vector2(x*tileLength, y*tileLength), Quaternion.Euler(0,0,180f));
            light.GetComponent<SpriteRenderer>().color =  regionTransitions[3].transitionLightColor;
            light.GetComponent<TransitionLight>().regionIndex = 3;
        } else if (x <= regionBounds[3]) {   
            GameObject light = Instantiate(x==regionBounds[3]? transitionLight:transitionFog, new Vector2(x*tileLength, y*tileLength), Quaternion.Euler(0,0,90f));
            light.GetComponent<SpriteRenderer>().color =  regionTransitions[4].transitionLightColor;
            light.GetComponent<TransitionLight>().regionIndex = 4;
        }
    }
    GameObject SpawnTile(Tile tile, int x, int y ){
        GameObject inst = Instantiate(tile.tileObject,
            new Vector3(x * tileLength, y * tileLength, 0), tile.tileObject.transform.rotation);
        inst.transform.SetParent(transform);
        CheckRegionalLights(x,y);
        return inst;
    }

    public Tile GetTileAtCoordinates(float cx, float cy) {
        int x =(int)Math.Round(cx/tileLength);
        int y =(int)Math.Round(cy/tileLength);
        if (tileMap[x]==null || tileMap[x][y]==null) return null;
        return tileMap[x][y];
    }
}
