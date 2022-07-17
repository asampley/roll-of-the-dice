using System;
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
    public TextMeshProUGUI PiecesRemaining;
    public TextMeshProUGUI LogText;

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
        EventManager.AddListener("Move", _onMove);
        foreach (DiceState a in Enum.GetValues(typeof(DiceState))) {
            foreach (DiceState b in Enum.GetValues(typeof(DiceState))) {
                EventManager.AddListener("Ally" + a + "Beats" + b, () => _onABeatsB(a, b));
                EventManager.AddListener("Ally" + b + "BeatenBy" + a, () => _onABeatsB(a, b));
            }
        }
        EventManager.AddListener("Draw", _onDraw);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("SelectUnit", OnSelectUnit);
        EventManager.RemoveListener("Move", _onMove);

        foreach (DiceState a in Enum.GetValues(typeof(DiceState))) {
            foreach (DiceState b in Enum.GetValues(typeof(DiceState))) {
                EventManager.RemoveListener("Ally" + a + "Beats" + b, () => _onABeatsB(a, b));
                EventManager.RemoveListener("Ally" + b + "BeatenBy" + a, () => _onABeatsB(a, b));
            }
        }
        EventManager.RemoveListener("Draw", _onDraw);
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


    // UNIT SELECTION ------------------------
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

    private void _onMove()
    {
        LogText.text = "";
    }
    private void _onABeatsB(DiceState a, DiceState b) {
        LogText.text = a + " beats " + b + "!";
    }
    private void _onAllyRockBeatsScissors()
    {
        LogText.text = "Rock beats Scissors!";
    }
    private void _onAllyRockBeatenByPaper()
    {
        LogText.text = "Rock beaten by paper!";
    }
    private void _onAllyPaperBeatsRock()
    {
        LogText.text = "Paper beats Rock!";
    }
    private void _onAllyPaperBeatenByScissors()
    {
        LogText.text = "Paper beaten by Scissors!";
    }
    private void _onAllyScissorsBeatsPaper()
    {
        LogText.text = "Scissors beats Paper!";
    }
    private void _onAllyScissorsBeatenByRock()
    {
        LogText.text = "Scissors beaten by Rock!";
    }

    private void _onDraw()
    {
        LogText.text = "Draw.";
    }
}
