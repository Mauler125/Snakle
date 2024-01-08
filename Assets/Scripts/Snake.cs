using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Snake : MonoBehaviour
{
    public Vector3 direction;

    public int initialSnakeLength;

    public List<Transform> snakeParts = new List<Transform>();

    public Transform partPrefab;
    public Transform tailPrefab;

    private Transform tail;

    public void Start()
    {
        // Add head to the List
        snakeParts.Add(this.transform);

        // Initial direction
        direction = Vector3.forward;

        // Add initial tail pieces
        for (int i = 1; i < initialSnakeLength; i++)
        {
            GrowSnake();
        }

        // Adds Tail
        tail = Instantiate(tailPrefab);
        tail.position = this.transform.position - new Vector3(0, 0, 1);

        // Add the tail to the list
        snakeParts.Add(tail);
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
        // Updating the position of each segment to match the position of the segment in front of it
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
        // NOTE: cache off the last index since it will contain the
        // tail of the snake, we add a new part towards the end, but
        // want to move the tail towards the back again.
        int currArraySize = (snakeParts.Count - 1);

        // Spawns new snake part and moves it to the last position
        Transform part = Instantiate(this.partPrefab);
        part.position = snakeParts[currArraySize].position;

        snakeParts.Add(part);
        tail = snakeParts[currArraySize]; // Get the tail.

        // Move the tail to the last position in the list
        if (snakeParts.Count > 2)
        {
            Assert.IsTrue(tail.tag == "SnakeTail", "tail should always be the last part of the snake; forgot to tag it SnakeTail???");

            snakeParts.RemoveAt(currArraySize);  // Remove the tail from its current position
            snakeParts.Add(tail);   // Add the tail to the end of the list
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        // Speaks for itself
        if (other.CompareTag("Food")) 
        {
            GrowSnake();
        }
        if (other.CompareTag("Obstacle") || other.CompareTag("SnakeTail"))
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
