using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DieManager : MonoBehaviour
{
    private List<OverlayTile> _tilesInRange = new List<OverlayTile>();
    public OverlayTile parentTile;

    [SerializeField]
    private int _maxRange;
    private int _currentRange;


    public void Start()
    {
        ResetRange();
    }

    public void Move(OverlayTile newTile)
    {
        if (!_tilesInRange.Contains(newTile))
            return;
        CalculateDirection(newTile);
        parentTile.RemoveDiceFromTile();
        newTile.MoveDiceToTile(this);
        
        _currentRange--;
        HideTilesInRange();
        GetTilesInRange();
        ShowTilesInRange();

    }

    public void Select()
    {
        Globals.SELECTED_UNIT = this;
        GetTilesInRange();
        ShowTilesInRange();
    }

    public void Deselect()
    {
        HideTilesInRange();
    }

    public void ShowTilesInRange()
    {
        GhostManager.Instance.RemoveGhosts(gameObject);
        foreach (OverlayTile tile in _tilesInRange)
        {
            tile.ShowTile();            
            GhostManager.Instance.CreateGhost(gameObject, new Vector2Int(tile.gridLocation.x, tile.gridLocation.y), 1, 1);
        }
    }

    public void HideTilesInRange()
    {
        foreach (OverlayTile tile in _tilesInRange)
        {
            tile.HideTile();
        }
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
        Debug.Log(dir);

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
    }

    private IEnumerator MoveToPos(Vector2 startPos, Vector2 endPos)
    {
        float elapsedTime = 0;

        while (elapsedTime < Globals.MOVEMENT_TIME)
        {
            transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / Globals.MOVEMENT_TIME));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
