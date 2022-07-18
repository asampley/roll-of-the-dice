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

        OverlayTile startTile = dieManager.parentTile;
        Vector2Int pos = new Vector2Int(startTile.gridLocation.x, startTile.gridLocation.y);

        int currentRange = dieManager.MaxRange();

        List<OverlayTile> tiles = new List<OverlayTile>();
        tiles.Add(startTile);

        List<Vector2Int> rots = new List<Vector2Int>();

        while (currentRange > 0) {
            var adjacent = GetTilesBeside(pos)
                .Where(a => !a.IsBlocked)
                .Where(a => !EnemyPathManager.Instance.taken.Contains((Vector2Int)a.gridLocation))
                .ToList();

            if (adjacent.Count == 0) break;

            var next = adjacent[(int)(UnityEngine.Random.value * adjacent.Count) % adjacent.Count];
            var nextCoord = (Vector2Int)next.gridLocation;

            tiles.Add(next);
            path.Add(nextCoord);
            EnemyPathManager.Instance.taken.Add(nextCoord);

            rots.Add(pos - nextCoord);

            var ghost = GhostManager.Instance.CreateGhost(gameObject, Vector3.zero, 0, 0);

            var translator = ghost.GetComponentInChildren<DieTranslator>();
            // required to skip first time step of translation of ghost
            translator.TranslateNow();
            for (var i = 1; i < tiles.Count; ++i) {
                translator.Translate(
                    MapManager.Instance.GetWorldSpace(tiles[i].transform.position)
                    - MapManager.Instance.GetWorldSpace(tiles[i - 1].transform.position)
                );
            }

            var rotator = ghost.GetComponentInChildren<DieRotator>();
            foreach (var rot in rots) {
                rotator.RotateX(rot.x);
                rotator.RotateY(rot.y);
            }

            currentRange--;
            pos = nextCoord;
        }

        Debug.Log("Path: " + PathStr());
    }

    public void FollowPath() {
        Debug.Log("Following Path: " + PathStr());

        GhostManager.Instance.RemoveGhosts(gameObject);

        var tiles = path.Select(x => MapManager.Instance.GetTileAtPos(x)).ToList();
        dieManager.Move(tiles);

        ClearPath();
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
            GameManager.Instance.AddEnemyWaiting(this);
        }
    }

    private void MoveFinished(OverlayTile tile) {
        GameManager.Instance.RemoveEnemyWaiting(this);
    }

    void ClearPath() {
        foreach (var p in path) {
            EnemyPathManager.Instance.taken.Remove(p);
        }

        path.Clear();
    }

    void OnDestroy() {
        if (turnChange != null) {
            GameManager.Instance.TurnChange -= turnChange;
        }

        GameManager.Instance.RemoveEnemyWaiting(this);

        if (moveFinished != null) {
            dieManager.MoveFinished -= moveFinished;
        }

        ClearPath();
    }
}
