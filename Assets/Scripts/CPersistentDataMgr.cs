//===========================================================================//
//
// Purpose: persistent data saving & the management thereof
//
//===========================================================================//
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class CPersistentDataMgr
{
    //-------------------------------------------------------------------------
    // Init/Shutdown
    //-------------------------------------------------------------------------
    public void Init()
    {
        lastDateFilePath = Path.Combine(Application.persistentDataPath, lastDateFileName);
        lastScoresFilePath = Path.Combine(Application.persistentDataPath, lastScoresFileName);
        levelChangeFilePath = Path.Combine(Application.persistentDataPath, levelSwapFileName);

        LoadDateData();
        LoadScoreData();
        LoadLevelChangeData();

        CheckAndUpdateLevelChangeData();

        isInitialized = true;
    }

    public bool Initialized()
    {
        return isInitialized;
    }

    //-------------------------------------------------------------------------
    // Date data
    //-------------------------------------------------------------------------
    public void LoadDateData()
    {
        if (File.Exists(lastDateFilePath))
        {
            lastPlayDate = File.ReadAllText(lastDateFilePath);
        }
    }

    public void SaveDateFile()
    {
        lastPlayDate = Utility.GetCurrentDate_Formatted();
        File.WriteAllText(lastDateFilePath, lastPlayDate);
    }

    public string GetLastPlayDate()
    {
        return lastPlayDate;
    }

    //-------------------------------------------------------------------------
    // Score data
    //-------------------------------------------------------------------------
    public void LoadScoreData()
    {
        if (File.Exists(lastScoresFilePath))
        {
            string jsonData = File.ReadAllText(lastScoresFilePath);
            scoreTracker = JsonUtility.FromJson<ScoreTracker_s>(jsonData).scores;

            // if the user manually added entries to meme the game, truncate
            // then off here
            TruncateRedundantScoreEntries();
        }
        else
        {
            // create an empty if we don't have any data yet
            scoreTracker = new List<int>();
        }
    }

    public void SaveScoreData()
    {
        TruncateRedundantScoreEntries();

        string jsonData = JsonUtility.ToJson(new ScoreTracker_s(scoreTracker));
        File.WriteAllText(lastScoresFilePath, jsonData);
    }

    public void AddScoreEntry(int element)
    {
        // add to head
        scoreTracker.Insert(0, element);
        TruncateRedundantScoreEntries();
    }

    private void TruncateRedundantScoreEntries()
    {
        // NOTE: if we have moe elements than the number we are supposed to
        // track, truncate the last ones as the UI is fixed to this size
        while (scoreTracker.Count >= numScoresToTrack)
        {
            scoreTracker.RemoveAt(scoreTracker.Count-1);
        }
    }

    public int GetScoreEntry(int element)
    {
        // out of bound dictionary
        Assert.IsTrue(element < numScoresToTrack);
        return scoreTracker[element];
    }

    public int NumScoresTrackable()
    {
        return scoreTracker.Count;
    }

    //-------------------------------------------------------------------------
    // Level change data
    //-------------------------------------------------------------------------
    public void LoadLevelChangeData()
    {
        if (File.Exists(levelChangeFilePath))
        {
            string jsonString = File.ReadAllText(levelChangeFilePath);
            levelChangeData = JsonUtility.FromJson<LevelChange_s>(jsonString);
            levelChangeData.currentDateObj = DateTime.Parse(levelChangeData.currentDate);
        }
        else
        {
            levelChangeData = new LevelChange_s();
            levelChangeData.currentDateObj = DateTime.Now;
            levelChangeData.skinIndex = 1;
            SaveLevelChangeData();
        }
    }

    public void SaveLevelChangeData()
    {
        // Update currentDate string before saving
        levelChangeData.currentDate = levelChangeData.currentDateObj.ToString();

        string jsonString = JsonUtility.ToJson(levelChangeData);
        File.WriteAllText(levelChangeFilePath, jsonString);
    }

    private void CheckAndUpdateLevelChangeData()
    {
        DateTime oneMonthLater = levelChangeData.currentDateObj.AddMonths(1);

        if (DateTime.Now >= oneMonthLater)
        {
            levelChangeData.currentDateObj = DateTime.Now;
            levelChangeData.skinIndex++;
            SaveLevelChangeData();
        }
    }

    public int GetSkinIndex()
    {
        return levelChangeData.skinIndex;
    }

    [Serializable]
    private class ScoreTracker_s
    {
        public List<int> scores;

        public ScoreTracker_s(List<int> data)
        {
            scores = data;
        }
    }

    [Serializable]
    public class LevelChange_s
    {
        public string currentDate;
        public int skinIndex;

        [NonSerialized]
        public DateTime currentDateObj;
    }


    // unqualified file names for persistent data, currently we only track the
    // date and score; this should be good enuf
    private const string lastDateFileName = "last_date.txt";
    private const string lastScoresFileName = "last_scores.json";
    private const string levelSwapFileName = "level_swap.json";

    // qualified file path to the persisten data files
    private string lastDateFilePath;
    private string lastScoresFilePath;
    private string levelChangeFilePath;

    // the total # of scores we track and display, the UI is fixed at size 5 as
    // that was our design choice, so we just keep this a const
    private const int numScoresToTrack = 5;

    // cached off last play date, so we don't need to reload the last_date file
    // of the disk each time when we query whether we could play
    private string lastPlayDate;

    // whether the persistent data has been initialized
    private bool isInitialized;

    [SerializeField] // internal container for score json data
    private List<int> scoreTracker = new List<int>();

    [SerializeField]
    LevelChange_s levelChangeData;
}
