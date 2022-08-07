using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject worldSelect;
    public GameObject meadowsLevels;
    public GameObject instructions;
    public GameObject options;

    // MAIN MENU -------------------------------------------------
    public void PlayGame()
    {
        mainMenu.SetActive(false);
        worldSelect.SetActive(true);
    }

    public void SelectMeadows()
    {
        worldSelect.SetActive(false);
        meadowsLevels.SetActive(true);
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
        worldSelect.SetActive(false);
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
