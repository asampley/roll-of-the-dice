using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

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
    //Dice
    public LevelData levelData;
    public GameRulesData gameRulesData;
    public GameObject diceParent;

    private Dictionary<DiceSpawn, DiceOrientation> alliedSpawnPositions = new Dictionary<DiceSpawn, DiceOrientation>();
    private Dictionary<DiceSpawn, DiceOrientation> enemySpawnPositions = new Dictionary<DiceSpawn, DiceOrientation>();

    private CancellationTokenSource phaseUpdateCancel = new CancellationTokenSource();
    public PhaseManager phaseManager = new PhaseManager();

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
    private List<UnitManager> _movedPieces = new List<UnitManager>();
    public List<UnitManager> MovedPieces
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
        DataHandler.LoadGameData();

        if (_instance == null)
            _instance = this;

        phaseManager.AllPhaseListeners.Add(this);
    }

    private void Start()
    {
        Debug.Log("START NEW GAME");
        levelData = CoreDataHandler.Instance.LevelData;
        gameRulesData = levelData.gameRules;
        RollPositions();
        StartGame();
    }

    public void SpawnDie(Vector2Int startPos, DiceClass diceClass, bool isEnemy, DiceOrientation orientation)
    {
        UnitData unitData = Globals.UNIT_DATA.Where((UnitData x) => (int)x.unitClass == (int)diceClass).First();
        Unit die = new Unit(unitData, isEnemy, orientation);
        die.SetPosition(startPos);
    }

    public void RollPositions()
    {
        ClearDictionaries();
        foreach (DiceSpawn spawn in levelData.alliedDice)
            alliedSpawnPositions.Add(spawn, GenerateDiceOrientation());
        foreach (DiceSpawn spawn in levelData.enemyDice)
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
        phaseUpdateCancel?.Cancel();
        phaseUpdateCancel?.Dispose();
        phaseUpdateCancel = new CancellationTokenSource();

        phaseManager.Clear();
        phaseManager.Push(Phase.Setup);

        

        PlayerKingDefeated = false;
        MaxNumberOfTurns = gameRulesData.maxTurns;
        CurrentRound = 1;

        ClearMap();

        Debug.Log("player count " + PlayerCount + " enemy count " + EnemyCount + " player move remaining " + PlayerMoveRemaining);
        foreach (KeyValuePair<DiceSpawn, DiceOrientation> die in alliedSpawnPositions)
            SpawnDie(die.Key.tilePosition, die.Key.diceClass, false, die.Value);
        foreach (KeyValuePair<DiceSpawn, DiceOrientation> die in enemySpawnPositions)
            SpawnDie(die.Key.tilePosition, die.Key.diceClass, true, die.Value);
        Debug.Log("player count " + PlayerCount + " enemy count " + EnemyCount + " player move remaining " + PlayerMoveRemaining);
        RunPhaseUpdate(phaseUpdateCancel.Token).Forget();
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
        Debug.Log("Finished clearing map");
    }

    public void ClearDictionaries()
    {
        alliedSpawnPositions.Clear();
        enemySpawnPositions.Clear();
    }

    public void CheckWin()
    {
        if (phaseManager.CurrentPhase == Phase.Setup) return;

        if (CurrentRound >= MaxNumberOfTurns && gameRulesData.turnLimit)
            WinEvent?.Invoke(Win.Enemy);
        else if (PlayerCount == 0 || PlayerKingDefeated)
            WinEvent?.Invoke(Win.Enemy);
        else if (EnemyCount == 0)
            WinEvent?.Invoke(Win.Player);
    }

    public void CheckWin(bool player)
    {
        if (player)
            WinEvent?.Invoke(Win.Player);
        else
            WinEvent?.Invoke(Win.Enemy);
    }

    private async UniTask RunPhaseUpdate(CancellationToken token) {
        Debug.Log("Start Run Phase Update: " + phaseManager.CurrentPhase);

        while (true) {
            await phaseManager.PhaseStep(token);

            if (token.IsCancellationRequested) {
                return;
            }

            TryAdvancePhase();

            await UniTask.DelayFrame(1, cancellationToken: token);
        }
    }

    private void TryAdvancePhase() {
        var results = phaseManager.CurrentPhaseResults();

        switch (phaseManager.CurrentPhase) {
            case null:
            case Phase.Setup:
                phaseManager.Transition(Phase.Player);
                break;
            case Phase.Enemy:
                if (results.Any(r => r == PhaseStepResult.Changed)) {
                    phaseManager.Push(Phase.TileEffects);
                    phaseManager.Push(Phase.Fight);
                } else if (results.Any(r => r == PhaseStepResult.Unchanged)) {
                    // do not transition
                } else {
                    phaseManager.Transition(Phase.Player);
                }
                break;
            case Phase.Player:
                if (results.Any(r => r == PhaseStepResult.Changed)) {
                    phaseManager.Push(Phase.TileEffects);
                    phaseManager.Push(Phase.Fight);
                } else if (results.Any(r => r == PhaseStepResult.Unchanged)) {
                    // do not transition
                } else {
                    phaseManager.Transition(Phase.Enemy);
                }
                break;
            case Phase.Fight:
                phaseManager.Pop();
                break;
            case Phase.TileEffects:
                if (results.Any(r => r == PhaseStepResult.Changed)) {
                    phaseManager.Push(Phase.Fight);
                } else {
                    phaseManager.Pop();
                }
                break;
            default:
                Debug.LogError("Unreconized phase. Cannot advance");
                break;
        }
    }

    public PhaseStepResult OnPhaseEnter(Phase phase) {
        switch (phase) {
            case Phase.Setup:
                return PhaseStepResult.Unchanged;
            case Phase.Player:
                SetMaxMoves();
                CurrentRound++;
                PlayerPiecesMoved = 0;
                MovedPieces.Clear();
                _playerMoveRemaining = _maxPlayerMoves;
                return PhaseStepResult.Unchanged;
            default:
                return PhaseStepResult.Done;
        }
    }

    public async UniTask<PhaseStepResult> OnPhaseStep(Phase phase, CancellationToken token) {
        switch (phase) {
            case Phase.Setup:
                await UniTask.DelayFrame(1);
                return PhaseStepResult.Done;
            case Phase.Player:
                if (_playerMoveRemaining <= 0) {
                    return PhaseStepResult.Done;
                } else {
                    return PhaseStepResult.Unchanged;
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

    public void OnDestroy()
    {
        Debug.Log("Destroying Game Manager");
        _instance = null;
        phaseManager = null;
    }
}
