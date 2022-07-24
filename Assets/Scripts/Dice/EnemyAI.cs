using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour, PhaseListener {
    private List<Vector2Int> path = new List<Vector2Int>();

    private DieManager dieManager;

    // Start is called before the first frame update
    void Start() {
        OnPhaseChange(GameManager.Instance.CurrentPhase);
    }

    void OnEnable() {
        dieManager = GetComponent<DieManager>();

        GameManager.Instance.PhaseChange += OnPhaseChange;
        dieManager.MoveFinished += OnMoveFinished;
    }

    void OnDisable() {
        GameManager.Instance.PhaseChange -= OnPhaseChange;
        dieManager.MoveFinished -= OnMoveFinished;
    }

    private List<OverlayTile> GetTilesBeside(Vector2Int pos) {
        return MapManager.Instance.GetSurroundingTiles(pos);
    }

    public void CreatePath() {
        Debug.Log("Garfield Starting CreatePath: " + transform.name);
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
                .Where(a => !EnemyPathManager.Instance.IsReserved(a))
                .ToList();

            if (adjacent.Count == 0) break;

            var next = adjacent[(int)(UnityEngine.Random.value * adjacent.Count) % adjacent.Count];

            path.Add(next - pos);
            EnemyPathManager.Instance.Reserve(this, next);

            deltas.Add(next - pos);
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
        Debug.Log("Garfield Ending CreatePath: " + transform.name);
        Debug.Log("Created Path: " + PathStr());
    }

    public void FollowPath() {
        Debug.Log("Garfield Starting FollowPath: " + transform.name);
        Debug.Log("Following Path: " + PathStr());

        GhostManager.Instance.RemoveGhosts(gameObject);

        dieManager.Move(PathGenerator());

        Debug.Log("Garfield Ending FollowPath: " + transform.name);
    }

    private string PathStr() {
        return (Vector2Int)dieManager.parentTile.gridLocation + " -> " + Utilities.EnumerableString(path);
    }

    private IEnumerator<OverlayTile> PathGenerator() {
        foreach (var p in path) {
            OverlayTile tile;
            try {
                tile = MapManager.Instance.GetTileAtPos(
                    (Vector2Int)dieManager.parentTile.gridLocation + p
                );
            } catch (KeyNotFoundException) {
                Debug.Log("Tile does not exist, stopping path");
                break;
            }

            yield return tile;
        }
    }

    public void OnPhaseChange(Phase phase) {
        switch(phase) {
            case Phase.Enemy:
                GameManager.Instance.AddPhaseProcessing(this);
                FollowPath();
                break;
            case Phase.Player:
                CreatePath();
                break;
        }
    }

    private void OnMoveFinished(OverlayTile tile) {
        ClearPath();
        GameManager.Instance.RemovePhaseProcessing(this);
    }

    void UnreservePath() {
        EnemyPathManager.Instance.ClearReserved(this);
    }

    void ClearPath() {
        UnreservePath();

        path.Clear();
    }

    void OnDestroy() {
        GameManager.Instance.RemovePhaseProcessing(this);

        UnreservePath();
    }
}
