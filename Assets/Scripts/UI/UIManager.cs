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
    public TextMeshProUGUI piecesRemaining;
    public TextMeshProUGUI logText;
    public Transform inspector;
    public GameObject inspectorObject;

    public void Start() {
        GameManager.Instance.WinEvent += w => ShowVictoryScreen(w == Win.Player);
    }

    public void OpenMenu()
    {
        menuButton.SetActive(false);
        menus.SetActive(true);
    }


    private void OnEnable()
    {
        foreach (DiceState a in Enum.GetValues(typeof(DiceState))) {
            foreach (DiceState b in Enum.GetValues(typeof(DiceState))) {
                EventManager.AddListener("Ally" + a + "Beats" + b, () => _onABeatsB(a, b));
                EventManager.AddListener("Ally" + b + "BeatenBy" + a, () => _onABeatsB(a, b));
            }
        }

        EventManager.AddListener("SelectUnit", OnSelectUnit);
        EventManager.AddListener("Draw", _onDraw);
    }

    private void OnDisable()
    {
        foreach (DiceState a in Enum.GetValues(typeof(DiceState))) {
            foreach (DiceState b in Enum.GetValues(typeof(DiceState))) {
                EventManager.RemoveListener("Ally" + a + "Beats" + b, () => _onABeatsB(a, b));
                EventManager.RemoveListener("Ally" + b + "BeatenBy" + a, () => _onABeatsB(a, b));
            }
        }

        EventManager.RemoveListener("SelectUnit", OnSelectUnit);
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
        foreach (Transform child in inspector.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        gameInfo.SetActive(true);
        diceName.text = Globals.SELECTED_UNIT.diceName;
        movesAvailable.text = "Moves Available: " + Globals.SELECTED_UNIT.movesAvailable.ToString();
        GameObject inspectDie = Globals.SELECTED_UNIT.transform.Find("Mesh").gameObject;
        inspectorObject = Instantiate(inspectDie);
        inspectorObject.transform.parent = inspector;
        Vector3 pos = inspector.transform.position;
        pos.z -= 5;
        inspectorObject.transform.position = pos;
        inspectorObject.transform.localScale += new Vector3(65, 65, 65);
        inspectorObject.GetComponent<DieRotator>().enabled = false;
        inspectorObject.GetComponent<ObjectRotator>().enabled = true;
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


    private void _onABeatsB(DiceState a, DiceState b) {
        logText.text = a + " beats " + b + "!";
    }
    private void _onAllyRockBeatsScissors()
    {
        logText.text = "Rock beats Scissors!";
    }
    private void _onAllyRockBeatenByPaper()
    {
        logText.text = "Rock beaten by paper!";
    }
    private void _onAllyPaperBeatsRock()
    {
        logText.text = "Paper beats Rock!";
    }
    private void _onAllyPaperBeatenByScissors()
    {
        logText.text = "Paper beaten by Scissors!";
    }
    private void _onAllyScissorsBeatsPaper()
    {
        logText.text = "Scissors beats Paper!";
    }
    private void _onAllyScissorsBeatenByRock()
    {
        logText.text = "Scissors beaten by Rock!";
    }

    private void _onDraw()
    {
        logText.text = "Draw.";
    }
}
