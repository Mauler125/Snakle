using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public Vector3 direction;

    public List<Transform> snakeParts;

    public Transform partPrefab;

    private void Start()
    {
        snakeParts = new List<Transform>();
        snakeParts.Add(this.transform);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if(direction.z == 0)
            {
                direction = new Vector3(0, 0, 1);
            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (direction.x == 0)
            {
                direction = Vector3.right;
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (direction.z == 0)
            {
                direction = new Vector3(0, 0, -1);
            }
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (direction.x == 0)
            {
                direction = Vector3.left;
            }
        }
    }
    private void FixedUpdate()
    {
        for(int i = snakeParts.Count - 1; i > 0; i--)
        {
            snakeParts[i].position = snakeParts[i - 1].position;
        }

        this.transform.position = new Vector3(
            Mathf.Round(this.transform.position.x) + direction.x,
            1.0f,
            Mathf.Round(this.transform.position.z) + direction.z
            );

    }

    public void GrowSnake()
    {
        Transform part = Instantiate(this.partPrefab);
        part.position = snakeParts[snakeParts.Count - 1].position;

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
        Debug.Log("DEAD");
    }
}
