using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class FoodScript : MonoBehaviour
{
    public BoxCollider gridArea;
    public Snake snakeParts;
    public CGameMgr gameMgr;

    public AudioClip eatSound;

    public void FoodSpawning()
    {
        // Bounds where the Collectible can spawn in
        Bounds bounds = this.gridArea.bounds;

        // New spawnPosition for the Collectible
        Vector3 spawnPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            0.0f,
            Random.Range(bounds.min.z, bounds.max.z));

        // Rounds the number to a whole number
        spawnPosition = new Vector3(Mathf.Round(spawnPosition.x), 1.0f, Mathf.Round(spawnPosition.z));

        if (snakeParts.snakeParts.Count() < 1022)
        {
            if (IsValidSpawnPosition(spawnPosition))
            {
                // Instantiate the food at a unoccupied position
                this.transform.position = spawnPosition;
            }
            else
            {
                // Retry when false
                FoodSpawning();
            }
        }
        else 
        {
            // QUITS WHEN YOU'VE COLLECTED EVERYTHING
            Application.Quit();
        }

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Assert.IsTrue(gameMgr);

            gameMgr.IncrementScore();
            FoodSpawning();
        }
    }
    bool IsValidSpawnPosition(Vector3 position)
    {
        // Checks each snake piece if the place is occupied
        foreach(Transform t in snakeParts.snakeParts) 
        {
            if (t.position == position)
            {
                return false;
            }
        }
        return true;
    }
}
