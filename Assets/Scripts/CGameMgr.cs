using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CGameMgr : MonoBehaviour
{
    //-------------------------------------------------------------------------
    // Code callbacks
    //-------------------------------------------------------------------------
    private void Start()
    {
        persistenceFilePath = Path.Combine(Application.persistentDataPath, fileName);
        lastPlayDate = Persistence_ReadFile();
    }

    //-------------------------------------------------------------------------
    // 
    //-------------------------------------------------------------------------
    public void StartGame()
    {
        if (!Persistence_CanPlay())
        {
            // TODO[ KAWE ]: add menu for not being able to play here...
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
}
