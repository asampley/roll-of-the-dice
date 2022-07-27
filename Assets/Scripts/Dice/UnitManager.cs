using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public enum DiceState : uint
{
    Rock = 0,
    Paper = 1,
    Scissors = 2,
    Lich = 3,
    Blank = 4,
    King = 5,
}

public enum MovementPattern
{
    Single,
    Straight,
    Knight,
}

public class UnitManager : MonoBehaviour, PhaseListener
{
    public MonoBehaviour Self { get { return this; } }

    //Properties
    private string _unitName;
    public string UnitName
    {
        get { return _unitName; }
        set { _unitName = value; }
    }

    [SerializeField]
    private int _maxMoves;
    public int MaxMoves
    {
        get { return _maxMoves; }
        set { _maxMoves = value; }
    }
    private int _movesAvailable;
    public int MovesAvailable
    {
        get { return _movesAvailable; }
    }

    private MovementPattern _movementPattern;
    public MovementPattern MovementPattern
    {
        get { return _movementPattern; }
        set { _movementPattern = value; }
    }

    private bool _isEnemy;
    public bool IsEnemy
    {
        get { return _isEnemy; }
        set { _isEnemy = value; }
    }

    private DiceState _state;
    public DiceState State
    {
        get { return _state; }
        set { _state = value; }
    }

    private bool _isMoving;
    public bool IsMoving
    {
        get { return _isMoving; }
        set { _isMoving = value; }
    }

    private List<OverlayTile> _tilesInRange = new List<OverlayTile>();
    public OverlayTile parentTile;
    [SerializeField]
    private MeshRenderer _meshRenderer;
    [SerializeField]
    private GameObject _moveIndicator;
    [SerializeField]
    private EnemyAI _enemyAI;

    //Materials
    [HideInInspector]
    public Material ghostMaterial;
    public Material alliedMaterial;
    public Material enemyMaterial;
    public Material alliedGhostMaterial;
    public Material enemyGhostMaterial;

    public GameObject ghostComponents;
    private DieRotator _dieRotator;
    public DieRotator DieRotator { get { return _dieRotator; }
    }
    private DieTexturer _dieTexturer;
    public DieTexturer DieTexturer { get { return _dieTexturer; }
    }

    private TextMeshProUGUI nameText;

    public static event Action<UnitManager, UnitManager> ABeatsB;
    public static event Action<UnitManager, UnitManager> Draw;

    public List<Vector2Int> path = new List<Vector2Int>();

    private void Awake()
    {
        _dieRotator = GetComponentInChildren<DieRotator>();
        _dieTexturer = GetComponentInChildren<DieTexturer>();
    }

    static UnitManager() {
        ABeatsB += (a,b) => Debug.Log(a.State + "(" + a.name + ") beats " + b.State + "(" + b.name + ")");
        Draw += (a,b) => Debug.Log(a.State + "(" + a.name + ") draws with " + b.State + "(" + b.name + ")");
    }

    void Start() {
        nameText = GetComponentInChildren<TextMeshProUGUI>();
        nameText.enabled = false;
        nameText.text = this.name;

        GetComponentInChildren<DieTranslator>().ReachTarget += () => EventManager.TriggerEvent("Move");
    }

    void OnEnable() {
        GameManager.Instance.phaseManager.AllPhaseListeners.Add(this);
        DebugConsole.DebugNames += OnDebugNames;
    }

    void OnDisable() {
        GameManager.Instance.phaseManager.AllPhaseListeners.Remove(this);
        DebugConsole.DebugNames -= OnDebugNames;
    }

    private void Update()
    {
        if (!IsEnemy)
        {
            if (GameManager.Instance.PlayerPiecesMoved < GameManager.Instance.MaxPlayerMoves || GameManager.Instance.MovedPieces.Contains(this))
            {
                if (GameManager.Instance.phaseManager.CurrentPhase == Phase.Player && _movesAvailable > 0)
                    _moveIndicator.SetActive(true);
                else
                    _moveIndicator.SetActive(false);
            }
            else
            {
                _moveIndicator.SetActive(false);
            }
        }
    }

