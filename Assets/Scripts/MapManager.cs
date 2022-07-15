using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance {  get { return _instance; } }
    public OverlayTile overlayTilePrefab;
    public GameObject tileParent;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        var tileMap = gameObject.GetComponentInChildren<Tilemap>();
        BoundsInt bounds = tileMap.cellBounds;

        for (int z = bounds.max.z; z > bounds.min.z; z--)
        {
            for (int y = bounds.max.y; y > bounds.min.y; y--)
            {
                for (int x = bounds.max.y; x > bounds.min.x; x--)
                {
                    var tileLocation = new Vector3Int(x, y, z);

                    if (tileMap.HasTile(tileLocation))
                    {
                        var overlayTile = Instantiate(overlayTilePrefab, tileParent.transform);
                        var cellWorldPos = tileMap.GetCellCenterWorld(tileLocation);

                        overlayTile.transform.position = new Vector3(cellWorldPos.x, cellWorldPos.y, cellWorldPos.z - 1);
                        overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder;
                    }
                }
            }
        }
    }
}
