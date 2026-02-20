using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTrap : MonoBehaviour
{
    public RandomEvent[] randomEvents;

    private float[] eventCheckTimers;
    private float[] cooldowns;
    // Start is called before the first frame update
    void Start()
    {
        eventCheckTimers = new float[randomEvents.Length];
        cooldowns = new float[randomEvents.Length];
        for (int i = 0; i < randomEvents.Length; i++)
        {
            eventCheckTimers[i] = randomEvents[i].checkInterval;
            cooldowns[i] = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        
        // check if player is in range and if so, trigger a random event based on the player's level and the event's chance
        float dist = Vector2.Distance(transform.position, Camera.main.transform.position);
        if (dist < 10f) // example range of 10 units
        {
            for (int i = 0; i < randomEvents.Length; i++)
            {
                if (cooldowns[i] > 0f)
                {
                    cooldowns[i] -= Time.deltaTime;
                    continue;
                }
                if (eventCheckTimers[i] > 0f)
                {
                    eventCheckTimers[i] -= Time.deltaTime;
                }else
                {
                    eventCheckTimers[i] = randomEvents[i].checkInterval;
                    RandomEvent randomEvent = randomEvents[i];
            
                    if (randomEvent.minLevel <= Player.Instance.playerLevel && randomEvent.maxLevel >= Player.Instance.playerLevel)
                    {
                        if (Random.Range(0f, 101f) < randomEvent.chance)
                        {
                            // trigger the event
                            Debug.Log("Triggering event: " + randomEvent.name);
                            TriggerEvent(i);
                        }
                    }
                }
            }
        }
    }
    void TriggerEvent(int randomEventIndex) {
        RandomEvent randomEvent = randomEvents[randomEventIndex];

        // cooldown
        cooldowns[randomEventIndex] = randomEvent.cooldown;

        // determine targets based on target type
        List<GameObject> targets = new List<GameObject>();
        switch (randomEvent.targetType)
        {
            case TargetType.MainChar:
                targets.Add(Player.Instance.controller.gameObject);
                break;
            case TargetType.RandomChar:
                if (Player.Instance.characters.Count > 0)                {
                    int randomIndex = Random.Range(0, Player.Instance.characters.Count);
                    targets.Add(Player.Instance.characters[randomIndex].controller.gameObject);
                }
                break;
            case TargetType.AllChars:
                foreach (FriendlyChar character in Player.Instance.characters)                {
                    targets.Add(character.controller.gameObject);
                }
                break;
        }
        foreach (GameObject target in targets)
        {
            
            // spawn the visual effect
            if (randomEvent.visual)
            {
                GameObject visual = Instantiate(randomEvent.visual, target.transform.position, Quaternion.identity);
                PlayDeath anim = visual.GetComponent<PlayDeath>();
                if (anim != null) {
                    anim.stickTarget = target;
                    anim.sticky = true;
                }
                // stun if its a trap
                if (randomEvent.eventTypes != null && System.Array.IndexOf(randomEvent.eventTypes, EventType.Trap) != -1)
                {
                    ZombieController combatant = target.GetComponent<ZombieController>();
                    if (combatant != null)
                    {
                        float duration = 2f;
                        duration = anim != null ? anim.steps.Count * anim.timePerStep : duration;
                        combatant.Stun(duration); // example stun duration of 2 seconds
                    }
                }
            }
        }
        // spawn the monsters
        foreach (Spawnable spawnable in randomEvent.spawnables)
        {
            int quantity = Random.Range(spawnable.minQuantity, spawnable.maxQuantity + 1);
            for (int i = 0; i < quantity; i++)
            {
                Vector2 spawnPos = (Vector2)targets[Random.Range(0, targets.Count)].transform.position + Random.insideUnitCircle * 2f; // example spawn radius of 2 units
                GameObject monsterObj = Instantiate(spawnable.monster, spawnPos, Quaternion.identity);
                Monster monster = monsterObj.GetComponent<Monster>();
                GameObject target = targets[Random.Range(0, targets.Count)];
                monster.Engage(target);
                // Set timer to run engage again in case the monster loses the target or gets stuck
                StartCoroutine(ReEngage(monster, target));
            }
        }
    }
    private IEnumerator ReEngage(Monster monster, GameObject target) {
        
            yield return new WaitForSeconds(2f); 
           
            monster.Engage(target);
            
        
    }
}
