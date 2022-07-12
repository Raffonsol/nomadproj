using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class VillageManager : MonoBehaviour
{
    public static VillageManager Instance { get; private set; }

    public PreScene[] scenes;
    public PreSceneOccupant[] occupants;

    public GameObject neutralTemplate;

    public float villageCheckTime = 15f;

    private float checkTimer;

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
    // Start is called before the first frame update
    void Start()
    {
        checkTimer = villageCheckTime;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (checkTimer > 0) {
             checkTimer -= Time.deltaTime;
        } else {
            checkTimer = villageCheckTime;
            if (!BerkeleyManager.Instance.npcCapped) {
                int spawnI = UnityEngine.Random.Range(0,scenes.Length);
                SpawnScene(spawnI);
            }
        }
    }

    void SpawnScene(int sceneId) {
        float disappearDistance = BerkeleyManager.Instance.disappearDistance;
        Vector2 CamPos = Camera.main.transform.position;
        float x = UnityEngine.Random.Range(CamPos.x-disappearDistance, CamPos.x+disappearDistance);
        while (Math.Abs(x - Camera.main.transform.position.x) < 10) 
            x = UnityEngine.Random.Range(CamPos.x-disappearDistance, CamPos.x+disappearDistance);
        
        float y = UnityEngine.Random.Range(CamPos.y-disappearDistance, CamPos.y+disappearDistance);
        while (Math.Abs(y - Camera.main.transform.position.y) < 10) 
            y = UnityEngine.Random.Range(CamPos.y-disappearDistance, CamPos.y+disappearDistance);

        // now that x and y are settled, go through the spaces that have to be filled
        PreScene workingScene = scenes[sceneId];

        PreSceneAvailableTypes[] availableTypes = Array.ConvertAll(workingScene.availableTypes, x => x.Clone());

        PreSceneRoom[] availableRooms = Array.ConvertAll(workingScene.rooms, x=> x.Clone());

        for(int i = 0; i <availableTypes.Length; i++){
            // now go through all spaces
            for(int j = 0; j <availableRooms.Length; j++){
                if (!availableRooms[j].occupied && availableRooms[j].types.Contains(availableTypes[i].type) 
                && availableTypes[i].quantity > 0)
                {
                    // found match
                    availableRooms[j].occupied = true;
                    availableTypes[i].quantity -=1;

                    // find obj
                    PreSceneOccupant occupant = occupants.First( s => s.type == availableTypes[i].type);
                    GameObject toSpawn = occupant.visual; 

                    Instantiate(toSpawn, new Vector2(x + availableRooms[j].row*workingScene.spacePerRoom, y+ availableRooms[j].col*workingScene.spacePerRoom), Quaternion.Euler(0,0,UnityEngine.Random.Range(0,360)));

                }
            }
            
        }

        // now spawn population
        // adults
        for(int i = 0; i <workingScene.adults; i++){
            Instantiate(neutralTemplate, new Vector2(
                UnityEngine.Random.Range(x - 20f, x + 20f),
                UnityEngine.Random.Range(y - 20f, y + 20f)),
                transform.rotation);
        }
        
        
        
    }
}
