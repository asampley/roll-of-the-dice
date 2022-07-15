using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance {  get { return _instance; } }
    public OverlayTile overlayTilePrefab;
    public GameObject tileParent;
    public Tilemap tileMap;

    public Dictionary<Vector2Int, OverlayTile> map;

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
        BoundsInt bounds = tileMap.cellBounds;
        map = new Dictionary<Vector2Int, OverlayTile>();

        for (int z = bounds.max.z; z >= bounds.min.z; z--)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int x = bounds.min.x; x < bounds.max.x; x++)
                {
                    var tileLocation = new Vector3Int(x, y, z);
                    var tileKey = new Vector2Int(x, y);

                    if (tileMap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                    {
                        var overlayTile = Instantiate(overlayTilePrefab, tileParent.transform);
                        var cellWorldPos = tileMap.GetCellCenterWorld(tileLocation);

                        overlayTile.transform.position = new Vector3(cellWorldPos.x, cellWorldPos.y, cellWorldPos.z);
                        overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder + 1;
                        overlayTile.gridLocation = tileLocation;
                        overlayTile.GetComponent<OverlayTile>().HideTile();
                        map.Add(tileKey, overlayTile);
                    }
                }
            }
        }
    }

    public RaycastHit2D? GetFocusedTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2d = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2d, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }

        return null;
    }

    public Collider2D GetTileAtPos(Vector2 pos)
    {
        Collider2D[] col = Physics2D.OverlapCircleAll(pos, 1.0f);
        Debug.Log(col[0]);

        return col.OrderByDescending(i => Vector2.Distance(i.transform.position, pos)).First();
    }

    public Vector3 GetTileWorldSpace(Vector2Int pos)
    {
        return this.tileMap.GetCellCenterWorld((Vector3Int)pos);
    }
}
