using System;
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

public class UnitManager : MonoBehaviour, IPhaseListener
{
    public MonoBehaviour Self { get { return this; } }

    //Properties
    private Unit _unit;
    public Unit Unit
    {
        get { return _unit; }
        set { _unit = value; }
    }
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
        set { _movesAvailable = value; }
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
    public DiceOrientation Orientation
    {
        get => _dieRotator.GetOrientation();
        set => _dieRotator.SetOrientation(value);
    }

    public Vector2Int Position { get => (Vector2Int)parentTile.gridLocation; }

    private List<OverlayTile> _tilesInRange = new();
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

    public GameObject ghostComponents;

    [SerializeField]
    private DieRotator _dieRotator;
    public DieRotator DieRotator { get => _dieRotator; }
    [SerializeField]
    private DieTexturer _dieTexturer;
    public DieTexturer DieTexturer { get => _dieTexturer; }

    private bool _toTileEffect = false;
    private bool _toFight = false;

    private TextMeshProUGUI nameText;

    public static event Action MoveTile;
    public static event Action<UnitManager> SelectUnit;
    public static event Action<UnitManager, UnitManager> ABeatsB;
    public static event Action<UnitManager, UnitManager> Draw;

    public List<Vector2Int> path = new();


    static UnitManager() {
        ABeatsB += (a,b) => Debug.Log(a.State + "(" + a.name + ") beats " + b.State + "(" + b.name + ")");
        Draw += (a,b) => Debug.Log(a.State + "(" + a.name + ") draws with " + b.State + "(" + b.name + ")");
    }

    void Start() {
        nameText = GetComponentInChildren<TextMeshProUGUI>();
        nameText.enabled = false;
        nameText.text = this.name;

        GetComponentInChildren<DieTranslator>().ReachTarget += () => MoveTile?.Invoke();
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
        ghostMaterial = Globals.GHOST_MATERIALS[(_unit.UnitClass, IsEnemy)];

        if (Application.isPlaying)
        {
            GetComponentInChildren<MeshRenderer>().sharedMaterial = Globals.DICE_MATERIALS[(_unit.UnitClass, IsEnemy)];
        }
        else
        {
            Material editorMat = new(_unit.Data.diceMaterial);
            if (IsEnemy)
                editorMat.color = _unit.Data.enemyColor;
            else
                editorMat.color = _unit.Data.allyColor;

            GetComponentInChildren<MeshRenderer>().sharedMaterial = editorMat;
        }

        if (!_unit.LoadFromSave)
            ResetRange();
        SetOrientation(orientation);

        if (!Application.isPlaying) return;

        if (IsEnemy)
        {
            GameManager.Instance.EnemyCount++;
            _enemyAI.enabled = true;
        }
        else
        {
            GameManager.Instance.AddPlayer(this);
            _enemyAI.enabled = false;
            Destroy(_enemyAI);
        }
    }

    // consumes each step of the enumerator only after the last move has completed
    private async UniTask MoveMany(IEnumerator<OverlayTile> tiles, CancellationToken token) {
        if (tiles.MoveNext())
        {
            OverlayTile tile;
            do
            {
                tile = tiles.Current;
                if (tile.IsBlocked || _movesAvailable <= 0) break;
                GetTilesInRange();

                if (!_tilesInRange.Contains(tile))
                    return;
                await UpdateTilePos(tile, token);
            } while (tiles.MoveNext());
        }
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

        SelectUnit?.Invoke(this);
    }

    public void Deselect()
    {
        HideTilesInRange();
    }

    public void AddPath(OverlayTile tile)
    {
        var delta = tile.gridLocation - parentTile.gridLocation;

        // filter moves
        switch (this.MovementPattern)
        {
            case MovementPattern.Single:
                if (Math.Abs(delta.x) + Math.Abs(delta.y) != 1)
                    return;
                else
                    break;
            case MovementPattern.Straight:
                if (delta.x != 0 && delta.y != 0)
                    return;
                else
                    break;
            case MovementPattern.Knight:
                if (Math.Abs(delta.x * delta.y) != 2)
                    return;
                else
                    break;
            default:
                return;
        }
        path.Add((Vector2Int)delta);
    }

    public void MapPath()
    {
        Vector2Int start = (Vector2Int)parentTile.gridLocation;
        Vector2Int pos = start;
        List<IEnumerable<Vector2Int>> deltas = new();
        List<Vector3> trans = new();

        GhostManager.Instance.RemoveArrow(this.gameObject);
        GhostManager.Instance.PushArrow(this.gameObject, pos);

        for (int i = 0; i < MaxMoves; i++)
        {
            Vector2Int next = path[i] + pos;
            deltas.Add(StepRotations(path[i]));
            trans.Add(
                MapManager.Instance.TileToWorldSpace(new Vector2Int(0, 0))
                - MapManager.Instance.TileToWorldSpace(pos - next)
            );

            GhostManager.Instance.SetupGhostEffects(this.gameObject, next, trans, deltas);
            pos = next;
        }
    }

