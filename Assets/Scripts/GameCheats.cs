//===========================================================================//
//
// Purpose: development tools and shortcuts
//
//===========================================================================//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCheats : MonoBehaviour
{
    public List<GameObject> gameMaps;
    public GameObject currentMap;

    // Start is called before the first frame update
    void Start()
    {
        gameMaps[0].SetActive(true);
        currentMap = gameMaps[0].gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentMap.gameObject.SetActive(false);
            gameMaps[0].SetActive(true);
            currentMap = gameMaps[0].gameObject;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentMap.gameObject.SetActive(false);
            gameMaps[1].SetActive(true);
            currentMap = gameMaps[1].gameObject;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentMap.gameObject.SetActive(false);
            gameMaps[2].SetActive(true);
            currentMap = gameMaps[2].gameObject;
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(1);
    }
}
