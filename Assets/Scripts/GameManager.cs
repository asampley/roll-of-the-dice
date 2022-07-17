using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum Turn
{
    Setup,
    Player,
    Enemy,
}

public enum Win
{
    Player,
    Enemy,
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    //Dice
    public StartPositions startPositions;
    public GameObject diceParent;

    //Dice Prefabs
    public GameObject pawnPrefab;
    public GameObject rookPrefab;
    public GameObject artisanPrefab;
    public GameObject trebuchetPrefab;
    public GameObject errantKnightPrefab;
    public GameObject lichPrefab;
    public GameObject kingPrefab;

    private Dictionary<DiceSpawn, DiceOrientation> alliedSpawnPositions = new Dictionary<DiceSpawn, DiceOrientation>();
    private Dictionary<DiceSpawn, DiceOrientation> enemySpawnPositions = new Dictionary<DiceSpawn, DiceOrientation>();

    public HashSet<EnemyAI> EnemiesWaiting = new HashSet<EnemyAI>();
    private int _enemies;
    public int EnemyCount {
        get { return _enemies; }
        set { _enemies = value; CheckWin(); }
    }

    private int _players;
    public int PlayerCount {
        get { return _players; }
        set { _players = value; CheckWin(); }
    }

    private bool _playerKingDefeated;
    public bool PlayerKingDefeated {
        get { return _playerKingDefeated; }
        set { _playerKingDefeated = value; CheckWin(); }
    }

    public event Action<Win> WinEvent;

    private int _playerMoveRemaining;
    public int PlayerMoveRemaining {
        get { return _playerMoveRemaining; }
        set {
            _playerMoveRemaining = value;
            if (CurrentTurn == Turn.Player && _playerMoveRemaining <= 0) {
                CurrentTurn = Turn.Enemy;
            }
        }
    }

    private Turn _turn;
    public Turn CurrentTurn {
        get { return _turn; }
        set { _turn = value; TurnChange?.Invoke(_turn); }
    }
    public event Action<Turn> TurnChange;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        TurnChange += t => Debug.Log("Turn: " + t);
    }

    private void Start()
    {
        CurrentTurn = Turn.Setup;
        RollPositions();
        StartGame();
    }

    public void SpawnDie(Vector2Int startPos, DiceClass diceClass, bool isEnemy, DiceOrientation orientation)
    {
        GameObject prefab;
        switch (diceClass)
        {
            case DiceClass.Pawn:
                prefab = pawnPrefab;
                break;
            case DiceClass.Rook:
                prefab = rookPrefab;
                break;
            case DiceClass.Artisan:
                prefab = artisanPrefab;
                break;
            case DiceClass.Trebuchet:
                prefab = trebuchetPrefab;
                break;
            case DiceClass.ErrantKnight:
                prefab = errantKnightPrefab;
                break;
            case DiceClass.Lich:
                prefab = lichPrefab;
                break;
            case DiceClass.King:
                prefab = kingPrefab;
                break;
            default:
                prefab = pawnPrefab;
                break;
        }

        Vector3 pos = MapManager.Instance.GetTileWorldSpace(startPos);
        GameObject die = Instantiate(prefab, pos, Quaternion.identity);
        die.transform.SetParent(diceParent.transform);
        die.name = prefab.name + (isEnemy ? " Enemy" : " Player");
        DieManager dieManager = die.GetComponent<DieManager>();
        var placedOnTile = MapManager.Instance.GetTileAtPos(startPos);

        dieManager.Initialize(isEnemy, orientation);
        if (placedOnTile != null)
        {
            GameObject overlayTile = placedOnTile.gameObject;
            OverlayTile overlayTileManager = overlayTile.GetComponent<OverlayTile>();

            overlayTileManager.MoveDiceToTile(dieManager);
        }
        else
        {
            Debug.LogError("Dice spawning off map.");
        }
    }

    public void RollPositions()
    {
        ClearDictionaries();
        foreach (DiceSpawn spawn in startPositions.alliedDice)
        {
            alliedSpawnPositions.Add(spawn, GenerateDiceOrientation());
        }
        foreach (DiceSpawn spawn in startPositions.enemyDice)
        {
            enemySpawnPositions.Add(spawn, GenerateDiceOrientation());
        }
    }

    public DiceOrientation GenerateDiceOrientation()
    {
        DiceOrientation orientation = new DiceOrientation();

        orientation.xRolls = UnityEngine.Random.Range(0, 4);
        orientation.yRolls = UnityEngine.Random.Range(0, 4);
        orientation.zRolls = UnityEngine.Random.Range(0, 4);

        return orientation;
    }

    public void StartGame()
    {
        CurrentTurn = Turn.Setup;
        ClearMap();

        PlayerKingDefeated = false;

        Debug.Log("player count " + PlayerCount + " enemy count " + EnemyCount + " player move remaining " + PlayerMoveRemaining);
        foreach (KeyValuePair<DiceSpawn, DiceOrientation> die in alliedSpawnPositions)
        {
            SpawnDie(die.Key.tilePosition, die.Key.diceClass, false, die.Value);
        }
        foreach (KeyValuePair<DiceSpawn, DiceOrientation> die in enemySpawnPositions)
        {
            SpawnDie(die.Key.tilePosition, die.Key.diceClass, true, die.Value);
        }
        Debug.Log("player count " + PlayerCount + " enemy count " + EnemyCount + " player move remaining " + PlayerMoveRemaining);


        StartCoroutine(SleepyStart());
    }

    public IEnumerator SleepyStart() {
        yield return new WaitForFixedUpdate();
        CurrentTurn = Turn.Player;
    }

    public void RerollGame()
    {
        RollPositions();
        StartGame();
    }

    public void ClearMap()
    {
        // seems necessary to fix a bug with persistent state
        EnemyPathManager.Instance.ResetReserved();

        foreach(Transform child in diceParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        MapManager.Instance.ClearMap();
        MapManager.Instance.GenerateMap();
    }
    public void ClearDictionaries()
    {
        alliedSpawnPositions.Clear();
        enemySpawnPositions.Clear();
    }

    public void CheckWin() {
        if (CurrentTurn == Turn.Setup) return;

        if (PlayerCount == 0 || PlayerKingDefeated) {
            WinEvent?.Invoke(Win.Enemy);
        } else if (EnemyCount == 0) {
            WinEvent?.Invoke(Win.Player);
        }
    }
}
