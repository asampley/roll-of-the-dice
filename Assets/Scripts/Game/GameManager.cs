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

    private static uint DieSpawnID = 0;

    //Dice
    public MapData mapData;
    public GameRulesData gameRulesData;
    public GameObject diceParent;

    //Dice Prefabs
    private string _dicePrefabLocation = "Prefabs/DiceClasses/";
    private Dictionary<DiceClass, GameObject> _dicePrefabs = new Dictionary<DiceClass, GameObject>();

    private Dictionary<DiceSpawn, DiceOrientation> alliedSpawnPositions = new Dictionary<DiceSpawn, DiceOrientation>();
    private Dictionary<DiceSpawn, DiceOrientation> enemySpawnPositions = new Dictionary<DiceSpawn, DiceOrientation>();

    private HashSet<EnemyAI> enemiesWaiting = new HashSet<EnemyAI>();

    private int _enemies;
    public int EnemyCount {
        get { return _enemies; }
        set { _enemies = value; CheckWin(); }
    }

    private int _players;
    public int PlayerCount {
        get { return _players; }
        set { _players = value; SetMaxMoves(); CheckWin(); }
    }

    private bool _playerKingDefeated;
    public bool PlayerKingDefeated {
        get { return _playerKingDefeated; }
        set { _playerKingDefeated = value; CheckWin(); }
    }

    public event Action<Win> WinEvent;

    private int _maxPlayerMoves;
    public int MaxPlayerMoves
    {
        get { return _maxPlayerMoves; }
        set { _maxPlayerMoves = value; }
    }

    private int _playerMoveRemaining;
    public int PlayerMoveRemaining {
        get { return _playerMoveRemaining; }
        set { _playerMoveRemaining = value; }
    }

    private int _playerpiecesMoved;
    public int PlayerPiecesMoved
    {
        get { return _playerpiecesMoved; }
        set { _playerpiecesMoved = value; }
    }
    private List<DieManager> _movedPieces = new List<DieManager>();
    public List<DieManager> MovedPieces
    {
        get { return _movedPieces; }
        set { _movedPieces = value; }
    }

    private int _turnsRemaining;
    public int TurnsRemaining
    {
        get { return _turnsRemaining; }
        set { _turnsRemaining = value; }
    }

    private int _currentRound;
    public int CurrentRound
    {
        get { return _currentRound; }
        set { _currentRound = value; CheckWin(); }
    }

    private int _maxNumberOfTurns;

    public int MaxNumberOfTurns
    {
        get { return _maxNumberOfTurns; }
        set { _maxNumberOfTurns = value; }
    }

    private Turn _turnValue;
    public Turn CurrentTurnValue {
        get { return _turnValue; }
        private set {
            if (_turnValue != value) {
                _turnValue = value;
                PostTurnChange();
                TurnChange?.Invoke(_turnValue);
            }
        }
    }
    public event Action<Turn> TurnChange;


    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        TurnChange += t => Debug.Log("Turn: " + t);
    }

    private void Start()
    {
        FindPrefabs();
        RollPositions();
        StartGame();
    }

    private void FindPrefabs()
    {
        foreach (DiceClass diceClass in Enum.GetValues(typeof(DiceClass)))
            _dicePrefabs.Add(diceClass, Resources.Load<GameObject>(_dicePrefabLocation + diceClass));
    }

    public void SpawnDie(Vector2Int startPos, DiceClass diceClass, bool isEnemy, DiceOrientation orientation)
    {
        GameObject prefab = _dicePrefabs[diceClass];

        Vector3 pos = MapManager.Instance.TileToWorldSpace(startPos);
        GameObject die = Instantiate(prefab, pos, Quaternion.identity);
        die.transform.SetParent(diceParent.transform);
        die.name = prefab.name + (isEnemy ? " Enemy " : " Player ") + (DieSpawnID++);
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
        foreach (DiceSpawn spawn in mapData.alliedDice)
            alliedSpawnPositions.Add(spawn, GenerateDiceOrientation());
        foreach (DiceSpawn spawn in mapData.enemyDice)
            enemySpawnPositions.Add(spawn, GenerateDiceOrientation());
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
        CurrentTurnValue = Turn.Setup;
        PlayerKingDefeated = false;
        MaxNumberOfTurns = gameRulesData.maxTurns;
        CurrentRound = 1;

        ClearMap();

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

        StartCoroutine(SleepyTurnSwitch(Turn.Player));
    }

    public IEnumerator SleepyTurnSwitch(Turn turn) {
        yield return new WaitForFixedUpdate();
        CurrentTurnValue = turn;
    }

    public void RerollGame()
    {
        RollPositions();
        StartGame();
    }

    public void ClearMap()
    {
        foreach(Transform child in diceParent.transform)
            GameObject.Destroy(child.gameObject);
        MapManager.Instance.ClearMap();
        MapManager.Instance.GenerateMap();
    }
    public void ClearDictionaries()
    {
        alliedSpawnPositions.Clear();
        enemySpawnPositions.Clear();
    }

    public void CheckWin() {
        if (CurrentTurnValue == Turn.Setup) return;

        if (CurrentRound >= MaxNumberOfTurns && gameRulesData.turnLimit)
            WinEvent?.Invoke(Win.Enemy);
        else if (PlayerCount == 0 || PlayerKingDefeated)
            WinEvent?.Invoke(Win.Enemy);
        else if (EnemyCount == 0)
            WinEvent?.Invoke(Win.Player);
    }

    public void AddEnemyWaiting(EnemyAI enemy) {
        enemiesWaiting.Add(enemy);

        string str = Utilities.EnumerableString(enemiesWaiting.Select(e => e.name));
        Debug.Log("Still waiting for " + str);
    }

    public void RemoveEnemyWaiting(EnemyAI enemy) {
        Debug.Log("Removing enemy");
        enemiesWaiting.Remove(enemy);

        string str = Utilities.EnumerableString(enemiesWaiting.Select(e => e.name));
        Debug.Log("Still waiting for " + str);

        if (CurrentTurnValue == Turn.Enemy && enemiesWaiting.Count == 0)
            CurrentTurnValue = Turn.Player;
    }

    private void PostTurnChange() {
        if (CurrentTurnValue == Turn.Player) {
            CurrentRound++;
            PlayerPiecesMoved = 0;
            MovedPieces.Clear();
            _playerMoveRemaining = _maxPlayerMoves;
        }
    }

    public void SetMaxMoves()
    {
        if (gameRulesData.canMoveAll)
            MaxPlayerMoves = PlayerCount;
        else
            MaxPlayerMoves = gameRulesData.playerUnitsToMove;
    }

    public void PieceOutOfMoves()
    {
        _playerMoveRemaining--;
        if (_playerMoveRemaining <= 0 && CurrentTurnValue == Turn.Player)
            CurrentTurnValue = Turn.Enemy;
    }
}
