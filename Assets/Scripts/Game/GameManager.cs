using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum Win
{
    Player,
    Enemy,
}

public class GameManager : MonoBehaviour, PhaseListener
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public MonoBehaviour Self { get { return this; } }

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

    public PhaseManager phaseManager = new PhaseManager();

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

    public int PlayerSteps;

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

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        phaseManager.AllPhaseListeners.Add(this);
    }

    private void Start()
    {
        FindPrefabs();
        RollPositions();
        StartGame();
        RunPhaseUpdate().Forget();
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
        phaseManager.Clear();
        phaseManager.Push(Phase.Setup);
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
        if (phaseManager.CurrentPhase == Phase.Setup) return;

        if (CurrentRound >= MaxNumberOfTurns && gameRulesData.turnLimit)
            WinEvent?.Invoke(Win.Enemy);
        else if (PlayerCount == 0 || PlayerKingDefeated)
            WinEvent?.Invoke(Win.Enemy);
        else if (EnemyCount == 0)
            WinEvent?.Invoke(Win.Player);
    }

    private async UniTask RunPhaseUpdate() {
        Debug.Log("Start Run Phase Update: " + phaseManager.CurrentPhase);

        while (true) {
            await phaseManager.PhaseUpdate();

            TryAdvancePhase();
        }
    }

    private bool TryAdvancePhase() {
        var popped = false;

        while (true) {
            var results = phaseManager.CurrentPhaseResults();

            Debug.Log("TryAdvancePhase: " + Utilities.EnumerableString(results));
            switch (phaseManager.CurrentPhase) {
                case null:
                case Phase.Setup:
                    phaseManager.Transition(Phase.Player);
                    return true;
                case Phase.Enemy:
                    if (!popped) {
                        phaseManager.Push(Phase.Fight);
                        return true;
                    } else if (results.Any(r => r == PhaseStepResult.ShouldContinue)) {
                        return false;
                    } else {
                        phaseManager.Transition(Phase.Player);
                        return true;
                    }
                case Phase.Player:
                    if (!popped) {
                        phaseManager.Push(Phase.Fight);
                        return true;
                    } else if (results.Any(r => r == PhaseStepResult.ShouldContinue)) {
                        return false;
                    } else {
                        phaseManager.Transition(Phase.Enemy);
                        return true;
                    }
                case Phase.Fight:
                    phaseManager.Pop();
                    popped = true;
                    continue;
                default:
                    Debug.LogError("Unreconized phase. Cannot advance");
                    return false;
            }
        }
    }

    public bool OnPhaseEnter(Phase phase) {
        switch (phase) {
            case Phase.Setup:
                return true;
            case Phase.Player:
                CurrentRound++;
                PlayerPiecesMoved = 0;
                MovedPieces.Clear();
                _playerMoveRemaining = _maxPlayerMoves;
                return true;
            default:
                return false;
        }
    }

    public async UniTask<PhaseStepResult> OnPhaseUpdate(Phase phase, CancellationToken token) {
        switch (phase) {
            case Phase.Setup:
                await UniTask.DelayFrame(1);
                return PhaseStepResult.Done;
            case Phase.Player:
                await UniTask.WaitUntilValueChanged(this, m => m.PlayerSteps);
                if (_playerMoveRemaining <= 0) {
                    return PhaseStepResult.Done;
                } else {
                    return PhaseStepResult.ShouldContinue;
                }
            default:
                return PhaseStepResult.Done;
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
