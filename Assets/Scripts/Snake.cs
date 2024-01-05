using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public Vector3 direction;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            direction = new Vector3(0,0,1);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            direction = Vector3.right;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            direction = new Vector3(0,0,-1);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            direction = Vector3.left;
        }
    }
    private void FixedUpdate()
    {
        this.transform.position = new Vector3(
            Mathf.Round(this.transform.position.x) + direction.x,
            1.0f,
            Mathf.Round(this.transform.position.z) + direction.z
            );

    }
}
