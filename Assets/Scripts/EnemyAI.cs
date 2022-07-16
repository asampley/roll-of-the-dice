using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour {
    private static HashSet<Vector2Int> taken = new HashSet<Vector2Int>();

    private List<Vector2Int> path = new List<Vector2Int>();

    private DieManager dieManager;

    // Start is called before the first frame update
    void Start() {
        dieManager = GetComponent<DieManager>();

        CreatePath();
    }

    // Update is called once per frame
    void Update() {

    }

    private List<OverlayTile> GetTilesBeside(Vector2Int pos) {
        return MapManager.Instance.GetSurroundingTiles(pos);
    }

    public void CreatePath() {
        Vector2Int pos = new Vector2Int(dieManager.parentTile.gridLocation.x, dieManager.parentTile.gridLocation.y);
        int currentRange = dieManager.MaxRange();

        while (currentRange > 0) {
            var adjacent = GetTilesBeside(pos)
                .Where(a => a.occupyingDie == null)
                .Select(a => (Vector2Int)a.gridLocation)
                .Where(a => !taken.Contains(a))
                .ToList();

            if (adjacent.Count == 0) break;

            Vector2Int next = adjacent[(int)(Random.value * adjacent.Count) % adjacent.Count];

            path.Add(next);
            taken.Add(next);

            var rot = pos - next;
            GhostManager.Instance.CreateGhost(gameObject, next, rot.x, rot.y);

            currentRange--;
            pos = next;
        }

        string pathstr = "";
        foreach (var p in path) pathstr += p;
        Debug.Log("Path: " + pathstr);
    }
}
