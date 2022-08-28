using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cysharp.Threading.Tasks;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance => _instance;
    private float zSpread;
    public OverlayTile overlayTilePrefab;
    public GameObject tileParent;
    public Tilemap tileMap;

    public Dictionary<Vector2Int, OverlayTile> map = new();
    private readonly Dictionary<TileBase, TileData> tileDataDict = new();

    private void Awake()
    {
        if (_instance == null)
            _instance = this;

        foreach (var data in Resources.LoadAll<TileData>("ScriptableObjects/TileData/"))
            foreach (var tile in data.tiles)
                tileDataDict.Add(tile, data);
        zSpread = tileMap.GetComponent<TilemapRenderer>().sharedMaterial.GetFloat("_ZSpread");
    }

    public async UniTask GenerateMap()
    {
        BoundsInt bounds = tileMap.cellBounds;

        for (int z = bounds.max.z; z >= bounds.min.z; z--)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int x = bounds.min.x; x < bounds.max.x; x++)
                {
                    Vector3Int tileLocation = new(x, y, z);
                    Vector2Int tileKey = new(x, y);

                    if (tileMap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                    {
                        var overlayTile = Instantiate(overlayTilePrefab, tileParent.transform);
                        var cellWorldPos = TileToWorldSpace(tileLocation);

                        overlayTile.transform.position = new Vector3(cellWorldPos.x, cellWorldPos.y, cellWorldPos.z + Globals.OVERLAY_TILE_Z_OFFSET);
                        overlayTile.gridLocation = tileLocation;
                        overlayTile.GetComponent<OverlayTile>().HideTile();

                        overlayTile.data = GetTileData(tileLocation);

                        map.Add(tileKey, overlayTile);
                    }
                }
            }
        }

        await UniTask.Yield();
    }

    public async UniTask ClearMap()
    {
        map.Clear();
        foreach (Transform child in tileParent.transform)
        {
            if (child.gameObject != null)
                GameObject.Destroy(child.gameObject);
        }

        await UniTask.Yield();
    }

    public RaycastHit2D? GetFocusedTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2d = new(mousePos.x, mousePos.y);
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
        vec += (zSpread * (pos.x + pos.y)) * Vector3.forward;
        return vec;
    }

    public Vector2 TileToWorldSpace2(Vector2Int pos)
    {
        Vector3 vec = this.tileMap.GetCellCenterWorld((Vector3Int)pos);
        vec += (zSpread * (pos.x + pos.y)) * Vector3.forward;
        return vec;
    }

    public Vector3 TileDeltaToWorldDelta(Vector2Int delta) {
        return TileToWorldSpace(delta) - TileToWorldSpace(Vector2Int.zero);
    }

    public List<OverlayTile> GetSurroundingTiles(Vector2Int originTile)
    {
        List<OverlayTile> surroundingTiles = new();


        Vector2Int TileToCheck = new(originTile.x + 1, originTile.y);
        if (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].gridLocation.z - map[originTile].gridLocation.z) <= 1)
                surroundingTiles.Add(map[TileToCheck]);
        }

        TileToCheck = new Vector2Int(originTile.x - 1, originTile.y);
        if (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].gridLocation.z - map[originTile].gridLocation.z) <= 1)
                surroundingTiles.Add(map[TileToCheck]);
        }

        TileToCheck = new Vector2Int(originTile.x, originTile.y + 1);
        if (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].gridLocation.z - map[originTile].gridLocation.z) <= 1)
                surroundingTiles.Add(map[TileToCheck]);
        }

        TileToCheck = new Vector2Int(originTile.x, originTile.y - 1);
        if (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].gridLocation.z - map[originTile].gridLocation.z) <= 1)
                surroundingTiles.Add(map[TileToCheck]);
        }

        return surroundingTiles;
    }

    public List<OverlayTile> GetTilesStraightLine(Vector2Int originTile)
    {
        List<OverlayTile> surroundingTiles = new();


        Vector2Int TileToCheck = new(originTile.x + 1, originTile.y);
        while (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].gridLocation.z - map[originTile].gridLocation.z) <= 1)
                if (!map[TileToCheck].IsBlocked)
                    surroundingTiles.Add(map[TileToCheck]);
                else
                    break;
            TileToCheck.x += 1;
        }

        TileToCheck = new Vector2Int(originTile.x - 1, originTile.y);
        while (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].gridLocation.z - map[originTile].gridLocation.z) <= 1)
                if (!map[TileToCheck].IsBlocked)
                    surroundingTiles.Add(map[TileToCheck]);
                else
                    break;
            TileToCheck.x -= 1;
        }

        TileToCheck = new Vector2Int(originTile.x, originTile.y + 1);
        while (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].gridLocation.z - map[originTile].gridLocation.z) <= 1)
                if (!map[TileToCheck].IsBlocked)
                    surroundingTiles.Add(map[TileToCheck]);
                else
                    break;
            TileToCheck.y += 1;
        }

        TileToCheck = new Vector2Int(originTile.x, originTile.y - 1);
        while (map.ContainsKey(TileToCheck))
        {
            if (Mathf.Abs(map[TileToCheck].gridLocation.z - map[originTile].gridLocation.z) <= 1)
                if (!map[TileToCheck].IsBlocked)
                    surroundingTiles.Add(map[TileToCheck]);
                else
                    break;
            TileToCheck.y -= 1;
        }

        return surroundingTiles;
    }

    public List<OverlayTile> GetTilesKnight(Vector2Int position) {
        List<OverlayTile> tiles = new();

        foreach (int a in new int[] {-2, 2}) {
            foreach (int b in new int[] {-1, 1}) {
                foreach (var delta in new Vector2Int[] {new(a, b), new(b, a)}) {
                    Vector2Int next = position + delta;

                    if (map.TryGetValue(next, out OverlayTile nextTile) && !nextTile.IsBlocked) {
                        tiles.Add(nextTile);
                    }
                }
            }
        }

        return tiles;
    }

    public void OnDestroy()
    {
        _instance = null;
    }

    public void DeclareInstance()
    {
        if (_instance == null)
            _instance = this;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;
        GenerateMap().Forget();
        foreach (KeyValuePair<Vector2Int, OverlayTile> pair in map)
        {
            pair.Value.gridLocationText.text = pair.Value.gridLocation.ToString();
        }
    }
}
