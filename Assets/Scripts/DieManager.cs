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
    Nuclear = 3,
    Blank = 4,
}

public class DieManager : MonoBehaviour
{
    //Properties
    public string diceName;
    [SerializeField]
    private int _maxRange;
    public int movesAvailable;
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
        turnChange = t => this.TurnChange(t);
        GameManager.Instance.TurnChange += turnChange;
    }

    private void Update()
    {
        if (!isEnemy)
        {
            if (movesAvailable > 0)
            {
                _moveIndicator.SetActive(true);
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
        _dieRotator.RotateX(orientation.xRolls);
        _dieRotator.RotateY(orientation.yRolls);
        _dieRotator.RotateZ(orientation.zRolls);
        _dieRotator.RotateNow();
        state = _dieRotator.UpFace();
    }

    private IEnumerator MoveMany(List<OverlayTile> tiles) {
        if (tiles.Count > 0) {
            foreach (var tile in tiles) {
                if (!movesInStraightLine)
                    GetTilesInRange();
                if (!_tilesInRange.Contains(tile)) {
                    yield break;
                }
                CalculateDirection(tile);
                state = _dieRotator.UpFace();
                yield return StartCoroutine(UpdateTilePos(tile));
            }

            if (!isEnemy)
            {
                GameManager.Instance.PlayerMoveRemaining--;
            }
            movesAvailable--;

            MoveFinished?.Invoke(tiles[tiles.Count - 1]);
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
        if (!isEnemy) {
            GhostManager.Instance.SetEnemyGhostsVisible(true);
        }

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

                switch (state)
                {
                    case DiceState.Rock:
                        if (enemyState == DiceState.Scissors)
                        {
                            toKill.Add(enemyDie);
                            EventManager.TriggerEvent("AllyRockBeatsScissors");
                            Debug.Log(state + "(" + this.name + ") beats " + enemyState + "(" + enemyDie.name + ")");
                        }
                        else if (enemyState == DiceState.Paper)
                        {
                            toKill.Add(this);
                            EventManager.TriggerEvent("AllyRockBeatenByPaper");
                            Debug.Log(state + "(" + this.name + ") beaten by " + enemyState + "(" + enemyDie.name + ")");
                        }
                        else if (enemyState == DiceState.Rock)
                        {
                            EventManager.TriggerEvent("Draw");
                            Debug.Log(state + "(" + this.name + ") draws with " + enemyState + "(" + enemyDie.name + ")");
                        }
                        break;
                    case DiceState.Paper:
                        if (enemyState == DiceState.Rock)
                        {
                            toKill.Add(enemyDie);
                            EventManager.TriggerEvent("AllyPaperBeatsRock");
                            Debug.Log(state + "(" + this.name + ") beats " + enemyState + "(" + enemyDie.name + ")");
                        }
                        else if (enemyState == DiceState.Scissors)
                        {
                            toKill.Add(this);
                            EventManager.TriggerEvent("AllyPaperBeatenByScissors");
                            Debug.Log(state + "(" + this.name + ") beaten by " + enemyState + "(" + enemyDie.name + ")");
                        }
                        else if (enemyState == DiceState.Paper)
                        {
                            EventManager.TriggerEvent("Draw");
                            Debug.Log(state + "(" + this.name + ") draws with " + enemyState + "(" + enemyDie.name + ")");
                        }
                        break;
                    case DiceState.Scissors:
                        if (enemyState == DiceState.Paper)
                        {
                            toKill.Add(enemyDie);
                            EventManager.TriggerEvent("AllyScissorsBeatsPaper");
                            Debug.Log(state + "(" + this.name + ") beats " + enemyState + "(" + enemyDie.name + ")");
                        }
                        else if (enemyState == DiceState.Rock)
                        {
                            toKill.Add(this);
                            EventManager.TriggerEvent("AllyScissorsBeatenByRock");
                            Debug.Log(state + "(" + this.name + ") beaten by " + enemyState + "(" + enemyDie.name + ")");
                        }
                        else if (enemyState == DiceState.Scissors)
                        {
                            EventManager.TriggerEvent("Draw");
                            Debug.Log(state + "(" + this.name + ") draws with " + enemyState + "(" + enemyDie.name + ")");
                        }
                        break;
                }
            }
        }

        foreach (DieManager die in toKill)
        {
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
            GhostManager.Instance.CreateGhost(gameObject, new Vector2Int(tile.gridLocation.x, tile.gridLocation.y), rot.x, rot.y);
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
        if (movesAvailable <= 0 && !movesInStraightLine) return;

        if (movesInStraightLine)
        {
            _tilesInRange = GetTilesStraightLine();
        }
        else if (!movesInStraightLine)
        {
            _tilesInRange = GetTilesAdjacent();
        }
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
        if (!isEnemy) {
            GameManager.Instance.PlayerMoveRemaining += _maxRange - movesAvailable;
        }

        movesAvailable = _maxRange;
    }

    public int MaxRange() {
        return _maxRange;
    }

    public void CalculateDirection(OverlayTile newTile)
    {
        StartCoroutine(MoveToPos(parentTile.transform.position, newTile.transform.position));
        Vector3Int dir = parentTile.gridLocation - newTile.gridLocation;

        if (dir == new Vector3Int(1, 0))
        {
            GetComponentInChildren<DieRotator>().RotateX(1);
        }
        else if (dir == new Vector3Int(-1, 0))
        {
            GetComponentInChildren<DieRotator>().RotateX(-1);
        }
        else if (dir == new Vector3Int(0, 1))
        {
            GetComponentInChildren<DieRotator>().RotateY(1);
        }
        else if (dir == new Vector3Int(0, -1))
        {
            GetComponentInChildren<DieRotator>().RotateY(-1);
        }

        Debug.Log(GetComponentInChildren<DieRotator>().UpFace());
    }

    private IEnumerator MoveToPos(Vector2 startPos, Vector2 endPos)
    {
        float elapsedTime = 0;

        while (elapsedTime < Globals.MOVEMENT_TIME)
        {
            Vector3 start = MapManager.Instance.GetWorldSpace(startPos);
            Vector3 end = MapManager.Instance.GetWorldSpace(endPos);
            transform.position = Vector3.Lerp(start, end, (elapsedTime / Globals.MOVEMENT_TIME));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        EventManager.TriggerEvent("Move");
    }

    private IEnumerator UpdateTilePos(OverlayTile newTile)
    {
        HideTilesInRange();
        yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);

        parentTile.RemoveDiceFromTile();
        newTile.MoveDiceToTile(this);

        GetTilesInRange();
        Fight();
    }

    void TurnChange(Turn turn) {
        if (isEnemy && turn == Turn.Enemy) {
            ResetRange();
        } else if (!isEnemy && turn == Turn.Player) {
            ResetRange();
        }
        Debug.Log("Turn change " + this + ":"+ movesAvailable + "/" + _maxRange);
    }

    void OnDestroy() {
        GhostManager.Instance.RemoveGhosts(gameObject);

        if (turnChange != null) {
            GameManager.Instance.TurnChange -= turnChange;
        }

        if (isEnemy) {
            GameManager.Instance.EnemyCount--;
        } else {
            GameManager.Instance.PlayerCount--;
            GameManager.Instance.PlayerMoveRemaining -= movesAvailable;
        }
    }
}
