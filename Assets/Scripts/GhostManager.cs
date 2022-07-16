using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostManager : MonoBehaviour {
    private static GhostManager _instance;
    public static GhostManager Instance { get { return _instance; } }

    public Material ghostMaterial;

    private Dictionary<Vector2Int, GameObject> ghosts = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<GameObject, List<GameObject>> ghostsByContext = new Dictionary<GameObject, List<GameObject>>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public bool CreateGhost(GameObject toGhost, Vector2Int pos, int xRot, int yRot) {
        if (ghosts.ContainsKey(pos)) return false;

        var ghost = Instantiate(toGhost, MapManager.Instance.GetTileWorldSpace(pos), Quaternion.identity, this.transform);
        var rotator = ghost.GetComponentInChildren<DieRotator>();

        rotator.RotateX(xRot);
        rotator.RotateY(yRot);

        ghosts.Add(pos, ghost);
        if (ghostsByContext[toGhost] == null) {
            ghostsByContext[toGhost] = new List<GameObject>();
        }

        ghost.GetComponentInChildren<MeshRenderer>().sharedMaterial = ghostMaterial;

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
