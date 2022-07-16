using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class UIManager : MonoBehaviour
{
    public GameObject menus;
    public GameObject menu;
    public GameObject options;
    public GameObject menuButton;

    //GameInfo
    public GameObject gameInfo;
    public TextMeshProUGUI diceName;
    public TextMeshProUGUI movesAvailable;

    public void OpenMenu()
    {
        menuButton.SetActive(false);
        menus.SetActive(true);
    }


    private void OnEnable()
    {
        EventManager.AddListener("SelectUnit", OnSelectUnit);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("SelectUnit", OnSelectUnit);
    }


    // MENU -------------------------------------------------
    public void RestartLevel()
    {
        Back();
        GameManager.Instance.StartGame();
    }

    public void RerollLevel()
    {
        Back();
        GameManager.Instance.RerollGame();
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


    // UNIT SELECTIOn
    private void OnSelectUnit()
    {
        gameInfo.SetActive(true);
        diceName.text = Globals.SELECTED_UNIT.diceName;
        movesAvailable.text = "Moves Available: " + Globals.SELECTED_UNIT.movesAvailable.ToString();
    }
}
