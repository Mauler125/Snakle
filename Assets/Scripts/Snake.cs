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

    private enum Movement_t
    {
        //NOTE: invalid means that we can store a new cmd to process in the next
        // update tick.
        INVALID = -1,
        UP,
        DOWN,
        LEFT,
        RIGHT
    };

    // since movement simulation runs in a different thread than input, we need
    // to track the simulation tick and cache off the 'pending' movement cmd
    // which needs to be executed the next tick. this fixes an edge case bug
    // where the player could rotate the snake by 180 degrees and collide head
    // on. this bug became apparent after we ran a test with the customer, and
    // customer clicked on the buttons faster than we initially tested on.
    private int currentTick;
    private int lastMovementTick;

    // NOTE: we track the movement command in the main update thread; we can't
    // run movement in FixedUpdate as its too slow.
    Movement_t pendingMovementCmd = Movement_t.INVALID;

    //-------------------------------------------------------------------------
    // code callbacks
    //-------------------------------------------------------------------------

    public void Start()
    {
        InitSnake();
    }

    void Update()
    {
        // poll movement cmd if there's none; see Movement_t enum
        if (pendingMovementCmd == Movement_t.INVALID)
            GetInputCmd();

        if (ProcessInput())
            pendingMovementCmd = Movement_t.INVALID;
    }

    private void FixedUpdate()
    {
        RunSnakeMovement();
        currentTick++;
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

    //-------------------------------------------------------------------------
    // init
    //-------------------------------------------------------------------------

    void InitSnake()
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

    //-------------------------------------------------------------------------
    // game
    //-------------------------------------------------------------------------

    void RunSnakeMovement()
    {
        // Updating the position of each segment to match the position of the segment in front of it
        for (int i = snakeParts.Count - 1; i > 0; i--)
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

    public void DieSnake()
    {
        // Still need logic for this..
        Debug.Log("DEAD");
    }

    //-------------------------------------------------------------------------
    // input
    //-------------------------------------------------------------------------

    // returns true if a movement cmd has been processed
    bool ProcessInput()
    {
        // only process input once per simulation tick!!!
        if (lastMovementTick == currentTick)
            return false;

        lastMovementTick = currentTick;

        switch (pendingMovementCmd)
        {
            case Movement_t.UP:
                direction = Vector3.forward;
                this.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                return true;
            case Movement_t.DOWN:
                direction = Vector3.right;
                this.gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
                return true;
            case Movement_t.LEFT:
                direction = Vector3.back;
                this.gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                return true;
            case Movement_t.RIGHT:
                direction = Vector3.left;
                this.gameObject.transform.rotation = Quaternion.Euler(0, 270, 0);
                return true;

            // didn't process movement cmd.
            default:
                return false;
        }
    }

    void GetInputCmd()
    {
        if (pendingMovementCmd != Movement_t.INVALID)
            return;

        if (Input.GetKeyDown(KeyCode.W) && direction.z == 0)
        {
            pendingMovementCmd = Movement_t.UP;
        }
        else if (Input.GetKeyDown(KeyCode.D) && direction.x == 0)
        {
            pendingMovementCmd = Movement_t.DOWN;
        }
        else if (Input.GetKeyDown(KeyCode.S) && direction.z == 0)
        {
            pendingMovementCmd = Movement_t.LEFT;
        }
        else if (Input.GetKeyDown(KeyCode.A) && direction.x == 0)
        {
            pendingMovementCmd = Movement_t.RIGHT;
        }
    }
}