    public void Initialize(DiceOrientation orientation)
    {
        if (IsEnemy)
        {
            GameManager.Instance.EnemyCount++;
            _enemyAI.enabled = true;
            ghostMaterial = enemyGhostMaterial;
            GetComponentInChildren<MeshRenderer>().sharedMaterial = enemyMaterial;
        }
        else
        {
            GameManager.Instance.PlayerCount++;
            _enemyAI.enabled = false;
            Destroy(_enemyAI);
            ghostMaterial = alliedGhostMaterial;
            GetComponentInChildren<MeshRenderer>().sharedMaterial = alliedMaterial; ;
        }

        ResetRange();
        _dieRotator.RotateTileDelta(Vector2Int.right, orientation.xRolls);
        _dieRotator.RotateTileDelta(Vector2Int.up, orientation.yRolls);
        _dieRotator.RotateZ(orientation.zRolls);
        _dieRotator.RotateNow();
        State = _dieRotator.GetUpFace();
        IsMoving = false;
    }

    // consumes each step of the enumerator only after the last move has completed
    private async UniTask MoveMany(IEnumerator<OverlayTile> tiles, CancellationToken token) {
        IsMoving = true;
        if (tiles.MoveNext()) {
            OverlayTile tile;
            do {
                tile = tiles.Current;

                if (tile.IsBlocked || _movesAvailable <= 0) break;

                GetTilesInRange();
                if (!_tilesInRange.Contains(tile)) {
                    return;
                }
                await UpdateTilePos(tile, token);
            } while (tiles.MoveNext());

            _movesAvailable--;

            EventManager.TriggerEvent("SelectUnit");
            if (!IsEnemy)
            {
                if (!GameManager.Instance.MovedPieces.Contains(this))
                    {
                    GameManager.Instance.MovedPieces.Add(this);
                    GameManager.Instance.PlayerPiecesMoved += 1;
                }

                if (_movesAvailable <= 0)
                    GameManager.Instance.PlayerMoveRemaining--;
            }
        }
        IsMoving = false;
    }

    public async UniTask MoveAsync(OverlayTile newTile, CancellationToken token) {
        await MoveMany(new List<OverlayTile> { newTile }.GetEnumerator(), token);
    }

