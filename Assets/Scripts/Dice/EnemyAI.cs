using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour {
    private List<Vector2Int> path = new List<Vector2Int>();

    private DieManager dieManager;

    // Start is called before the first frame update
    void Start() {
        dieManager = GetComponent<DieManager>();

        GameManager.Instance.TurnChange += TurnChange;
        dieManager.MoveFinished += MoveFinished;

        TurnChange(GameManager.Instance.CurrentTurnValue);
    }

    private List<OverlayTile> GetTilesBeside(Vector2Int pos) {
        return MapManager.Instance.GetSurroundingTiles(pos);
    }

    public void CreatePath() {
        Debug.Log("Garfield Starting CreatePath: " + dieManager.diceName);
        Debug.Log("Create Path: currently taken " + EnemyPathManager.Instance.TakenStr());

        Vector2Int start = (Vector2Int)dieManager.parentTile.gridLocation;
        Vector2Int pos = start;

        int currentRange = dieManager.MaxRange();

        List<Vector2Int> deltas = new List<Vector2Int>();
        List<Vector3> trans = new List<Vector3>();

        while (currentRange > 0) {
            var adjacent = GetTilesBeside(pos)
                .Where(a => !a.IsBlocked)
                .Select(a => (Vector2Int)a.gridLocation)
                .Where(a => !EnemyPathManager.Instance.taken.Contains(a))
                .ToList();

            if (adjacent.Count == 0) break;

            var next = adjacent[(int)(UnityEngine.Random.value * adjacent.Count) % adjacent.Count];

            path.Add(next);
            EnemyPathManager.Instance.taken.Add(next);

            deltas.Add(pos - next);
            trans.Add(
                MapManager.Instance.TileToWorldSpace(new Vector2Int(0, 0))
                - MapManager.Instance.TileToWorldSpace(pos - next)
            );

            var ghost = GhostManager.Instance.CreateGhost(gameObject, null, null);

            var translator = ghost.GetComponentInChildren<DieTranslator>();
            foreach (var t in trans) {
                translator.Translate(t);
            }

            var rotator = ghost.GetComponentInChildren<DieRotator>();
            foreach (var delta in deltas) {
                rotator.RotateTileDelta(delta);
            }

            currentRange--;
            pos = next;
        }
        Debug.Log("Garfield Ending CreatePath: " + dieManager.diceName);
        Debug.Log("Created Path: " + PathStr());
    }

    public void FollowPath() {
        Debug.Log("Garfield Starting FollowPath: " + dieManager.diceName);
        Debug.Log("Following Path: " + PathStr());

        GhostManager.Instance.RemoveGhosts(gameObject);

        var tiles = path.Select(x => MapManager.Instance.GetTileAtPos(x)).ToList();
        dieManager.Move(tiles);

        ClearPath();
        Debug.Log("Garfield Ending FollowPath: " + dieManager.diceName);
    }

    private string PathStr() {
        return (Vector2Int)dieManager.parentTile.gridLocation + " -> " + Utilities.EnumerableString(path);
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
        GameManager.Instance.RemoveEnemyWaiting(this);

        GameManager.Instance.TurnChange -= TurnChange;
        dieManager.MoveFinished -= MoveFinished;

        ClearPath();
    }
}
