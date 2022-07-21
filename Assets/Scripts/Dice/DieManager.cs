using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DiceState : uint
{
    Rock = 0,
    Paper = 1,
    Scissors = 2,
    Lich = 3,
    Blank = 4,
    King = 5,
}

public class DieManager : MonoBehaviour
{
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

    private Action<Turn> turnChange;

    public event Action<OverlayTile> MoveFinished;


    void Start() {
        turnChange = t => TurnChange(t);
        GameManager.Instance.TurnChange += turnChange;

        GetComponentInChildren<DieTranslator>().ReachTarget += () => EventManager.TriggerEvent("Move");
    }

    private void Update()
    {
        if (!isEnemy)
        {
            if (GameManager.Instance.PlayerPiecesMoved < GameManager.Instance.MaxPlayerMoves || GameManager.Instance.MovedPieces.Contains(this))
            {
                if (GameManager.Instance.CurrentTurnValue == Turn.Player && _movesAvailable > 0)
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
    }

    private IEnumerator MoveMany(List<OverlayTile> tiles) {
        Debug.Log("Garfield");
        if (tiles.Count > 0) {
            foreach (var tile in tiles.TakeWhile(t => (!t.IsBlocked && _movesAvailable > 0))) {
                GetTilesInRange();
                if (!_tilesInRange.Contains(tile)) {
                    yield break;
                }
                CalculateDirection(tile);
                state = _dieRotator.GetUpFace();
                yield return StartCoroutine(UpdateTilePos(tile));
            }
            _movesAvailable--;

            EventManager.TriggerEvent("SelectUnit");
            if (!isEnemy)
            {
                if (!GameManager.Instance.MovedPieces.Contains(this))
                    {
                    GameManager.Instance.MovedPieces.Add(this);
                    GameManager.Instance.PlayerPiecesMoved += 1;
                }

                if (_movesAvailable <= 0)
                    GameManager.Instance.PieceOutOfMoves();
            }

            MoveFinished?.Invoke(tiles[tiles.Count - 1]);
            // hack to fix bug
            if (_movesAvailable <= 0) {
                HideTilesInRange();
            }
        } else {
            // important if no path exists to still pass event
            MoveFinished?.Invoke(null);
        }
    }

    public void Move(OverlayTile newTile) {
        StartCoroutine(MoveMany(new List<OverlayTile> { newTile }));
    }

    public void Move(List<OverlayTile> tiles) {
        StartCoroutine(MoveMany(tiles));
    }

    public void Select()
    {
        if (Globals.SELECTED_UNIT != null)
            Globals.SELECTED_UNIT.Deselect();
        Globals.SELECTED_UNIT = this;

        GhostManager.Instance.SetEnemyGhostsVisible(Input.GetKey("space"));
        GhostManager.Instance.SetGhostsVisible(this.gameObject, true);

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

    public List<OverlayTile> FollowPath(OverlayTile tile)
    {
        List<OverlayTile> list = new List<OverlayTile>();

        if (parentTile.gridLocation.x < tile.gridLocation.x)
        {
            int x = parentTile.gridLocation.x + 1;
            while (x <= tile.gridLocation.x)
            {
                OverlayTile tileToAdd = MapManager.Instance.GetTileAtPos(new Vector2Int(x, parentTile.gridLocation.y));
                list.Add(tileToAdd);
                x++;
            }
        }
        else if (parentTile.gridLocation.x > tile.gridLocation.x)
        {
            int x = parentTile.gridLocation.x - 1;
            while (x >= tile.gridLocation.x)
            {
                OverlayTile tileToAdd = MapManager.Instance.GetTileAtPos(new Vector2Int(x, parentTile.gridLocation.y));
                list.Add(tileToAdd);
                x--;
            }
        }
        else if (parentTile.gridLocation.y < tile.gridLocation.y)
        {
            int y = parentTile.gridLocation.y + 1;
            while (y <= tile.gridLocation.y)
            {
                OverlayTile tileToAdd = MapManager.Instance.GetTileAtPos(new Vector2Int(parentTile.gridLocation.x, y));
                list.Add(tileToAdd);
                y++;
            }
        }
        else if (parentTile.gridLocation.y > tile.gridLocation.y)
        {
            int y = parentTile.gridLocation.y - 1;
            while (y >= tile.gridLocation.y)
            {
                OverlayTile tileToAdd = MapManager.Instance.GetTileAtPos(new Vector2Int(parentTile.gridLocation.x, y));
                list.Add(tileToAdd);
                y--;
            }
        }

        return list;
    }

    public void Fight()
    {
        List<DieManager> toKill = new List<DieManager>();

        foreach (OverlayTile tile in GetTilesAdjacent())
        {
            if (tile.occupyingDie != null && isEnemy != tile.occupyingDie.isEnemy)
            {
                DieManager enemyDie = tile.occupyingDie;
                DiceState enemyState = enemyDie.state;

                String eventName = null;

                switch (state)
                {
                    case DiceState.King:
                        switch (enemyState) {
                            case DiceState.Blank:
                                eventName = "Ally" + state + "Beats" + enemyState;

                                toKill.Add(enemyDie);
                                Debug.Log(state + "(" + this.name + ") beats " + enemyState + "(" + enemyDie.name + ")");
                                break;
                            case DiceState.Rock:
                            case DiceState.Scissors:
                            case DiceState.Paper:
                            case DiceState.Lich:
                                eventName = "Ally" + state + "BeatenBy" + enemyState;

                                if (!this.isEnemy) {
                                    GameManager.Instance.PlayerKingDefeated = true;
                                }

                                toKill.Add(this);
                                Debug.Log(state + "(" + this.name + ") beaten by " + enemyState + "(" + enemyDie.name + ")");

                                break;
                            case DiceState.King:
                                eventName = "Draw";
                                Debug.Log(state + "(" + this.name + ") draws with " + enemyState + "(" + enemyDie.name + ")");
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
                                eventName = "Ally" + state + "Beats" + enemyState;

                                toKill.Add(enemyDie);
                                Debug.Log(state + "(" + this.name + ") beats " + enemyState + "(" + enemyDie.name + ")");
                                break;
                            case DiceState.Lich:
                                eventName = "Draw";
                                Debug.Log(state + "(" + this.name + ") draws with " + enemyState + "(" + enemyDie.name + ")");
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
                                eventName = "Ally" + state + "BeatenBy" + enemyState;

                                toKill.Add(this);
                                Debug.Log(state + "(" + this.name + ") beaten by " + enemyState + "(" + enemyDie.name + ")");
                                break;
                            case DiceState.Blank:
                                eventName = "Draw";
                                Debug.Log(state + "(" + this.name + ") draws with " + enemyState + "(" + enemyDie.name + ")");
                                break;
                        }
                        break;
                    case DiceState.Rock:
                        switch (enemyState) {
                            case DiceState.King:
                            case DiceState.Scissors:
                            case DiceState.Blank:
                                eventName = "Ally" + state + "Beats" + enemyState;

                                toKill.Add(enemyDie);
                                Debug.Log(state + "(" + this.name + ") beats " + enemyState + "(" + enemyDie.name + ")");
                                break;
                            case DiceState.Paper:
                            case DiceState.Lich:
                                eventName = "Ally" + state + "BeatenBy" + enemyState;

                                toKill.Add(this);
                                Debug.Log(state + "(" + this.name + ") beaten by " + enemyState + "(" + enemyDie.name + ")");
                                break;
                            case DiceState.Rock:
                                eventName = "Draw";
                                Debug.Log(state + "(" + this.name + ") draws with " + enemyState + "(" + enemyDie.name + ")");
                                break;
                        }
                        break;
                    case DiceState.Paper:
                        switch (enemyState) {
                            case DiceState.King:
                            case DiceState.Rock:
                            case DiceState.Blank:
                                eventName = "Ally" + state + "Beats" + enemyState;

                                toKill.Add(enemyDie);
                                Debug.Log(state + "(" + this.name + ") beats " + enemyState + "(" + enemyDie.name + ")");
                                break;
                            case DiceState.Scissors:
                            case DiceState.Lich:
                                eventName = "Ally" + state + "BeatenBy" + enemyState;

                                toKill.Add(this);
                                Debug.Log(state + "(" + this.name + ") beaten by " + enemyState + "(" + enemyDie.name + ")");
                                break;
                            case DiceState.Paper:
                                eventName = "Draw";

                                Debug.Log(state + "(" + this.name + ") draws with " + enemyState + "(" + enemyDie.name + ")");
                                break;
                        }
                        break;
                    case DiceState.Scissors:
                        switch (enemyState) {
                            case DiceState.King:
                            case DiceState.Paper:
                            case DiceState.Blank:
                                eventName = "Ally" + state + "Beats" + enemyState;

                                toKill.Add(enemyDie);
                                Debug.Log(state + "(" + this.name + ") beats " + enemyState + "(" + enemyDie.name + ")");
                                break;
                            case DiceState.Rock:
                            case DiceState.Lich:
                                eventName = "Ally" + state + "BeatenBy" + enemyState;

                                toKill.Add(this);
                                Debug.Log(state + "(" + this.name + ") beaten by " + enemyState + "(" + enemyDie.name + ")");
                                break;
                            case DiceState.Scissors:
                                eventName = "Draw";
                                Debug.Log(state + "(" + this.name + ") draws with " + enemyState + "(" + enemyDie.name + ")");
                                break;
                        }
                        break;
                }

                EventManager.TriggerEvent(eventName);
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
            Vector3Int rot = parentTile.gridLocation - tile.gridLocation;
            Vector3 translation
                = MapManager.Instance.TileToWorldSpace(tile.gridLocation)
                - MapManager.Instance.TileToWorldSpace(parentTile.gridLocation);
            var ghost = GhostManager.Instance.CreateGhost(gameObject, translation, (Vector2Int)rot);
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

    public void CalculateDirection(OverlayTile newTile)
    {
        MoveToPos((Vector2Int)parentTile.gridLocation, (Vector2Int)newTile.gridLocation);
        Vector3Int dir = parentTile.gridLocation - newTile.gridLocation;

        GetComponentInChildren<DieRotator>().RotateTileDelta((Vector2Int)dir);

        Debug.Log(GetComponentInChildren<DieRotator>().GetUpFace());
    }

    private void MoveToPos(Vector2Int startPos, Vector2Int endPos)
    {
        GetComponentInChildren<DieTranslator>().Translate(
            MapManager.Instance.TileToWorldSpace(endPos) - MapManager.Instance.TileToWorldSpace(startPos)
        );
    }

    private IEnumerator UpdateTilePos(OverlayTile newTile)
    {
        HideTilesInRange();
        parentTile.RemoveDiceFromTile();
        newTile.MoveDiceToTile(this);

        yield return StartCoroutine(GetTileEffects());        

    }

    private IEnumerator GetTileEffects()
    {
        TileType tileType = parentTile.data.TileType;
        HideTilesInRange();
        switch (tileType)
        {
            case TileType.Normal:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                GetTilesInRange();
                Fight();
                break;
            case TileType.Stopping:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                _movesAvailable = 0;
                GetTilesInRange();
                Fight();
                break;
            case TileType.RotateClockwise:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                _dieRotator.RotateZ(1);
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME);
                GetTilesInRange();
                Fight();
                break;
            case TileType.RotateCounterClockwise:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                _dieRotator.RotateZ(-1);
                GetTilesInRange();
                Fight();
                break;
            case TileType.ShovePosX:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                Shove(new Vector2Int(1, 0));
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME);
                GetTilesInRange();
                Fight();
                break;
            case TileType.ShovePosY:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                Shove(new Vector2Int(0, 1));
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME);
                GetTilesInRange();
                Fight();
                break;
            case TileType.ShoveNegX:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                Shove(new Vector2Int(-1, 0));
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME);
                GetTilesInRange();
                Fight();
                break;
            case TileType.ShoveNegY:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                Shove(new Vector2Int(0, -1));
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME);
                GetTilesInRange();
                Fight();
                break;
            case TileType.RemoveFace:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                _dieRotator.SetDownFace(DiceState.Blank);
                GetTilesInRange();
                Fight();
                break;
            case TileType.Randomize:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                _dieRotator.RotateZ(UnityEngine.Random.Range(0, _dieRotator.axes.FaceEdges));
                _dieRotator.RotateZ(UnityEngine.Random.Range(0, _dieRotator.axes.FaceEdges));
                _dieRotator.RotateZ(UnityEngine.Random.Range(0, _dieRotator.axes.FaceEdges));
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME);
                GetTilesInRange();
                Fight();
                break;
            default:
                yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);
                GetTilesInRange();
                Fight();
                break;
        }
    }

    public void Shove(Vector2Int dir)
    {
        Vector2Int newPos = (Vector2Int)parentTile.gridLocation + dir;
        MoveToPos((Vector2Int)parentTile.gridLocation, newPos);
        StartCoroutine(UpdateTilePos(MapManager.Instance.GetTileAtPos(newPos)));
    }

    private void TurnChange(Turn turn) {
        if (isEnemy && turn == Turn.Enemy)
            ResetRange();
        else if (!isEnemy && turn == Turn.Player)
            ResetRange();

        Debug.Log("Turn change " + this + ":"+ _movesAvailable + "/" + _maxRange);
    }

    void OnDestroy() {
        GhostManager.Instance.RemoveGhosts(gameObject);

        if (Globals.SELECTED_UNIT == this) {
            Globals.SELECTED_UNIT = null;
        }

        if (turnChange != null)
            GameManager.Instance.TurnChange -= turnChange;

        if (isEnemy)
        {
            GameManager.Instance.EnemyCount--;
        }
        else
        {
            GameManager.Instance.PieceOutOfMoves();
            GameManager.Instance.PlayerCount--;
        }
    }
}
