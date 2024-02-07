using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FoodScript : MonoBehaviour
{
    public BoxCollider gridArea;
    public Snake snakeParts;
    public CGameMgr gameMgr;

    public void FoodSpawning()
    {
        Bounds bounds = this.gridArea.bounds;

        Vector3 spawnPosition = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            0.0f,
            Random.Range(bounds.min.z, bounds.max.z));

        if (IsValidSpawnPosition(spawnPosition))
        {
            // Instantiate the food at a unoccupied position
            this.transform.position = new Vector3(Mathf.Round(spawnPosition.x), 1.0f, Mathf.Round(spawnPosition.z));
        }
        else
        {
            Destroy(this.gameObject);
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
        // Creates temporary transform for it to be "converted" to a Vector3
        Transform testTransform = new GameObject().transform;
        testTransform.position = position;

        // Check if the position is not occupied by the snake
        return !snakeParts.snakeParts.Contains(testTransform);
    }
}
