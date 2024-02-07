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
    private int GetScoreMultiplier()
    {
        if (elapsedTime < 60)
        {
            return 5;
        }
        else if (elapsedTime < 120)
        {
            return 3;
        }
        else if (elapsedTime < 180)
        {
            return 2;
        }

        return 1;
    }

    public void IncrementScore()
    {
        currentScore += 1*GetScoreMultiplier();
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

    private const string fileName = "last_date.txt";
    private string persistenceFilePath;
    private string lastPlayDate;

    private float startTime;
    private float elapsedTime;

    private bool stopped;

    public Text timeUi;
    public Text scoreUi;

    public GameObject gameOverPanel;
    public Text scoreSummaryText;
    public Text nextDateSummaryText;

    public GameObject delayPanel;
    public Text nextDateDelayText;

    public int currentScore;
}
