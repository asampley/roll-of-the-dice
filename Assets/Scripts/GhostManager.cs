using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostManager : MonoBehaviour {
    private GhostManager _instance;
    public GhostManager Instance { get { return _instance; } }

    public GameObject GhostPrefab;

    private Dictionary<Vector2Int, GameObject> ghosts = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<GameObject, GameObject[]> ghostsByContext = new Dictionary<GameObject, GameObject[]>();

    public bool CreateGhost(GameObject context, Vector2Int pos, Quaternion rot) {
        if (ghosts.ContainsKey(pos)) return false;

        Instantiate(GhostPrefab, MapManager.Instance.GetTileWorldSpace(pos), rot, this.transform);

        return true;
    }

    public void RemoveGhosts(GameObject context) {
        var ghostsToRemove = ghostsByContext[context];

        if (ghostsToRemove == null) return;

        foreach (var ghostKeyVal in ghosts.Where(kvp => ghostsToRemove.Contains(kvp.Value)).ToList()) {
            ghosts.Remove(ghostKeyVal.Key);
        }

        foreach (var ghost in ghostsByContext[context]) {
            Destroy(ghost);
        }
    }

    public void Clear() {
        foreach (var ghost in ghosts.Values) {
            Destroy(ghost);
        }

        ghosts.Clear();
        ghostsByContext.Clear();
    }
}
