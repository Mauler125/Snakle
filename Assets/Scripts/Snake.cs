using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public Vector3 direction;

    public List<Transform> snakeParts = new List<Transform>();

    public Transform partPrefab;

    private void Start()
    {
        // Add head to the List
        snakeParts.Add(this.transform);

        // Initial direction
        direction = Vector3.forward;
    }

    void Update()
    {
        // Move the player based to the direction based on their input
        // Rotates the head to the right direction
        if (Input.GetKeyDown(KeyCode.W) && direction.z == 0)
        {
            direction = Vector3.forward;
            this.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.D) && direction.x == 0)
        {
            direction = Vector3.right;
            this.gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (Input.GetKeyDown(KeyCode.S) && direction.z == 0)
        {
            direction = Vector3.back;
            this.gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (Input.GetKeyDown(KeyCode.A) && direction.x == 0)
        {
            direction = Vector3.left;
            this.gameObject.transform.rotation = Quaternion.Euler(0, 270, 0);
        }
    }
    private void FixedUpdate()
    {
        // Updating the position of eacht segment to match the position of the segment in front of it
        for(int i = snakeParts.Count - 1; i > 0; i--)
        {
            snakeParts[i].position = snakeParts[i - 1].position;
            snakeParts[i].rotation = snakeParts[i - 1].rotation;
        }

        //Updates the position of the snakes head
        this.transform.position = new Vector3(
            Mathf.Round(this.transform.position.x) + direction.x,
            1.0f,
            Mathf.Round(this.transform.position.z) + direction.z
            );
    }

    public void GrowSnake()
    {
        // Spawns new snake part and moves it to the last position
        Transform part = Instantiate(this.partPrefab);
        part.position = snakeParts[snakeParts.Count - 1].position;

        // Adds new part to the list
        snakeParts.Add(part);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food")) 
        {
            GrowSnake();
        }
        if (other.CompareTag("Obstacle"))
        {
            DieSnake();
        }
    }

    public void DieSnake()
    {
        // Still need logic for this..
        Debug.Log("DEAD");
    }
}
