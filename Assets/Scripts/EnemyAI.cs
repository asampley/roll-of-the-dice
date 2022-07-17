using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour {
    private List<Vector2Int> path = new List<Vector2Int>();

    private DieManager dieManager;

    private Action<Turn> turnChange;
    private Action<OverlayTile> moveFinished;

    // Start is called before the first frame update
    void Start() {
        dieManager = GetComponent<DieManager>();

        turnChange = t => this.TurnChange(t);
        GameManager.Instance.TurnChange += turnChange;
        moveFinished = t => this.MoveFinished(t);
        dieManager.MoveFinished += moveFinished;

        TurnChange(GameManager.Instance.CurrentTurn);
    }

    private List<OverlayTile> GetTilesBeside(Vector2Int pos) {
        return MapManager.Instance.GetSurroundingTiles(pos);
    }

    public void CreatePath() {
        Debug.Log("Create Path: currently taken " + EnemyPathManager.Instance.TakenStr());

        Vector2Int pos = new Vector2Int(dieManager.parentTile.gridLocation.x, dieManager.parentTile.gridLocation.y);
        int currentRange = dieManager.MaxRange();

        List<Vector2Int> rots = new List<Vector2Int>();

        while (currentRange > 0) {
            var adjacent = GetTilesBeside(pos)
                .Where(a => !a.IsBlocked)
                .Select(a => (Vector2Int)a.gridLocation)
                .Where(a => !EnemyPathManager.Instance.taken.Contains(a))
                .ToList();

            if (adjacent.Count == 0) break;

            Vector2Int next = adjacent[(int)(UnityEngine.Random.value * adjacent.Count) % adjacent.Count];

            path.Add(next);
            EnemyPathManager.Instance.taken.Add(next);

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

        Debug.Log("Path: " + PathStr());
    }

    public void FollowPath() {
        Debug.Log("Following Path: " + PathStr());

        GhostManager.Instance.RemoveGhosts(gameObject);

        var tiles = path.Select(x => MapManager.Instance.GetTileAtPos(x)).ToList();
        dieManager.Move(tiles);

        foreach (var p in path) {
            EnemyPathManager.Instance.taken.Remove(p);
        }
        path.Clear();
    }

    private string PathStr() {
        string pathstr = "" + new Vector2Int(dieManager.parentTile.gridLocation.x, dieManager.parentTile.gridLocation.y) + " -> ";
        foreach (var p in path) pathstr += p;
        return pathstr;
    }

    private void TurnChange(Turn turn) {
        if (turn == Turn.Enemy) {
            FollowPath();
        } else if (turn == Turn.Player) {
            CreatePath();
            GameManager.Instance.EnemiesWaiting.Add(this);
        }
    }

    private void MoveFinished(OverlayTile tile) {
        GameManager.Instance.EnemiesWaiting.Remove(this);

        if (GameManager.Instance.EnemiesWaiting.Count == 0) {
            GameManager.Instance.CurrentTurn = Turn.Player;
        }
    }

    void OnDestroy() {
        if (turnChange != null) {
            GameManager.Instance.TurnChange -= turnChange;
        }

        GameManager.Instance.EnemiesWaiting.Remove(this);

        if (moveFinished != null) {
            dieManager.MoveFinished -= moveFinished;
        }

        foreach (var p in path) {
            EnemyPathManager.Instance.taken.Remove(p);
        }
    }
}
