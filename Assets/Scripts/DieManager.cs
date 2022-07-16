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
        parentTile.RemoveDiceFromTile();
        newTile.MoveDiceToTile(this);
        transform.position = newTile.transform.position;
        
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
       foreach (OverlayTile tile in _tilesInRange)
        {
            tile.ShowTile();
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
}