    public async UniTask Fight()
    {
        List<UnitManager> toKill = new();

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
        Debug.Log("Killing " + UnitName);
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

    public List<OverlayTile> GetTilesInRange(Vector2Int position)
    {
        if (_movesAvailable <= 0) return new();

        return MovementPattern switch
        {
            MovementPattern.Straight => MapManager.Instance.GetTilesStraightLine(position),
            MovementPattern.Single => MapManager.Instance.GetSurroundingTiles(position),
            MovementPattern.Knight => MapManager.Instance.GetTilesKnight(position),
            _ => new(),
        };
    }

    private void GetTilesInRange()
    {
        _tilesInRange = GetTilesInRange(Position);
    }

    private List<OverlayTile> GetTilesAdjacent()
    {
        return MapManager.Instance.GetSurroundingTiles(Position);
    }

    public void ResetRange()
    {
        _movesAvailable = MaxMoves;
    }

    public void SetOrientation(DiceOrientation orientation)
    {
        _dieRotator.SetOrientation(orientation);
        State = _dieRotator.GetUpFace();
    }

    public void SetOrientation(Vector3 orientation)
    {
        _dieRotator.SetRotation(Quaternion.Euler(orientation.x, orientation.y, orientation.z));
        State = _dieRotator.GetUpFace();
    }

    private void MoveToPos(Vector2Int step, bool rotate = true)
    {
        if (rotate)
            _dieRotator.RotateTileDeltas(StepRotations(step));

        GetComponentInChildren<DieTranslator>().Translate(
            MapManager.Instance.TileDeltaToWorldDelta(step)
        );
    }

    private async UniTask UpdateTilePos(OverlayTile newTile, CancellationToken token, bool rotate = true)
    {
        _toFight = true;
        _toTileEffect = true;

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
                int a = UnityEngine.Random.Range(0, _dieRotator.axes.FaceEdges);
                int b = UnityEngine.Random.Range(0, _dieRotator.axes.FaceEdges);
                int c = UnityEngine.Random.Range(0, _dieRotator.axes.FaceEdges);

                _dieRotator.RotateZ(a);
                _dieRotator.RotateZ(b);
                _dieRotator.RotateZ(c);
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
        switch (phase)
        {
            case Phase.Enemy:
                if (IsEnemy)
                    return PhaseStepResult.Unchanged;
                else
                    return PhaseStepResult.Done;

            case Phase.Player:
                if (!IsEnemy && _unit.LoadFromSave)
                {
                    _unit.LoadFromSave = false;
                    return PhaseStepResult.Passive;
                }
                ResetRange();
                if (IsEnemy)
                    return PhaseStepResult.Done;
                else
                    return PhaseStepResult.Passive;
            case Phase.TileEffects:
            case Phase.Fight:
                return PhaseStepResult.Unchanged;
            default:
                return PhaseStepResult.Done;
        }
    }

    public void OnPhaseResume(Phase phase) {
        switch (phase) {
            case Phase.Player:
                if (Globals.SELECTED_UNIT == this && path.Count == 0) {
                    GetTilesInRange();
                    ShowTilesInRange();
                }
                break;
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
                if (_toFight) {
                    _toFight = false;
                    await Fight();
                }
                return PhaseStepResult.Done;
            case Phase.TileEffects:
                if (_toTileEffect) {
                    _toTileEffect = false;

                    if (await GetTileEffects(token)) {
                        return PhaseStepResult.Changed;
                    } else {
                        return PhaseStepResult.Done;
                    }
                }
                return PhaseStepResult.Done;
            default:
                return PhaseStepResult.Done;
        }
    }

    void OnDebugNames() {
        nameText.enabled = !nameText.enabled;
    }

    void OnDestroy()
    {
        Logging.LogNotification(("Destroying " + _unitName).ToString(), LogType.GAME_SETUP);

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
            GameManager.Instance.RemovePlayer(this);
        }
    }

    public async UniTask StepPath(CancellationToken token)
    {
        Logging.LogNotification(("Following Path: " + PathStr()).ToString(), LogType.AI);

        if (path.Count > 0) {
            OverlayTile tile;
            try {
                var step = PathSteps(path[0]).First();
                if (_movementPattern == MovementPattern.Knight)
                    tile = MapManager.Instance.GetTileAtPos(
                        (Vector2Int)parentTile.gridLocation + path[0]
                    );
                else
                    tile = MapManager.Instance.GetTileAtPos(
                            (Vector2Int)parentTile.gridLocation + step
                        );


                path[0] -= step;

                await MoveAsync(tile, token);

                if (path[0] == Vector2Int.zero)
                {
                    path.RemoveAt(0);

                    _movesAvailable--;

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
            } catch (KeyNotFoundException) {
                Debug.Log("Tile does not exist, stopping path");
                path.Clear();

                return;
            }
        }

        Logging.LogNotification(("Ending StepPath: " + transform.name).ToString(), LogType.AI);
    }

    // break into intermediate steps that are occupied along the movement
    public IEnumerable<Vector2Int> PathSteps(Vector2Int delta) {
        switch (MovementPattern) {
            case MovementPattern.Knight:
            case MovementPattern.Single:
                yield return delta;
                break;
            case MovementPattern.Straight:
                while (delta != Vector2Int.zero) {
                    var step = new Vector2Int(Math.Sign(delta.x), Math.Sign(delta.y));
                    yield return step;
                    delta -= step;
                }
                break;
        }
    }

    // break step into intermediate rotations
    public IEnumerable<Vector2Int> StepRotations(Vector2Int step) {
        Vector2Int delta;

        if (Math.Abs(step.x) > Math.Abs(step.y)) {
            delta = new(Math.Sign(step.x), 0);
            while (step.x != 0) {
                step -= delta;
                yield return delta;
            }

            delta = new(0, Math.Sign(step.y));
            while (step.y != 0) {
                step -= delta;
                yield return delta;
            }
        } else if (Math.Abs(step.x) < Math.Abs(step.y)) {
            delta = new(0, Math.Sign(step.y));
            while (step.y != 0) {
                step -= delta;
                yield return delta;
            }

            delta = new(Math.Sign(step.x), 0);
            while (step.x != 0) {
                step -= delta;
                yield return delta;
            }
        }
    }

    public string PathStr() {
        return (Vector2Int)parentTile.gridLocation + " -> " + Utilities.EnumerableString(path);
    }
}
