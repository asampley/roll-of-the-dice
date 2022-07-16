using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour {
    private static HashSet<Vector2Int> taken = new HashSet<Vector2Int>();

    private List<Vector2Int> path = new List<Vector2Int>();

    private DieManager dieManager;

    private Action<Turn> turnChange;

    // Start is called before the first frame update
    void Start() {
        dieManager = GetComponent<DieManager>();

        turnChange = t => this.TurnChange(t);
        GameManager.Instance.TurnChange += turnChange;

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

        List<Vector2Int> rots = new List<Vector2Int>();

        while (currentRange > 0) {
            var adjacent = GetTilesBeside(pos)
                .Where(a => a.occupyingDie == null)
                .Select(a => (Vector2Int)a.gridLocation)
                .Where(a => !taken.Contains(a))
                .ToList();

            if (adjacent.Count == 0) break;

            Vector2Int next = adjacent[(int)(UnityEngine.Random.value * adjacent.Count) % adjacent.Count];

            path.Add(next);
            taken.Add(next);

            rots.Add(pos - next);

            var ghost = GhostManager.Instance.CreateGhost(gameObject, next, 0, 0);
            foreach (var rot in rots) {
                var rotator = ghost.GetComponentInChildren<DieRotator>();
                rotator.RotateX(rot.x);
                rotator.RotateY(rot.y);
            }

            currentRange--;
            pos = next;
        }

        string pathstr = "";
        foreach (var p in path) pathstr += p;
        Debug.Log("Path: " + pathstr);
    }

    private void FollowPath() {

    }

    private void TurnChange(Turn turn) {
        if (turn == Turn.Enemy) {
            FollowPath();
            GameManager.Instance.EnemiesWaiting.Remove(this);

            if (GameManager.Instance.EnemiesWaiting.Count == 0) {
                GameManager.Instance.CurrentTurn = Turn.Player;
            }
        } else {
            CreatePath();
            GameManager.Instance.EnemiesWaiting.Add(this);
        }
    }

    void OnDestroy() {
        GameManager.Instance.TurnChange -= turnChange;
        GameManager.Instance.EnemiesWaiting.Remove(this);
    }
}
