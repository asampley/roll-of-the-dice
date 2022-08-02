using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum MovementStrategy {
    Aggressive,
    Evasive,
    Random,
}

public class EnemyAI : MonoBehaviour, PhaseListener {
    public MonoBehaviour Self { get { return this; } }

    private UnitManager _unitManager;

    private MovementStrategy Strategy {
        get { return _unitManager.Unit.MovementStrategy; }
    }

    // Start is called before the first frame update
    void Start() {
        if (GameManager.Instance.phaseManager.CurrentPhase != null) {
            OnPhaseEnter(GameManager.Instance.phaseManager.CurrentPhase.Value);
        }
    }

    void OnEnable() {
        _unitManager = GetComponent<UnitManager>();
        GameManager.Instance.phaseManager.AllPhaseListeners.Add(this);
    }

    void OnDisable() {
        GameManager.Instance.phaseManager.AllPhaseListeners.Remove(this);
    }

    private List<OverlayTile> GetTilesBeside(Vector2Int pos) {
        return MapManager.Instance.GetSurroundingTiles(pos);
    }

    public void CreatePath() {
        Debug.Log("Create Path: currently taken " + EnemyPathManager.Instance.TakenStr());

        Vector2Int start = (Vector2Int)_unitManager.parentTile.gridLocation;
        Vector2Int pos = start;

        int currentMoves = _unitManager.MaxMoves;

        List<Vector2Int> deltas = new();
        List<Vector3> trans = new();

        GhostManager.Instance.RemoveArrow(this.gameObject);
        GhostManager.Instance.PushArrow(this.gameObject, pos);

        while (currentMoves > 0) {
            var adjacent = _unitManager.GetTilesInRange(pos)
                .Where(a => !a.IsBlocked)
                .Select(a => (Vector2Int)a.gridLocation)
                .Where(a => !EnemyPathManager.Instance.IsReserved(a))
                .ToList();

            if (adjacent.Count == 0) break;

            Vector2Int next;

            switch (Strategy) {
                case MovementStrategy.Aggressive:
                case MovementStrategy.Evasive:
                    next = adjacent[0];
                    int nearness = EnemyPathManager.Instance.NearnessToPlayer(_unitManager.MovementPattern, next);

                    foreach (var a in adjacent) {
                        var n = EnemyPathManager.Instance.NearnessToPlayer(_unitManager.MovementPattern, a);
                        if (
                            (Strategy == MovementStrategy.Aggressive && n < nearness)
                            || (Strategy == MovementStrategy.Evasive && n > nearness)
                        ) {
                            next = a;
                            nearness = n;
                        }
                    }
                    break;
                default:
                    next = adjacent[(int)(UnityEngine.Random.value * adjacent.Count) % adjacent.Count];
                    break;
            }

            _unitManager.path.Add(next - pos);
            EnemyPathManager.Instance.Reserve(this, next);

            deltas.Add(next - pos);
            trans.Add(
                MapManager.Instance.TileToWorldSpace(new Vector2Int(0, 0))
                - MapManager.Instance.TileToWorldSpace(pos - next)
            );

            GhostManager.Instance.SetupGhostEffects(this.gameObject, next, trans, deltas);

            currentMoves--;
            pos = next;
        }
        Debug.Log("Created Path: " + _unitManager.PathStr());
        DataHandler.SaveGameData();
    }


    public PhaseStepResult OnPhaseEnter(Phase phase) {
        switch(phase) {
            case Phase.Enemy:
                if (_unitManager.Unit.LoadFromSave)
                {
                    _unitManager.Unit.LoadFromSave = false;
                    return PhaseStepResult.Unchanged;
                }
                GhostManager.Instance.RemoveGhosts(gameObject);
                GhostManager.Instance.RemoveArrow(gameObject);
                UnreservePath();
                return PhaseStepResult.Unchanged;
            case Phase.Player:
                if (_unitManager.Unit.LoadFromSave)
                {
                    _unitManager.Unit.LoadFromSave = false;
                    return PhaseStepResult.Done;
                }
                return PhaseStepResult.Unchanged;
            default:
                return PhaseStepResult.Done;
        }
    }

    public async UniTask<PhaseStepResult> OnPhaseStep(Phase phase, CancellationToken token) {
        switch(phase) {
            case Phase.Player:
                CreatePath();
                return PhaseStepResult.Done;
            default:
                return PhaseStepResult.Done;
        }
    }

    void UnreservePath() {
        EnemyPathManager.Instance.ClearReserved(this);
    }

    void OnDestroy() {
        GhostManager.Instance.RemoveGhosts(gameObject);
        GhostManager.Instance.RemoveArrow(gameObject);

        UnreservePath();
    }
}
