using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DiceState : uint
{
    Rock = 0,
    Paper = 1,
    Scissors = 2
}

public class DieManager : MonoBehaviour
{
    private List<OverlayTile> _tilesInRange = new List<OverlayTile>();
    public OverlayTile parentTile;
    [SerializeField]
    private MeshRenderer _meshRenderer;

    [SerializeField]
    private int _maxRange;
    private int _currentRange;
    public bool isEnemy;
    public DiceState state;
    public Material ghostMaterial;
    public GameObject ghostComponents;
    private DieRotator _dieRotator;

    private Action<Turn> turnChange;

    public event Action<OverlayTile> MoveFinished;

    void Start() {
        turnChange = t => this.TurnChange(t);
        GameManager.Instance.TurnChange += turnChange;
    }

    public void Initialize(bool enemy, DiceOrientation orientation)
    {
        isEnemy = enemy;
        ResetRange();
        _dieRotator = GetComponentInChildren<DieRotator>();
        state = _dieRotator.UpFace();
        _dieRotator.RotateX(orientation.xRolls);
        _dieRotator.RotateY(orientation.yRolls);
        _dieRotator.RotateZ(orientation.zRolls);
    }

    private IEnumerator MoveMany(List<OverlayTile> tiles) {
        foreach (var tile in tiles) {
            GetTilesInRange();
            if (!_tilesInRange.Contains(tile)) {
                yield break;
            }
            CalculateDirection(tile);
            state = _dieRotator.UpFace();
            Debug.Log(tile);
            yield return StartCoroutine(UpdateTilePos(tile));
        }

        MoveFinished?.Invoke(tiles[tiles.Count - 1]);
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
        GetTilesInRange();
        ShowTilesInRange();
    }

    public void Deselect()
    {
        HideTilesInRange();
    }

    public void Fight()
    {
        List<DieManager> toKill = new List<DieManager>();

        foreach (OverlayTile tile in _tilesInRange)
        {
            if (tile.occupyingDie != null && tile.occupyingDie.isEnemy)
            {
                DieManager enemyDie = tile.occupyingDie;
                DiceState enemyState = enemyDie.state;
                switch (state)
                {
                    case DiceState.Rock:
                        if (enemyState == DiceState.Scissors)
                        {
                            toKill.Add(enemyDie);
                            Debug.Log(state);
                            Debug.Log(enemyState);
                        }
                        else if (enemyState == DiceState.Paper)
                        {
                            toKill.Add(this);
                        }
                        else if (enemyState == DiceState.Rock)
                        {
                            //Draw
                        }
                        break;
                    case DiceState.Paper:
                        if (enemyState == DiceState.Rock)
                        {
                            toKill.Add(enemyDie);
                            Debug.Log(state);
                            Debug.Log(enemyState);
                        }
                        else if (enemyState == DiceState.Scissors)
                        {
                            toKill.Add(this);
                        }
                        else if (enemyState == DiceState.Paper)
                        {
                            //Draw
                        }
                        break;
                    case DiceState.Scissors:
                        if (enemyState == DiceState.Paper)
                        {
                            toKill.Add(enemyDie);
                            Debug.Log(state);
                            Debug.Log(enemyState);
                        }
                        else if (enemyState == DiceState.Rock)
                        {
                            toKill.Add(this);
                        }
                        else if (enemyState == DiceState.Scissors)
                        {
                            //Draw
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
        Destroy(gameObject);
    }

    public void ShowTilesInRange()
    {
        if (isEnemy) return;

        GhostManager.Instance.RemoveGhosts(gameObject);
        foreach (OverlayTile tile in _tilesInRange)
        {
            if (tile.isBlocked) continue;
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
        if (_currentRange <= 0) return;
        _tilesInRange = MapManager.Instance.GetSurroundingTiles(new Vector2Int(parentTile.gridLocation.x, parentTile.gridLocation.y));
    }

    public void ResetRange()
    {
        _currentRange = _maxRange;
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
    }

    private IEnumerator UpdateTilePos(OverlayTile newTile)
    {
        HideTilesInRange();
        yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);

        parentTile.RemoveDiceFromTile();
        newTile.MoveDiceToTile(this);
        _currentRange--;
        if (!isEnemy && _currentRange == 0) {
            GameManager.Instance.CurrentTurn = Turn.Enemy;
        }
        GetTilesInRange();
        Fight();
    }

    void TurnChange(Turn turn) {
        if (isEnemy && turn == Turn.Enemy) {
            ResetRange();
        } else if (!isEnemy && turn == Turn.Player) {
            ResetRange();
        }
        Debug.Log("Turn change " + this + ":"+ _currentRange + "/" + _maxRange);
    }

    void OnDestroy() {
        GhostManager.Instance.RemoveGhosts(gameObject);
        GameManager.Instance.TurnChange -= turnChange;
    }
}
