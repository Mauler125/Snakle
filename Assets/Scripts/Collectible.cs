//===========================================================================//
//
// Purpose: collectible game object (i.e. trophies, food, etc..)
//
//===========================================================================//
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Collectible : MonoBehaviour
{
    public BoxCollider gridArea;
    public Snake snake;
    public CGameMgr gameMgr;

    public AudioClip eatSound;
    public AudioSource eatSource;

    public bool disableAfterPickup;
    public int additionalScoreAmount;

    public void SpawnAtRandomLocation()
    {
        // when this function gets called, we should enable the gameobj if it
        // was disabled. else you would need to do this from call site, and if
        // you happen to forget it, you will have a hard to find bug
        gameObject.SetActive(true);

        // Bounds where the Collectible can spawn in
        Bounds bounds = gridArea.bounds;

        // New spawnPosition for the Collectible
        Vector3 spawnPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            0.0f,
            Random.Range(bounds.min.z, bounds.max.z));

        // Rounds the number to a whole number
        spawnPosition = new Vector3(Mathf.Round(spawnPosition.x), 1.0f, Mathf.Round(spawnPosition.z));

        if (snake.snakeParts.Count() < 1022)
        {
            if (IsValidSpawnPosition(spawnPosition))
            {
                // Instantiate the food at a unoccupied position
                transform.position = spawnPosition;
            }
            else
            {
                // Retry when false
                SpawnAtRandomLocation();
            }
        }
        else 
        {
            gameMgr.ShowGameSummary();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Assert.IsTrue(gameMgr);

            gameMgr.IncrementScore(snake.GetSnakeLength(), additionalScoreAmount);
            eatSource.PlayOneShot(eatSound);

            if (disableAfterPickup)
            {
                gameObject.SetActive(false);
                return;
            }

            SpawnAtRandomLocation();
        }
    }
    bool IsValidSpawnPosition(Vector3 position)
    {
        // Checks each snake piece if the place is occupied
        foreach(Transform t in snake.snakeParts) 
        {
            if (t.position == position)
            {
                return false;
            }
        }

        return true;
    }
}