    public void Move(OverlayTile newTile) {
        MoveMany(new List<OverlayTile> { newTile }.GetEnumerator(), this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void Move(IEnumerator<OverlayTile> tiles) {
        MoveMany(tiles, this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void Select()
    {
        if (Globals.SELECTED_UNIT != null)
            Globals.SELECTED_UNIT.Deselect();
        Globals.SELECTED_UNIT = this;

        GhostManager.Instance.SetEnemyGhostsVisible(Input.GetKey("space"));
        GhostManager.Instance.SetGhostsVisible(gameObject, true);

        GetTilesInRange();
        ShowTilesInRange();

        EventManager.TriggerEvent("SelectUnit");
    }

    public void Deselect()
    {
        if (!IsEnemy)
            GhostManager.Instance.SetEnemyGhostsVisible(true);

        HideTilesInRange();
    }

    public void AddPath(OverlayTile tile)
    {
        var delta = tile.gridLocation - parentTile.gridLocation;

        Vector2Int step;
        int steps;

        if (delta.x != 0) {
            step = Math.Sign(delta.x) * Vector2Int.right;
            steps = Math.Abs(delta.x);

        } else if (delta.y != 0) {
            step = Math.Sign(delta.y) * Vector2Int.up;
            steps = Math.Abs(delta.y);
        } else {
            return;
        }

        for (int i = 0; i < steps; ++i) {
            path.Add(step);
        }
    }

    public async UniTask Fight()
    {
        List<UnitManager> toKill = new List<UnitManager>();

        foreach (OverlayTile tile in GetTilesAdjacent())
        {
            if (tile.occupyingDie != null && IsEnemy != tile.occupyingDie.IsEnemy)
            {
                UnitManager enemyDie = tile.occupyingDie;
                DiceState enemyState = enemyDie.State;

                switch (State) {
                    case DiceState.King:
                        switch (enemyState) {
                            case DiceState.Blank:
                                ABeatsB?.Invoke(this, enemyDie);

                                toKill.Add(enemyDie);
                                break;
                            case DiceState.Rock:
                            case DiceState.Scissors:
                            case DiceState.Paper:
                            case DiceState.Lich:
                                ABeatsB?.Invoke(enemyDie, this);

                                if (!this.IsEnemy) {
                                    GameManager.Instance.PlayerKingDefeated = true;
                                }

                                toKill.Add(this);

                                break;
                            case DiceState.King:
                                Draw?.Invoke(this, enemyDie);
                                break;
                            }
                        break;
                    case DiceState.Lich:
                        switch (enemyState) {
                            case DiceState.King:
                            case DiceState.Rock:
                            case DiceState.Paper:
                            case DiceState.Scissors:
                            case DiceState.Blank:
                                ABeatsB?.Invoke(this, enemyDie);

                                toKill.Add(enemyDie);
                                break;
                            case DiceState.Lich:
                                Draw?.Invoke(this, enemyDie);
                                break;
                        }
                        break;
                    case DiceState.Blank:
                        switch (enemyState) {
                            case DiceState.King:
                            case DiceState.Rock:
                            case DiceState.Scissors:
                            case DiceState.Paper:
                            case DiceState.Lich:
                                ABeatsB?.Invoke(enemyDie, this);

                                toKill.Add(this);
                                break;
                            case DiceState.Blank:
                                Draw?.Invoke(this, enemyDie);
                                break;
                        }
                        break;
                    case DiceState.Rock:
                        switch (enemyState) {
                            case DiceState.King:
                            case DiceState.Scissors:
                            case DiceState.Blank:
                                ABeatsB?.Invoke(this, enemyDie);

                                toKill.Add(enemyDie);
                                break;
                            case DiceState.Paper:
                            case DiceState.Lich:
                                ABeatsB?.Invoke(enemyDie, this);

                                toKill.Add(this);
                                break;
                            case DiceState.Rock:
                                Draw?.Invoke(this, enemyDie);
                                break;
                        }
                        break;
                    case DiceState.Paper:
                        switch (enemyState) {
                            case DiceState.King:
                            case DiceState.Rock:
                            case DiceState.Blank:
                                ABeatsB?.Invoke(this, enemyDie);

                                toKill.Add(enemyDie);
                                break;
                            case DiceState.Scissors:
                            case DiceState.Lich:
                                ABeatsB?.Invoke(enemyDie, this);

                                toKill.Add(this);
                                break;
                            case DiceState.Paper:
                                Draw?.Invoke(this, enemyDie);
                                break;
                        }
                        break;
                    case DiceState.Scissors:
                        switch (enemyState) {
                            case DiceState.King:
                            case DiceState.Paper:
                            case DiceState.Blank:
                                ABeatsB?.Invoke(this, enemyDie);

                                toKill.Add(enemyDie);
                                break;
                            case DiceState.Rock:
                            case DiceState.Lich:
                                ABeatsB?.Invoke(enemyDie, this);

                                toKill.Add(this);
                                break;
                            case DiceState.Scissors:
                                Draw?.Invoke(this, enemyDie);
                                break;
                        }
                        break;
                }
            }
        }

        foreach (UnitManager die in toKill)
        {
            if (!die.IsEnemy && die.State == DiceState.King) {
                GameManager.Instance.PlayerKingDefeated = true;
            }
            die.Kill();
        }
    }

    public void Kill()
    {
        Deselect();
        parentTile.RemoveDiceFromTile();
        if (IsEnemy)
            Destroy(gameObject.GetComponent<EnemyAI>());
        Destroy(gameObject);
    }

    public void ShowTilesInRange()
    {
        if (IsEnemy) return;

        GhostManager.Instance.RemoveGhosts(gameObject);
        foreach (OverlayTile tile in _tilesInRange)
        {
            if (tile.IsBlocked) continue;
            tile.ShowTile();
            Vector2Int delta = (Vector2Int)(tile.gridLocation - parentTile.gridLocation);
            Vector3 translation = MapManager.Instance.TileDeltaToWorldDelta(delta);
            var ghost = GhostManager.Instance.CreateGhost(gameObject, translation, delta, Math.Abs(delta.x) + Math.Abs(delta.y));
            ghost.GetComponentInChildren<DieRotator>().Collapse = true;
            ghost.GetComponentInChildren<DieTranslator>().Collapse = true;
        }
    }

    public void HideTilesInRange()
    {
        if (IsEnemy) return;

        GhostManager.Instance.RemoveGhosts(gameObject);
        foreach (OverlayTile tile in _tilesInRange)
            tile.HideTile();
    }

    private void GetTilesInRange()
    {
        _tilesInRange.Clear();
        if (_movesAvailable <= 0) return;

        if (MovementPattern == MovementPattern.Straight)
            _tilesInRange = GetTilesStraightLine();
        else if (MovementPattern == MovementPattern.Single)
            _tilesInRange = GetTilesAdjacent();
    }

    private List<OverlayTile> GetTilesAdjacent() {
        return MapManager.Instance.GetSurroundingTiles(new Vector2Int(parentTile.gridLocation.x, parentTile.gridLocation.y));
    }

    private List<OverlayTile> GetTilesStraightLine()
    {
        return MapManager.Instance.GetTilesStraightLine(new Vector2Int(parentTile.gridLocation.x, parentTile.gridLocation.y));
    }

    public void ResetRange()
    {
        _movesAvailable = MaxMoves;
    }

    public int MaxRange() {
        return MaxMoves;
    }

    private void MoveToPos(Vector2Int delta, bool rotate = true)
    {
        if (rotate) {
            GetComponentInChildren<DieRotator>().RotateTileDelta(delta);
        }

        GetComponentInChildren<DieTranslator>().Translate(
            MapManager.Instance.TileDeltaToWorldDelta(delta)
        );
    }

    private async UniTask UpdateTilePos(OverlayTile newTile, CancellationToken token, bool rotate = true)
    {
        MoveToPos((Vector2Int)(newTile.gridLocation - parentTile.gridLocation), rotate);
        State = _dieRotator.GetUpFace();
        parentTile.RemoveDiceFromTile();
        newTile.MoveDiceToTile(this);

        await UniTask.Delay(TimeSpan.FromSeconds(Globals.MOVEMENT_TIME + 0.1f), cancellationToken: token);
    }

    private async UniTask<bool> GetTileEffects(CancellationToken token)
    {
        TileType tileType = parentTile.data.TileType;
        var moved = false;
        switch (tileType)
        {
            case TileType.Normal:
                GetTilesInRange();
                break;
            case TileType.Stopping:
                _movesAvailable = 0;
                GetTilesInRange();
                break;
            case TileType.RotateClockwise:
                _dieRotator.RotateZ(1);
                await UniTask.Delay(TimeSpan.FromSeconds(Globals.MOVEMENT_TIME), cancellationToken: token);
                GetTilesInRange();
                break;
            case TileType.RotateCounterClockwise:
                _dieRotator.RotateZ(-1);
                GetTilesInRange();
                break;
            case TileType.ShovePosX:
                moved = await Shove(new Vector2Int(1, 0), token);
                GetTilesInRange();
                break;
            case TileType.ShovePosY:
                moved = await Shove(new Vector2Int(0, 1), token);
                GetTilesInRange();
                break;
            case TileType.ShoveNegX:
                moved = await Shove(new Vector2Int(-1, 0), token);
                GetTilesInRange();
                break;
            case TileType.ShoveNegY:
                moved = await Shove(new Vector2Int(0, -1), token);
                GetTilesInRange();
                break;
            case TileType.RemoveFace:
                _dieRotator.SetDownFace(DiceState.Blank);
                GetTilesInRange();
                break;
            case TileType.Randomize:
                _dieRotator.RotateZ(UnityEngine.Random.Range(0, _dieRotator.axes.FaceEdges));
                _dieRotator.RotateZ(UnityEngine.Random.Range(0, _dieRotator.axes.FaceEdges));
                _dieRotator.RotateZ(UnityEngine.Random.Range(0, _dieRotator.axes.FaceEdges));
                await UniTask.Delay(TimeSpan.FromSeconds(Globals.MOVEMENT_TIME), cancellationToken: token);
                GetTilesInRange();
                break;
            default:
                GetTilesInRange();
                break;
        }

        return moved;
    }

    public async UniTask<bool> Shove(Vector2Int dir, CancellationToken token)
    {
        Vector2Int newPos = (Vector2Int)parentTile.gridLocation + dir;

        var tile = MapManager.Instance.GetTileAtPos(newPos);

        if (tile.IsBlocked) return false;

        await UpdateTilePos(tile, token, false);

        return true;
    }

    public PhaseStepResult OnPhaseEnter(Phase phase) {
        switch (phase) {
            case Phase.Enemy:
                if (IsEnemy) {
                    ResetRange();
                    return PhaseStepResult.Unchanged;
                }
                return PhaseStepResult.Done;
            case Phase.Player:
                if (!IsEnemy) {
                    ResetRange();
                    return PhaseStepResult.Passive;
                } else {
                    return PhaseStepResult.Done;
                }
            case Phase.TileEffects:
            case Phase.Fight:
                return PhaseStepResult.Unchanged;
            default:
                return PhaseStepResult.Done;
        }

    }

    public async UniTask<PhaseStepResult> OnPhaseStep(Phase phase, CancellationToken token) {
        switch (phase) {
            case Phase.Player:
                if (IsEnemy) return PhaseStepResult.Done;

                if (path.Count == 0) {
                    return PhaseStepResult.Passive;
                } else {
                    if (Globals.SELECTED_UNIT == this) {
                        HideTilesInRange();
                    }
                    await StepPath(token);
                    if (Globals.SELECTED_UNIT == this && path.Count == 0) {
                        GetTilesInRange();
                        ShowTilesInRange();
                    }
                    return PhaseStepResult.Changed;
                }
            case Phase.Enemy:
                if (!IsEnemy) return PhaseStepResult.Done;

                if (path.Count == 0) {
                    return PhaseStepResult.Done;
                } else {
                    await StepPath(token);
                    return PhaseStepResult.Changed;
                }
            case Phase.Fight:
                await Fight();
                return PhaseStepResult.Done;
            case Phase.TileEffects:
                if (await GetTileEffects(token)) {
                    return PhaseStepResult.Changed;
                } else {
                    return PhaseStepResult.Done;
                }
            default:
                return PhaseStepResult.Done;
        }
    }

    void OnDebugNames() {
        nameText.enabled = !nameText.enabled;
    }

    void OnDestroy() {
        GhostManager.Instance.RemoveGhosts(gameObject);

        if (Globals.SELECTED_UNIT == this) {
            Globals.SELECTED_UNIT = null;
        }

        if (IsEnemy)
        {
            GameManager.Instance.EnemyCount--;
        }
        else
        {
            GameManager.Instance.PlayerMoveRemaining--;
            GameManager.Instance.PlayerCount--;
        }
    }

    public async UniTask StepPath(CancellationToken token) {
        Debug.Log("Garfield Starting StepPath: " + transform.name);
        Debug.Log("Following Path: " + PathStr());

        if (path.Count > 0) {
            OverlayTile tile;
            try {
                tile = MapManager.Instance.GetTileAtPos(
                    (Vector2Int)parentTile.gridLocation + path[0]
                );

                path.RemoveAt(0);
            } catch (KeyNotFoundException) {
                Debug.Log("Tile does not exist, stopping path");
                path.Clear();

                return;
            }

            await MoveAsync(tile, token);
        }

        Debug.Log("Garfield Ending StepPath: " + transform.name);
    }

    public string PathStr() {
        return (Vector2Int)parentTile.gridLocation + " -> " + Utilities.EnumerableString(path);
    }
}
