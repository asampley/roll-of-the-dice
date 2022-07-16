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



    public void Initialize()
    {
        ResetRange();
        SetTexture();
    }

    private void SetTexture()
    {
    }

    public void Move(OverlayTile newTile)
    {
        if (!_tilesInRange.Contains(newTile) || isEnemy)
            return;
        CalculateDirection(newTile);
        Fight();
        StartCoroutine(UpdateTilePos(newTile));
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
        List<DieManager> deadDie = new List<DieManager>();

        foreach (OverlayTile tile in _tilesInRange)
        {
            if (tile.occupyingDie != null && isEnemy)
            {
                DieManager enemyDie = tile.occupyingDie;
                DiceState enemyState = enemyDie.state;
                if ((state == DiceState.Rock && enemyState == DiceState.Scissors) || (state == DiceState.Paper || enemyState == DiceState.Rock) || (state == DiceState.Scissors && enemyState == DiceState.Paper))
                {
                    deadDie.Add(enemyDie);
                }
                else if ((state == DiceState.Rock && enemyState == DiceState.Paper) || (state == DiceState.Paper || enemyState == DiceState.Scissors) || (state == DiceState.Scissors && enemyState == DiceState.Rock))
                {
                    deadDie.Add(this);
                }
                else if ((state == DiceState.Rock && enemyState == DiceState.Rock) || (state == DiceState.Paper || enemyState == DiceState.Paper) || (state == DiceState.Scissors && enemyState == DiceState.Scissors))
                {
                    //Draw
                }
            }
        }

        foreach (DieManager die in deadDie)
        {
            die.Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void ShowTilesInRange()
    {
        GhostManager.Instance.RemoveGhosts(gameObject);
        foreach (OverlayTile tile in _tilesInRange)
        {
            tile.ShowTile();
            Vector3Int rot = parentTile.gridLocation - tile.gridLocation;
            GhostManager.Instance.CreateGhost(gameObject, new Vector2Int(tile.gridLocation.x, tile.gridLocation.y), rot.x, rot.y);
        }
    }

    public void HideTilesInRange()
    {
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
        yield return new WaitForSeconds(Globals.MOVEMENT_TIME + 0.1f);

        parentTile.RemoveDiceFromTile();
        newTile.MoveDiceToTile(this);

        _currentRange--;
        HideTilesInRange();
        GetTilesInRange();
        ShowTilesInRange();
    }
}
