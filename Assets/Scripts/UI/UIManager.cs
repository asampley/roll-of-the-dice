using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    public GameObject menus;
    public GameObject menu;
    public GameObject options;
    public GameObject menuButton;
    public TextMeshProUGUI menuTitle;
    public Button nextLevelButton;

    //GameInfo
    public GameObject gameInfo;
    public TextMeshProUGUI diceName;
    public TextMeshProUGUI movesAvailable;

    public void Start() {
        GameManager.Instance.EnemyCountChange += c => {
            if (c == 0) {
                ShowVictoryScreen(true);
            }
        };

        GameManager.Instance.PlayerCountChange += c => {
            if (c == 0) {
                ShowVictoryScreen(false);
            }
        };
    }

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

    public void NextLevel() {
        Loader.LoadNext();
    }

    public void ShowVictoryScreen(bool victory) {
        menuTitle.text = victory ? "Victory" : "Defeat";
        menuTitle.color = victory
            ? new Color32(0, 255, 0, 255)
            : new Color32(255, 0, 0, 255);

        nextLevelButton.interactable = victory;

        OpenMenu();
    }
}
