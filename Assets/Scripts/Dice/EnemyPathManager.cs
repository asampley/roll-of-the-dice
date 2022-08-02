using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPathManager : MonoBehaviour {
    private static EnemyPathManager _instance;
    public static EnemyPathManager Instance { get { return _instance; } }

    private HashSet<Vector2Int> taken = new HashSet<Vector2Int>();
    private Dictionary<EnemyAI, List<Vector2Int>> takenBy = new Dictionary<EnemyAI, List<Vector2Int>>();

    private Dictionary<MovementPattern, Dijkstra<Vector2Int>> dijkstras = new();

    public bool Reserve(EnemyAI enemy, Vector2Int pos) {
        if (taken.Contains(pos)) return false;

        taken.Add(pos);
        if (!takenBy.ContainsKey(enemy)) {
            takenBy.Add(enemy, new List<Vector2Int>());
        }

        takenBy[enemy].Add(pos);

        return true;
    }

    public void ClearReserved(EnemyAI enemy) {
        if (!takenBy.ContainsKey(enemy)) return;

        foreach (var p in takenBy[enemy]) {
            taken.Remove(p);
        }

        takenBy.Remove(enemy);
    }

    public bool IsReserved(Vector2Int pos) {
        return taken.Contains(pos);
    }

    void Awake() {
        if (_instance == null)
            _instance = this;
    }

    public void ResetReserved() {
        taken.Clear();
        Debug.Log("Reset taken: " + TakenStr());
    }

    public string TakenStr() {
        string takenstr = "";
        foreach (var p in taken) takenstr += p;
        return takenstr;
    }

    public void NewPathFinder(IEnumerable<Vector2Int> targets) {
        this.dijkstras.Clear();

        Dijkstra<Vector2Int> single = new (
            p => MapManager.Instance.GetSurroundingTiles(p)
                .Where(t => !t.IsBlocked)
                .Select(t => (Vector2Int)t.gridLocation),
            targets
        );

        this.dijkstras.Add(MovementPattern.Single, single);
        this.dijkstras.Add(MovementPattern.Straight, single);

        Dijkstra<Vector2Int> knight = new (
            p => MapManager.Instance.GetTilesKnight(p)
                .Where(t => !t.IsBlocked)
                .Select(t => (Vector2Int)t.gridLocation),
            targets
        );

        this.dijkstras.Add(MovementPattern.Knight, knight);
    }

    public int NearnessToPlayer(MovementPattern pattern, Vector2Int position) {
        return this.dijkstras[pattern].Cost(position);
    }

    public bool TryNearnessToPlayer(MovementPattern pattern, Vector2Int position, out int nearness) {
        return this.dijkstras[pattern].TryCost(position, out nearness);
    }
}
