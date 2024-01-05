using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{

    public BoxCollider gridArea;
    public GameObject foodItem;
    public int foodCount;


    private void FixedUpdate()
    {
        if(foodCount == 0)
        {
            foodCount++;
            FoodSpawning();
        }
    }

    public void FoodSpawning()
    {
        Bounds bounds = this.gridArea.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        Vector3 foodLocation = new Vector3(Mathf.Round(x), 1.0f, Mathf.Round(z));

        Instantiate(foodItem, foodLocation, Quaternion.identity);

    }

    
}
