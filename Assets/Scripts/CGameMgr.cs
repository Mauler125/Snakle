using System;
using System.IO;
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
        persistenceFilePath = Path.Combine(Application.persistentDataPath, fileName);
        lastPlayDate = Persistence_ReadFile();

        // used in-game only
        if (timeUi && scoreUi)
        {
            timeUi.text = lastPlayDate;
            scoreUi.text = currentScore.ToString();
        }

        startTime = Time.time;
    }

    private void Update()
    {
        if (stopped)
            return;

        elapsedTime = Time.time - startTime;

        if (timeUi)
        {
            // F1 format cuz we don't care about the other digits besides the
            // first one after the mantissa
            timeUi.text = elapsedTime.ToString("F1");
        }
    }

    //-------------------------------------------------------------------------
    // Game
    //-------------------------------------------------------------------------
    public void StartGame()
    {
        if (!Persistence_CanPlay())
        {
            Assert.IsTrue(delayPanel);
            delayPanel.SetActive(true);

            Assert.IsTrue(nextDateDelayText);
            nextDateDelayText.text = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

            return;
        }

        SceneManager.LoadScene((int)Scenes_t.SCENE_GAME);

        // NOTE: persistence is written from here as we don't support pausing
        // the game; if the user quits for whatever reason (even forcefully),
        // the file could still be checked on the next launch and gate the
        // player out. this ultimately is also the only place this function
        // should be calld from!
        Persistence_WriteFile();
    }

    public void EndGame()
    {
        SceneManager.LoadScene((int)Scenes_t.SCENE_MAIN);
    }

    public void ShowGameSummary()
    {
        stopped = true;

        Assert.IsTrue(gameOverPanel);
        gameOverPanel.SetActive(true);

        scoreSummaryText.text = currentScore.ToString();
        nextDateSummaryText.text = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
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

    public void IncrementScore(int snakeLength)
    {
        currentScore += 1*GetScoreMultiplier(snakeLength);
        scoreUi.text = currentScore.ToString();
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

    //-------------------------------------------------------------------------
    // Persistence
    //-------------------------------------------------------------------------

    // returns true if we are allowed to play
    public bool Persistence_CanPlay()
    {
        return lastPlayDate != Persistence_GetCurrentDate();
    }

    private string Persistence_GetCurrentDate()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    private void Persistence_WriteFile()
    {
        lastPlayDate = Persistence_GetCurrentDate();
        File.WriteAllText(persistenceFilePath, lastPlayDate);
    }

    private string Persistence_ReadFile()
    {
        if (File.Exists(persistenceFilePath))
            return File.ReadAllText(persistenceFilePath);

        return null;
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

    // internal persistent data definition members
    private const string fileName = "last_date.txt";
    private string persistenceFilePath;
    private string lastPlayDate;

    // the time since we started, and the total elapsed time from
    // start to end
    private float startTime;
    private float elapsedTime;

    // once the player died or anything that requires the game to be stopped,
    // this bool must be set to stop simulating the player
    private bool stopped;

    // current player score
    private int currentScore;

    // in-game player statistics
    public Text timeUi;
    public Text scoreUi;

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
}
