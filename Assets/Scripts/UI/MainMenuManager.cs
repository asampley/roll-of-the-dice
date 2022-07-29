using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject levelSelect;
    public GameObject instructions;
    public GameObject options;

    // MAIN MENU -------------------------------------------------
    public void PlayGame()
    {
        mainMenu.SetActive(false);
        levelSelect.SetActive(true);
    }

    public void Instructions()
    {
        mainMenu.SetActive(false);
        instructions.SetActive(true);
    }

    public void Options()
    {
        mainMenu.SetActive(false);
        options.SetActive(true);
    }

    public void Back()
    {
        levelSelect.SetActive(false);
        instructions.SetActive(false);
        options.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }


    // LEVEL SELECT ----------------------------------------------------------
    public void Level(LevelData level)
    {
        CoreDataHandler.Instance.SetLevelID(level);
        CoreBooter.Instance.LoadLevel(level);
    }
}
