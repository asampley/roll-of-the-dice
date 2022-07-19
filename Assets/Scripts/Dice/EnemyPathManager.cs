using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPathManager : MonoBehaviour {
    private static EnemyPathManager _instance;
    public static EnemyPathManager Instance { get { return _instance; } }

    public HashSet<Vector2Int> taken = new HashSet<Vector2Int>();

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
