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

public class DieManager : MonoBehaviour, PhaseListener
{
    public MonoBehaviour Self { get { return this; } }

    //Properties
    public string diceName;
    [SerializeField]
    private int _maxRange;
    private int _movesAvailable;
    public int MovesAvailable
    {
        get { return _movesAvailable; }
    }

    public bool movesInStraightLine;
    public bool isEnemy;
    public DiceState state;
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
    private EnemyAI enemyAI;

    //Materials
    [HideInInspector]
    public Material ghostMaterial;
    public Material alliedMaterial;
    public Material enemyMaterial;
    public Material alliedGhostMaterial;
    public Material enemyGhostMaterial;

    public GameObject ghostComponents;
    private DieRotator _dieRotator;

    private TextMeshProUGUI nameText;

    public event Action<OverlayTile> MoveFinished;
    public static event Action<DieManager, DieManager> ABeatsB;
    public static event Action<DieManager, DieManager> Draw;

    static DieManager() {
        ABeatsB += (a,b) => Debug.Log(a.state + "(" + a.name + ") beats " + b.state + "(" + b.name + ")");
        Draw += (a,b) => Debug.Log(a.state + "(" + a.name + ") draws with " + b.state + "(" + b.name + ")");
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
        if (!isEnemy)
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

    public void Initialize(bool enemy, DiceOrientation orientation)
    {
        isEnemy = enemy;

        if (isEnemy)
        {
            GameManager.Instance.EnemyCount++;
            enemyAI.enabled = true;
            ghostMaterial = enemyGhostMaterial;
            GetComponentInChildren<MeshRenderer>().sharedMaterial = enemyMaterial;
        }
        else
        {
            GameManager.Instance.PlayerCount++;
            enemyAI.enabled = false;
            Destroy(enemyAI);
            ghostMaterial = alliedGhostMaterial;
            GetComponentInChildren<MeshRenderer>().sharedMaterial = alliedMaterial; ;
        }

        ResetRange();
        _dieRotator = GetComponentInChildren<DieRotator>();
        _dieRotator.RotateTileDelta(Vector2Int.right, orientation.xRolls);
        _dieRotator.RotateTileDelta(Vector2Int.up, orientation.yRolls);
        _dieRotator.RotateZ(orientation.zRolls);
        _dieRotator.RotateNow();
        state = _dieRotator.GetUpFace();
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
            if (!isEnemy)
            {
                GameManager.Instance.PlayerSteps++;

                if (!GameManager.Instance.MovedPieces.Contains(this))
                    {
                    GameManager.Instance.MovedPieces.Add(this);
                    GameManager.Instance.PlayerPiecesMoved += 1;
                }

                if (_movesAvailable <= 0)
                    GameManager.Instance.PlayerMoveRemaining--;
            }

            MoveFinished?.Invoke(tile);
            // hack to fix bug
            if (_movesAvailable <= 0) {
                HideTilesInRange();
            }
        } else {
            // important if no path exists to still pass event
            MoveFinished?.Invoke(null);
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
        if (!isEnemy)
            GhostManager.Instance.SetEnemyGhostsVisible(true);

        HideTilesInRange();
    }

    public IEnumerator<OverlayTile> PathGenerator(OverlayTile tile)
    {
        if (parentTile.gridLocation.x < tile.gridLocation.x)
        {
            for (int x = parentTile.gridLocation.x + 1; x <= tile.gridLocation.x; ++x) {
                yield return MapManager.Instance.GetTileAtPos(new Vector2Int(x, parentTile.gridLocation.y));
            }
        }
        else if (parentTile.gridLocation.x > tile.gridLocation.x)
        {
            for (int x = parentTile.gridLocation.x - 1; x >= tile.gridLocation.x; --x) {
                yield return MapManager.Instance.GetTileAtPos(new Vector2Int(x, parentTile.gridLocation.y));
            }
        }
        else if (parentTile.gridLocation.y < tile.gridLocation.y)
        {
            for (int y = parentTile.gridLocation.y + 1; y <= tile.gridLocation.y; ++y) {
                yield return MapManager.Instance.GetTileAtPos(new Vector2Int(parentTile.gridLocation.x, y));
            }
        }
        else if (parentTile.gridLocation.y > tile.gridLocation.y)
        {
            for (int y = parentTile.gridLocation.y - 1; y >= tile.gridLocation.y; --y) {
                yield return MapManager.Instance.GetTileAtPos(new Vector2Int(parentTile.gridLocation.x, y));
            }
        }
    }

    public async UniTask Fight()
    {
        List<DieManager> toKill = new List<DieManager>();

        foreach (OverlayTile tile in GetTilesAdjacent())
        {
            if (tile.occupyingDie != null && isEnemy != tile.occupyingDie.isEnemy)
            {
                DieManager enemyDie = tile.occupyingDie;
                DiceState enemyState = enemyDie.state;

                switch (state) {
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

                                if (!this.isEnemy) {
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

        foreach (DieManager die in toKill)
        {
            if (!die.isEnemy && die.state == DiceState.King) {
                GameManager.Instance.PlayerKingDefeated = true;
            }
            die.Kill();
        }
        if (!toKill.Contains(this))
            Select();
    }

    public void Kill()
    {
        Deselect();
        parentTile.RemoveDiceFromTile();
        if (isEnemy)
            Destroy(gameObject.GetComponent<EnemyAI>());
        Destroy(gameObject);
    }

    public void ShowTilesInRange()
    {
        if (isEnemy) return;

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
        if (isEnemy) return;

        GhostManager.Instance.RemoveGhosts(gameObject);
        foreach (OverlayTile tile in _tilesInRange)
            tile.HideTile();
    }

    private void GetTilesInRange()
    {
        _tilesInRange.Clear();
        if (_movesAvailable <= 0) return;

        if (movesInStraightLine)
            _tilesInRange = GetTilesStraightLine();
        else if (!movesInStraightLine)
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
        _movesAvailable = _maxRange;
    }

    public int MaxRange() {
        return _maxRange;
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
        state = _dieRotator.GetUpFace();
        HideTilesInRange();
        parentTile.RemoveDiceFromTile();
        newTile.MoveDiceToTile(this);

        await UniTask.Delay(TimeSpan.FromSeconds(Globals.MOVEMENT_TIME + 0.1f), cancellationToken: token);
    }

    private async UniTask<bool> GetTileEffects(CancellationToken token)
    {
        TileType tileType = parentTile.data.TileType;
        HideTilesInRange();
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
                if (isEnemy) {
                    ResetRange();
                }
                return PhaseStepResult.Done;
            case Phase.Player:
                if (!isEnemy) {
                    ResetRange();
                }
                return PhaseStepResult.Done;
            case Phase.TileEffects:
            case Phase.Fight:
                return PhaseStepResult.Blocking;
            default:
                return PhaseStepResult.Done;
        }

    }

    public async UniTask<PhaseStepResult> OnPhaseUpdate(Phase phase, CancellationToken token) {
        switch (phase) {
            case Phase.Fight:
                await Fight();
                return PhaseStepResult.Done;
            case Phase.TileEffects:
                if (await GetTileEffects(token)) {
                    return PhaseStepResult.Blocking;
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

        if (isEnemy)
        {
            GameManager.Instance.EnemyCount--;
        }
        else
        {
            GameManager.Instance.PlayerMoveRemaining--;
            GameManager.Instance.PlayerCount--;
        }
    }
}
