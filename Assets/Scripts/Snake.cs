using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Snake : MonoBehaviour
{
    public CGameMgr gameMgr;

    public Vector3 direction;

    public int initialSnakeLength;

    public List<Transform> snakeParts = new List<Transform>();

    public Transform partPrefab;
    public Transform tailPrefab;

    private Transform tail;
    public AudioClip deathSound; 
    public AudioSource deathSource;
    private bool stopMovement;

    public Text countDownText;
    public float duration, cTimer;
    public AudioSource timeSound;

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

        stopMovement = true;
        cTimer = duration;
        countDownText.text = cTimer.ToString();
        StartCoroutine(countDown());
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
        if (stopMovement)
            return;

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

    IEnumerator countDown()
    {
        while(cTimer >= 0)
        {
            countDownText.text = cTimer.ToString();
            yield return new WaitForSeconds(1f);
            cTimer--;
            timeSound.Play();
            if(cTimer <= 0)
            {
                Destroy(countDownText);
                stopMovement = false;
            }
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
        int currArraySize = (snakeParts.Count - 1);

        // Updating the position of each segment to match the position of the segment in front of it
        for (int i = currArraySize; i > 0; i--)
        {
            if (snakeParts[i].childCount > 1)
            {
                if (snakeParts[i].rotation != snakeParts[i - 1].rotation)
                {
                    Vector3 forwardCurr = snakeParts[i].forward;
                    Vector3 forwardPrev = snakeParts[i - 1].forward;

                    // left turns use a different snake mdl to connect the
                    // segments in thebends 
                    bool isLeftTurn = Vector3.Cross(forwardPrev, forwardCurr).y > 0.0f;

                    int mdlToEnable = isLeftTurn ? 1 : 2;

                    // NOTE: we have to disable the other one as testing
                    // revealed that it would otherwise show both of them
                    // if you are fast enough
                    int mdlToDisable = isLeftTurn ? 2 : 1;

                    snakeParts[i].GetChild(0).gameObject.SetActive(false);

                    snakeParts[i].GetChild(mdlToEnable).gameObject.SetActive(true);
                    snakeParts[i].GetChild(mdlToDisable).gameObject.SetActive(false);
                }
                else
                {
                    snakeParts[i].GetChild(0).gameObject.SetActive(true);

                    // disable the rotated models
                    snakeParts[i].GetChild(1).gameObject.SetActive(false);
                    snakeParts[i].GetChild(2).gameObject.SetActive(false);
                }
            }

            snakeParts[i].rotation = snakeParts[i - 1].rotation;
            snakeParts[i].position = snakeParts[i - 1].position;
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
        part.rotation = snakeParts[currArraySize].rotation;

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
        // called while already stopped; code bug.
        Assert.IsFalse(stopMovement);
        stopMovement = true;

        Assert.IsTrue(gameMgr);
        gameMgr.ShowGameSummary();
        deathSource.PlayOneShot(deathSound);
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

    //-------------------------------------------------------------------------
    // Getters
    //-------------------------------------------------------------------------
    public int GetSnakeLength()
    {
        return snakeParts.Count;
    }
}
