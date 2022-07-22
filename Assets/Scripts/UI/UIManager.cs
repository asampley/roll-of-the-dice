using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
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
    public Image img1;
    public Image img2;
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
        DieManager.ABeatsB += OnABeatsB;
        DieManager.Draw += OnDraw;

        EventManager.AddListener("SelectUnit", OnSelectUnit);
    }

    private void OnDisable()
    {
        DieManager.ABeatsB -= OnABeatsB;
        DieManager.Draw -= OnDraw;

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


    // UNIT SELECTION ------------------------
    private void OnSelectUnit()
    {
        foreach (Transform child in inspector.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        gameInfo.SetActive(true);
        diceName.text = Globals.SELECTED_UNIT.diceName;
        movesAvailable.text = "Moves Available: " + Globals.SELECTED_UNIT.MovesAvailable.ToString();
        GameObject inspectDie = Globals.SELECTED_UNIT.GetComponent<DieManager>().ghostComponents.gameObject;
        inspectorObject = Instantiate(inspectDie);
        inspectorObject.transform.parent = inspector;
        Vector3 pos = inspector.transform.position;
        pos.z -= 5;
        inspectorObject.transform.position = pos;
        inspectorObject.transform.localScale = new Vector3(95, 95, 95);
        inspectorObject.GetComponent<DieRotator>().enabled = false;
        inspectorObject.GetComponent<ObjectRotator>().enabled = true;
        inspectorObject.GetComponent<SortingGroup>().enabled = true;
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


    private void OnABeatsB(DieManager a, DieManager b) {
        var spriteA = Resources.Load<Sprite>("Sprites/" + a.state);
        var spriteB = Resources.Load<Sprite>("Sprites/" + b.state);

        if (!a.isEnemy) {
            img1.sprite = spriteA;
            logText.text = a.state + " beats " + b.state + "!";
            img2.sprite = spriteB;
        } else {
            img1.sprite = spriteB;
            logText.text = a.state + " beaten by " + b.state + "!";
            img2.sprite = spriteA;
        }
    }

    private void OnDraw(DieManager a, DieManager b)
    {
        logText.text = "Draw.";
    }
}
