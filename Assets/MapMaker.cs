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
    [HideInInspector]
    public bool isRiver;
    [HideInInspector]
    public bool hasGenerated;
    
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

public class MapMaker : MonoBehaviour
{
    [SerializeField]
    public List<Tile> Tiles;
    public List<Tile> RiverTiles;

    public float tileLength = 16.8f;
    public int riverLength;

    private Tile[][] tileMap;
    void Start()
    {
        tileMap = new Tile[100][];
        MapRiver();
        GenerateFirstTile();
    }
    void LateStart() {
       // this is not a thing 
    }

    void MapRiver() {

        int xPos = 0;
        int yPos = 0;
        bool up = UnityEngine.Random.Range(0, 1) == 0;

        for(int i = 0; i <riverLength; i++){

            bool lastUp = up;
            if (tileMap[xPos] == null) 
                tileMap[xPos] = new Tile[130];
            // 1/6 to change direction
            if (UnityEngine.Random.Range(0, 5) == 1) up =!up;

            tileMap[xPos][yPos] = new Tile();
            tileMap[xPos][yPos].northCon = up ? ConnectionPart.River : ConnectionPart.Void;
            tileMap[xPos][yPos].southCon = lastUp ? ConnectionPart.River : ConnectionPart.Void;
            tileMap[xPos][yPos].eastCon = !up ? ConnectionPart.River : ConnectionPart.Void;
            tileMap[xPos][yPos].westCon = !lastUp ? ConnectionPart.River : ConnectionPart.Void;
            tileMap[xPos][yPos].isRiver = true;
            try {
                tileMap[-1* xPos][-1*yPos] = new Tile();
                tileMap[-1* xPos][-1*yPos].northCon = up ? ConnectionPart.River : ConnectionPart.Void;
                tileMap[-1* xPos][-1*yPos].southCon = up ? ConnectionPart.River : ConnectionPart.Void;
                tileMap[-1* xPos][-1*yPos].eastCon = !up ? ConnectionPart.River : ConnectionPart.Void;
                tileMap[-1* xPos][-1*yPos].westCon = !up ? ConnectionPart.River : ConnectionPart.Void;
                tileMap[xPos][yPos].isRiver = true;
            } catch {
                // just whatever
            }
        
            // move
            if (up) yPos++; else xPos++;

            
        }
        
    }
    
    // Expects no tile to have graphics yet, but rivers will be allocated
    void GenerateFirstTile() {
        bool isRiver = false;
        if (tileMap[49] == null) tileMap[49] = new Tile[100];
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
        tileMap[49][49].tileObject = Instantiate(tile.tileObject,
            new Vector3(49 * tileLength, 49 * tileLength, 0), Quaternion.identity);
    }

    Tile FindRandomTile(int x, int y, bool river = false) {
        List<Tile> source = new List<Tile>(river ? RiverTiles : Tiles);
        // randomly sort array of options
        source.Shuffle();

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
            (tileMap[x][y].northCon != ConnectionPart.Void && tileMap[x][y].northCon != source[i].northCon) ||
            (tileMap[x][y].eastCon != ConnectionPart.Void && tileMap[x][y].eastCon != source[i].eastCon) ||
            (tileMap[x][y].southCon != ConnectionPart.Void && tileMap[x][y].southCon != source[i].southCon) ||
            (tileMap[x][y].westCon != ConnectionPart.Void && tileMap[x][y].westCon != source[i].westCon)
            )) {
                isValid = false;
            }


            if (isValid) {
                return source[i].Clone();
            }
        }
        Debug.LogError("No tile was found to match at " + x +", " + y);
        return null;
    }
    // Update is called once per frame
    void Update()
    {
        // player at a given tile. Generate the ones arend it
        int playerX = (int)Math.Round(Camera.main.transform.position.x/tileLength);
        int playerY = (int)Math.Round(Camera.main.transform.position.y/tileLength);
        if (playerX<0 ||  playerY <0 ) {
            // will not map
            return;
        }
        
        if (!(tileMap[playerX] != null && tileMap[playerX][playerY] !=null && tileMap[playerX][playerY].hasGenerated)) {
            // Debug.Log("Mapping around " + playerX + ", " + playerY);
            if (tileMap[playerX] == null) tileMap[playerX] = new Tile[100];
            if (tileMap[playerX][playerY] == null) {
                MapTile(playerX, playerY);
            }
            tileMap[playerX][playerY].hasGenerated = true;
            MapTile(playerX, playerY + 1);
            MapTile(playerX, playerY - 1);
            MapTile(playerX + 1, playerY);
            MapTile(playerX - 1, playerY);
        }
        
        
    }
    void MapTile(int x, int y) {
        if (x <0 || y <0 ) {
            // will not map
            return;
        }
        bool isRiver = false;
        if ( tileMap[x] == null) {
            tileMap[x] = new Tile[100];
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
        tileMap[x][y].tileObject = Instantiate(tile.tileObject,
            new Vector3(x * tileLength, y * tileLength, 0), Quaternion.identity);
        
    }
}
