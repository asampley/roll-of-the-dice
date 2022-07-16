using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class InGameMenu : MonoBehaviour
{
    public GameObject menus;
    public GameObject menu;
    public GameObject options;

    public GameObject menuButton;


    public void OpenMenu()
    {
        menuButton.SetActive(false);
        menus.SetActive(true);
    }

    // MENU -------------------------------------------------
    public void RestartLevel()
    {

    }

    public void RerollLevel()
    {

    }

    public void Options()
    {
        menu.SetActive(false);
        options.SetActive(true);
    }

    public void MainMenu()
    {
        Loader.Load(Loader.Scene.MainMenu);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void Back()
    {
        menus.SetActive(false);
        menuButton.SetActive(true);
    }
}
