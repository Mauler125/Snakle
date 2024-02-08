using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CGameMgr : MonoBehaviour
{
    //-------------------------------------------------------------------------
    // Code callbacks
    //-------------------------------------------------------------------------
    private void Start()
    {
        persisentDataManager.Init();

        // used in-game only
        if (timeUi && scoreUi)
        {
            timeUi.text = persisentDataManager.GetLastPlayDate();
            scoreUi.text = currentScore.ToString();
        }

        // if we have a score board, initialize it
        if (scoreFields != null && scoreFields.Count() > 0)
        {
            for (int i = 0; i < scoreFields.Count() && i < persisentDataManager.NumScoresTrackable(); i++)
            {
                scoreFields[i].text = persisentDataManager.GetScoreEntry(i).ToString();
            }
        }

        Time.fixedDeltaTime = defaultTimeStep;
    }

    private void Update()
    {
        if (paused)
            return;

        elapsedTime = (startTime > 0) ? Time.time - startTime : 0;

        if (timeUi)
        {
            // F1 format cuz we don't care about the other digits besides the
            // first one after the mantissa
            timeUi.text = elapsedTime.ToString("F1");
        }

        Debug.Log(Time.fixedDeltaTime);
    }

    //-------------------------------------------------------------------------
    // Game
    //-------------------------------------------------------------------------
    public void StartGame()
    {
        Assert.IsTrue(delayPanel); // Missing delay objects
        Assert.IsTrue(nextDateDelayText);

        if (!CanPlay())
        {
            delayPanel.SetActive(true);
            nextDateDelayText.text = Utility.GetDate_Formatted(1);

            return;
        }

        SceneManager.LoadScene((int)Scenes_t.SCENE_GAME);

        // NOTE: persistent date is written from here as we don't support
        // pausing the game; if the user quits for whatever reason (even
        // forcefully), the file could still be checked on the next launch
        // and gate the player out. this ultimately is also the only place
        // this function should be called from!
        persisentDataManager.SaveDateFile();
    }

    public void EndGame()
    {
        SceneManager.LoadScene((int)Scenes_t.SCENE_MAIN);
    }

    // returns true if we are allowed to play
    private bool CanPlay()
    {
        return persisentDataManager.GetLastPlayDate() != Utility.GetCurrentDate_Formatted();
    }

    public void ShowGameSummary()
    {
        paused = true;

        Assert.IsTrue(gameOverPanel);
        gameOverPanel.SetActive(true);

        scoreSummaryText.text = currentScore.ToString();
        nextDateSummaryText.text = Utility.GetDate_Formatted(1);

        persisentDataManager.AddScoreEntry(currentScore);
        persisentDataManager.SaveScoreData();
    }

    private void SpawnCollectible()
    {
        Collectible collectible = collectibles[UnityEngine.Random.Range(0, collectibles.Length)];
        collectible.SpawnAtRandomLocation();
    }

    // simple multiplier that checks the elapsed timr, the longer the player is
    // in the game, the less each score will be multiplied
    private int GetScoreMultiplier(int snakeLength)
    {
        // NOTE: this is ran backwards, as the player is very likely to have
        // reached the first few multipliers during the start of the game.
        // small optimization that prolly doesn't really matter, but why not,
        // its good practice in the long run
        for (int i = multipliers.Length; (i--) > 0;)
        {
            ScoreMultiplier_s multiplier = multipliers[i];

            if (snakeLength >= multiplier.requiredAmount)
            {
                return multiplier.multiplyAmount;
            }
        }

        // default = no multiplicatin at all
        return 1;
    }

    public void IncrementScore(int snakeLength, int additionalScore = 0)
    {
        int multiplier = GetScoreMultiplier(snakeLength);

        // must be set first since else we spawn collectibles right of the bat
        if (lastMultiplierAmmount == 0)
            lastMultiplierAmmount = multiplier;

        if (multiplier != lastMultiplierAmmount)
        {
            lastMultiplierAmmount = multiplier;
            SpawnCollectible();
        }

        currentScore += (1+ additionalScore) * GetScoreMultiplier(snakeLength);
        scoreUi.text = currentScore.ToString();

        Time.fixedDeltaTime *= timeStepIncrement;
    }

    public void SetPause(bool pause)
    {
        paused = pause;
    }

    public void InitTimer()
    {
        startTime = Time.time;
    }

    //-------------------------------------------------------------------------
    // Panel callbacks
    //-------------------------------------------------------------------------

    public void PanelCallback_StartGame()
    {
        StartGame();
    }

    public void PanelCallback_StopGame()
    {
        EndGame();
    }

    public void PanelCallback_QuitGame()
    {
        Application.Quit();
    }

    public void PanelCallback_HideDelayWindow()
    {
        Assert.IsTrue(delayPanel);
        delayPanel.SetActive(false);
    }

    enum Scenes_t : int
    {
        SCENE_MAIN = 0,
        SCENE_GAME
    }

    [System.Serializable]
    public struct ScoreMultiplier_s
    {
        // the criteria that has to be met before any multiplication
        // takes place
        public int requiredAmount;

        public int multiplyAmount;
    }

    // the time since we started, and the total elapsed time from
    // start to end
    private float startTime;
    private float elapsedTime;

    // once the player died or anything that requires the game to be paused,
    // this bool must be set to stop simulating the player
    private bool paused;

    // current player score
    private int currentScore;

    private int lastMultiplierAmmount;

    // used to check for whether we can play, and the score board on the title
    // screen of the game
    private CPersistentDataMgr persisentDataManager = new();

    // in-game player statistics
    public Text timeUi;
    public Text scoreUi;

    // the score fields constructing the score board
    public Text[] scoreFields;

    // summary panels and data; displayed when the game has finished
    public GameObject gameOverPanel;
    public Text scoreSummaryText;
    public Text nextDateSummaryText;

    // delay panel; displayed when a user attempts to restart the game while
    // the next playable date hasn't been reached yet
    public GameObject delayPanel;
    public Text nextDateDelayText;

    // array dictating when a user receives a multiplication on their score,
    // and how much. the order doesn't matter, but for best performance, order
    // then from "very likely to reach" to "very unlikely to reach" as that
    // avoids unnecessary iterations over already passed score multipliers
    public ScoreMultiplier_s[] multipliers;

    public Collectible[] collectibles;

    // the default fixed time step that will be enforced on each game start
    public float defaultTimeStep;

    // the time step gets incremented with this value each time the player
    // picks a collectible, this is to make the game run faster for a more
    // challenging game experienc
    public float timeStepIncrement;
}
