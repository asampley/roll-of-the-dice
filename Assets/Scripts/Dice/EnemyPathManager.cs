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
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
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
}
