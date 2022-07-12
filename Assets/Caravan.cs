using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CaravanAction
{
    Commuting,
    Fleeing,
    Camping,
}

public class Caravan : MonoBehaviour
{
    public static Caravan Instance { get; private set; }
    public List<FriendlyChar> members;
    public List<Horse> looseHorses;
    public List<Cart> carts;

    public CaravanAction currentAction;

    public Vector2 destination;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
