using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class EnemyAI : MonoBehaviour, PhaseListener {
    public MonoBehaviour Self { get { return this; } }

    private List<Vector2Int> path = new List<Vector2Int>();

    private DieManager dieManager;

    // Start is called before the first frame update
    void Start() {
        dieManager = GetComponent<DieManager>();

        if (GameManager.Instance.phaseManager.CurrentPhase != null) {
            OnPhaseEnter(GameManager.Instance.phaseManager.CurrentPhase.Value);
        }
    }

    void OnEnable() {
        GameManager.Instance.phaseManager.AllPhaseListeners.Add(this);
    }

    void OnDisable() {
        GameManager.Instance.phaseManager.AllPhaseListeners.Remove(this);
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

    public async UniTask StepPath(CancellationToken token) {
        Debug.Log("Garfield Starting StepPath: " + transform.name);
        Debug.Log("Following Path: " + PathStr());

        GhostManager.Instance.RemoveGhosts(gameObject);

        if (path.Count > 0) {
            OverlayTile tile;
            try {
                tile = MapManager.Instance.GetTileAtPos(
                    (Vector2Int)dieManager.parentTile.gridLocation + path[0]
                );

                path.RemoveAt(0);
            } catch (KeyNotFoundException) {
                Debug.Log("Tile does not exist, stopping path");
                ClearPath();

                return;
            }

            await dieManager.MoveAsync(tile, token);
        }

        Debug.Log("Garfield Ending StepPath: " + transform.name);
    }

    private string PathStr() {
        return (Vector2Int)dieManager.parentTile.gridLocation + " -> " + Utilities.EnumerableString(path);
    }

    public PhaseStepResult OnPhaseEnter(Phase phase) {
        switch(phase) {
            case Phase.Enemy:
                return PhaseStepResult.ShouldContinue;
            case Phase.Player:
                CreatePath();
                return PhaseStepResult.Done;
            default:
                return PhaseStepResult.Done;
        }
    }

    public async UniTask<PhaseStepResult> OnPhaseUpdate(Phase phase, CancellationToken token) {
        Debug.Log("Phase update: " + name);
        switch(phase) {
            case Phase.Enemy:
                if (path.Count == 0) {
                    ClearPath();
                    return PhaseStepResult.Done;
                } else {
                    await StepPath(token);
                    return PhaseStepResult.ShouldContinue;
                }
            default:
                return PhaseStepResult.Done;
        }
    }

    void UnreservePath() {
        EnemyPathManager.Instance.ClearReserved(this);
    }

    void ClearPath() {
        UnreservePath();

        path.Clear();
    }

    void OnDestroy() {
        UnreservePath();
    }
}
