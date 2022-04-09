using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    public GameObject grassTile;
    public GameObject waterTile;
    public int mapHeight;
    public int mapWidth;

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
    private GameObject[] mapArray;

    private int riverPos;
    private int bridgePos;
    private int riverLength;
    // Start is called before the first frame update
    void Start()
    {
        
        // determine river stuff
        riverPos = Random.Range(2, mapWidth - 2);
        bridgePos = Random.Range(2, mapHeight - 2);
        riverLength = Random.Range( 3, 6);

        Map(0, 0);
    }
    public void Map(float startX, float startY)
    {
        // Debug.Log("Mapping");
        bool xGenCreated = false;
        bool yGenCreated = false;
        bool xyGenCreated = false;

        Vector3 size = new Vector3(0,0,0);
        mapArray = new GameObject[mapHeight *mapWidth];

        // running through each tile in the map
        for(int y = 0; y <mapHeight; y++){
            for(int x = 0; x <mapWidth; x++){

                // decide tile type
                GameObject tile;
                // checkRiver
                if ((x >= riverPos && x <= riverPos + riverLength
                // make sure its not bridge
                && (y > bridgePos + 2 || y < bridgePos ))
                // borders
                // || (y == 0 || x == 0 || y == mapHeight - 1 || x == mapWidth - 1)
                ) {
                    // it is a river
                    tile = waterTile;
                    riverPos += Random.Range(-1, 2);
                } else {
                    // make it a grass tile
                    tile = grassTile;
                }
                if (size.x==0)
                size = tile.GetComponent<Renderer>().bounds.size;
                // instantiate it
                mapArray[x*y] = GameObject.Instantiate(tile, new Vector3(startX + x*size.x, startY + y*size.y, 0), Quaternion.identity);
                mapArray[x*y].transform.parent = this.gameObject.transform;

                // generators for more map
                if (x == mapWidth-1 && y == mapHeight-1 && xyGenCreated == false) {
                    xyGenCreated = true;
                    mapArray[x*y].AddComponent<MapGenWaiter>();
                    mapArray[x*y].GetComponent<MapGenWaiter>().waitFor = GeneratePlacer.NE;
                    mapArray[x*y].GetComponent<SpriteRenderer>().color = Color.red;
                } else if (x == mapWidth-1 && xGenCreated == false) {
                    xGenCreated = true;
                    mapArray[x*y].AddComponent<MapGenWaiter>();
                    mapArray[x*y].GetComponent<MapGenWaiter>().waitFor = GeneratePlacer.E;
                    mapArray[x*y].GetComponent<SpriteRenderer>().color = Color.red;
                } else if (y == mapHeight-1 && yGenCreated == false) {
                    yGenCreated = true;
                    mapArray[x*y].AddComponent<MapGenWaiter>();
                    mapArray[x*y].GetComponent<MapGenWaiter>().waitFor = GeneratePlacer.N;
                    mapArray[x*y].GetComponent<SpriteRenderer>().color = Color.red;
                }

            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
