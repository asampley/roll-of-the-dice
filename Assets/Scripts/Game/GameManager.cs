using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum Phase
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

public interface PhaseListener {
    string name { get; }

    // called once when the phase changes
    void OnPhaseChange(Phase phase);

    // async function that is called again once all listeners have completed a step in the phase
    // only called if the item has been added to phase processing
    IEnumerator OnPhaseUpdate(Phase phase);
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

    private HashSet<PhaseListener> phaseProcessing = new HashSet<PhaseListener>();

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
        set { _playerMoveRemaining = value; TryAdvancePhase(); }
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

    private Phase _phase;
    public Phase CurrentPhase {
        get { return _phase; }
        private set {
            if (_phase != value) {
                _phase = value;
                PostPhaseChange();
                PhaseChange?.Invoke(_phase);
                StartCoroutine(RunPhaseUpdate());
            }
        }
    }
    public event Action<Phase> PhaseChange;


    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        PhaseChange += p => Debug.Log("Phase: " + p);
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
        CurrentPhase = Phase.Setup;
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

        StartCoroutine(SleepyPhaseSwitch(Phase.Player));
    }

    public IEnumerator SleepyPhaseSwitch(Phase phase) {
        yield return new WaitForFixedUpdate();
        CurrentPhase = phase;
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
        if (CurrentPhase == Phase.Setup) return;

        if (CurrentRound >= MaxNumberOfTurns && gameRulesData.turnLimit)
            WinEvent?.Invoke(Win.Enemy);
        else if (PlayerCount == 0 || PlayerKingDefeated)
            WinEvent?.Invoke(Win.Enemy);
        else if (EnemyCount == 0)
            WinEvent?.Invoke(Win.Player);
    }

    // should only be called in OnPhaseChange
    public void AddPhaseProcessing(PhaseListener listener) {
        phaseProcessing.Add(listener);

        string str = Utilities.EnumerableString(phaseProcessing.Select(e => e.name));
        Debug.Log("Still waiting for " + str);
    }

    // must be a coroutine to remove only after a frame has passed
    public void RemovePhaseProcessing(PhaseListener listener) {
        phaseProcessing.Remove(listener);

        string str = Utilities.EnumerableString(phaseProcessing.Select(e => e.name));
        Debug.Log("Still waiting for " + str);

        TryAdvancePhase();
    }

    private IEnumerator RunPhaseUpdate() {
        List<Coroutine> coroutines = new List<Coroutine>();

        do {
            // copy list to protect from manipulation in the middle of processing
            List<PhaseListener> toUpdate = new List<PhaseListener>(phaseProcessing);

            coroutines = toUpdate.Select(l => StartCoroutine(l.OnPhaseUpdate(CurrentPhase))).ToList();

            foreach (var coroutine in coroutines) {
                yield return coroutine;
            }
        } while (!TryAdvancePhase());
    }

    private bool TryAdvancePhase() {
        if (phaseProcessing.Count != 0) return false;

        switch (CurrentPhase) {
            case Phase.Setup:
                CurrentPhase = Phase.Player;
                return true;
            case Phase.Enemy:
                CurrentPhase = Phase.Player;
                return true;
            case Phase.Player:
                if (_playerMoveRemaining <= 0) {
                    CurrentPhase = Phase.Enemy;
                }
                return true;
            default:
                Debug.LogError("Unreconized phase. Cannot advance");
                return false;
        }
    }

    private void PostPhaseChange() {
        if (CurrentPhase == Phase.Player) {
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
}
