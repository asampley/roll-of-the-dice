using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;
using Cysharp.Threading.Tasks;

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

    public void Start()
    {
        GameManager.Instance.WinEvent += w => ShowVictoryScreen(w == Win.Player);
    }

    public void OpenMenu()
    {
        menuButton.SetActive(false);
        menus.SetActive(true);
    }

    private void OnEnable()
    {
        UnitManager.ABeatsB += OnABeatsB;
        UnitManager.Draw += OnDraw;
        UnitManager.SelectUnit += OnSelectUnit;
    }

    private void OnDisable()
    {
        UnitManager.ABeatsB -= OnABeatsB;
        UnitManager.Draw -= OnDraw;
        UnitManager.SelectUnit -= OnSelectUnit;
    }


    // MENU -------------------------------------------------
    public void RestartLevel()
    {
        GameManager.Instance.WinEvent += w => ShowVictoryScreen(w == Win.Player);
        DataHandler.ClearData();
        Back();
        GameManager.Instance.SetupGame().Forget();
    }

    public void RerollLevel()
    {
        GameManager.Instance.WinEvent += w => ShowVictoryScreen(w == Win.Player);
        DataHandler.ClearData();
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
        if (GameManager.Instance.WinState == Win.None)
            DataHandler.SaveGameData();
        CoreBooter.Instance.LoadMenu();
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
    private void OnSelectUnit(UnitManager unit)
    {
        foreach (Transform child in inspector.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        gameInfo.SetActive(true);
        diceName.text = unit.UnitName;
        movesAvailable.text = "Moves Available: " + unit.MovesAvailable.ToString();
        GameObject inspectDie = unit.ghostComponents.gameObject;
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

    public void NextLevel()
    {
        LevelData level = CoreDataHandler.Instance.LevelData.nextLevel;
        CoreDataHandler.Instance.SetLevelID(level);
        CoreBooter.Instance.LoadLevel(level);
    }

    public void ShowVictoryScreen(bool victory) {
        menuTitle.text = victory ? "Victory" : "Defeat";
        menuTitle.color = victory
            ? new Color32(0, 255, 0, 255)
            : new Color32(255, 0, 0, 255);

        nextLevelButton.interactable = victory;

        OpenMenu();
    }

    private void OnABeatsB(UnitManager a, UnitManager b) {
        var spriteA = Resources.Load<Sprite>("Sprites/" + a.State);
        var spriteB = Resources.Load<Sprite>("Sprites/" + b.State);

        if (!a.IsEnemy) {
            img1.sprite = spriteA;
            logText.text = a.State + " beats " + b.State + "!";
            img2.sprite = spriteB;
        } else {
            img1.sprite = spriteB;
            logText.text = a.State + " beaten by " + b.State + "!";
            img2.sprite = spriteA;
        }
    }

    private void OnDraw(UnitManager a, UnitManager b)
    {
        logText.text = "Draw.";
    }
}
