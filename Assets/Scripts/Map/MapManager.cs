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
    private Dictionary<TileBase, TileData> tileDataDict = new Dictionary<TileBase, TileData>();

    private void Awake()
    {
        if (_instance == null)
            _instance = this;

        foreach (var data in Resources.LoadAll<TileData>("ScriptableObjects/TileData/"))
            foreach (var tile in data.tiles)
                tileDataDict.Add(tile, data);
    }

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
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
                        var cellWorldPos = TileToWorldSpace(tileLocation);

                        overlayTile.transform.position = new Vector3(cellWorldPos.x, cellWorldPos.y, cellWorldPos.z);
                        overlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder;
                        overlayTile.gridLocation = tileLocation;
                        overlayTile.GetComponent<OverlayTile>().HideTile();

                        overlayTile.data = GetTileData(tileLocation);

                        map.Add(tileKey, overlayTile);
                    }
                }
            }
        }
    }

    public void ClearMap()
    {
        foreach (Transform child in tileParent.transform)
        {
            if (child.gameObject != null)
                GameObject.Destroy(child.gameObject);
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

    public TileData GetTileData(Vector3Int pos) {
        TileBase tileBase = tileMap.GetTile(pos);

        if (!tileDataDict.ContainsKey(tileBase)) {
            return null;
        } else {
            return tileDataDict[tileBase];
        }
    }

    public OverlayTile GetTileAtPos(Vector2Int pos)
    {
        return map[pos];
    }

    public Vector3 TileToWorldSpace(Vector2Int pos)
    {
        return TileToWorldSpace((Vector3Int)pos);
    }

    public Vector3 TileToWorldSpace(Vector3Int pos)
    {
        Vector3 vec = this.tileMap.GetCellCenterWorld(pos);
        return vec + 4.0f * Vector3.forward * vec.y - 1.5f * Vector3.forward;
    }

    public Vector3 TileDeltaToWorldDelta(Vector2Int delta) {
        return TileToWorldSpace(delta) - TileToWorldSpace(Vector2Int.zero);
    }

    public List<OverlayTile> GetSurroundingTiles(Vector2Int originTile)
    {
        var surroundingTiles = new List<OverlayTile>();


        Vector2Int TileToCheck = new Vector2Int(originTile.x + 1, originTile.y);
        if (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].transform.position.z - map[originTile].transform.position.z) <= 1)
                surroundingTiles.Add(map[TileToCheck]);
        }

        TileToCheck = new Vector2Int(originTile.x - 1, originTile.y);
        if (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].transform.position.z - map[originTile].transform.position.z) <= 1)
                surroundingTiles.Add(map[TileToCheck]);
        }

        TileToCheck = new Vector2Int(originTile.x, originTile.y + 1);
        if (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].transform.position.z - map[originTile].transform.position.z) <= 1)
                surroundingTiles.Add(map[TileToCheck]);
        }

        TileToCheck = new Vector2Int(originTile.x, originTile.y - 1);
        if (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].transform.position.z - map[originTile].transform.position.z) <= 1)
                surroundingTiles.Add(map[TileToCheck]);
        }

        return surroundingTiles;
    }

    public List<OverlayTile> GetTilesStraightLine(Vector2Int originTile)
    {
        var surroundingTiles = new List<OverlayTile>();


        Vector2Int TileToCheck = new Vector2Int(originTile.x + 1, originTile.y);
        while (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].transform.position.z - map[originTile].transform.position.z) <= 1)
                if (!map[TileToCheck].IsBlocked)
                    surroundingTiles.Add(map[TileToCheck]);
                else
                    break;
            TileToCheck.x += 1;
        }

        TileToCheck = new Vector2Int(originTile.x - 1, originTile.y);
        while (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].transform.position.z - map[originTile].transform.position.z) <= 1)
                if (!map[TileToCheck].IsBlocked)
                    surroundingTiles.Add(map[TileToCheck]);
                else
                    break;
            TileToCheck.x -= 1;
        }

        TileToCheck = new Vector2Int(originTile.x, originTile.y + 1);
        while (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].transform.position.z - map[originTile].transform.position.z) <= 1)
                if (!map[TileToCheck].IsBlocked)
                    surroundingTiles.Add(map[TileToCheck]);
                else
                    break;
            TileToCheck.y += 1;
        }

        TileToCheck = new Vector2Int(originTile.x, originTile.y - 1);
        while (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].transform.position.z - map[originTile].transform.position.z) <= 1)
                if (!map[TileToCheck].IsBlocked)
                    surroundingTiles.Add(map[TileToCheck]);
                else
                    break;
            TileToCheck.y -= 1;
        }

        return surroundingTiles;
    }

    public void OnDestroy()
    {
        _instance = null;
    }
}
