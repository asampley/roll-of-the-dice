using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum Turn
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
    public GameObject normalPrefab;
    public GameObject rockPrefab;

    private Dictionary<DiceSpawn, DiceOrientation> alliedSpawnPositions = new Dictionary<DiceSpawn, DiceOrientation>();
    private Dictionary<DiceSpawn, DiceOrientation> enemySpawnPositions = new Dictionary<DiceSpawn, DiceOrientation>();

    public HashSet<EnemyAI> EnemiesWaiting = new HashSet<EnemyAI>();
    private int _enemies;
    public int EnemyCount {
        get { return _enemies; }
        set { _enemies = value; if (_enemies == 0) EnemiesGone?.Invoke(); }
    }

    private int _players;
    public int PlayerCount {
        get { return _players; }
        set { _players = value; if (_players == 0) PlayerDead?.Invoke(); }
    }

    public event Action EnemiesGone;
    public event Action PlayerDead;

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

        EnemiesGone += () => Debug.Log("All enemies dead");
    }

    private void Start()
    {
        RollPositions();
        StartGame();
        CurrentTurn = Turn.Player;
    }

    public void SpawnDie(Vector2Int startPos, DiceClass diceClass, bool isEnemy, DiceOrientation orientation)
    {
        GameObject prefab;
        switch (diceClass)
        {
            case DiceClass.Normal:
                prefab = normalPrefab;
                break;
            case DiceClass.Rock:
                prefab = rockPrefab;
                break;
            default:
                prefab = normalPrefab;
                break;
        }

        Vector3 pos = MapManager.Instance.GetTileWorldSpace(startPos);
        GameObject die = Instantiate(prefab, pos, Quaternion.identity);
        die.transform.SetParent(diceParent.transform);
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
        ClearMap();
        foreach (KeyValuePair<DiceSpawn, DiceOrientation> die in alliedSpawnPositions)
        {
            SpawnDie(die.Key.tilePosition, die.Key.diceClass, false, die.Value);
        }
        foreach (KeyValuePair<DiceSpawn, DiceOrientation> die in enemySpawnPositions)
        {
            SpawnDie(die.Key.tilePosition, die.Key.diceClass, true, die.Value);
        }
    }

    public void RerollGame()
    {
        RollPositions();
        StartGame();
    }

    public void ClearMap()
    {
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
}
