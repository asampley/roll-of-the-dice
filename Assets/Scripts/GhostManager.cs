using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostManager : MonoBehaviour {
    private static GhostManager _instance;
    public static GhostManager Instance { get { return _instance; } }

    private Dictionary<Vector2Int, GameObject> ghosts = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<GameObject, List<GameObject>> ghostsByContext = new Dictionary<GameObject, List<GameObject>>();

    public GameObject ghostContainer;

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

    public GameObject CreateGhost(GameObject toGhost, Vector2Int pos, int xRot, int yRot) {
        if (ghosts.ContainsKey(pos)) return null;

        var dieManager = toGhost.GetComponent<DieManager>();
        var ghostComponents = dieManager.ghostComponents;

        var ghost = Instantiate(ghostContainer, MapManager.Instance.GetTileWorldSpace(pos), toGhost.transform.rotation, this.transform);
        Instantiate(ghostComponents, ghost.transform);

        ghost.GetComponentInChildren<MeshRenderer>().sharedMaterial = dieManager.ghostMaterial;
        var rotator = ghost.GetComponentInChildren<DieRotator>();

        rotator.RotateX(xRot);
        rotator.RotateY(yRot);

        ghosts.Add(pos, ghost);
        if (!ghostsByContext.ContainsKey(toGhost)) {
            ghostsByContext.Add(toGhost, new List<GameObject>());
        }

        ghostsByContext[toGhost].Add(ghost);

        return ghost;
    }

    public void RemoveGhosts(GameObject context) {
        if (!ghostsByContext.ContainsKey(context)) return;

        var ghostsToRemove = ghostsByContext[context];

        foreach (var ghostKeyVal in ghosts.Where(kvp => ghostsToRemove.Contains(kvp.Value)).ToList()) {
            ghosts.Remove(ghostKeyVal.Key);
        }

        foreach (var ghost in ghostsByContext[context]) {
            Destroy(ghost);
        }
    }

    public void SetGhostVisible(Vector2Int pos, bool visible) {
        if (!ghosts.ContainsKey(pos)) return;

        ghosts[pos].SetActive(visible);
    }

    public void Clear() {
        foreach (var ghost in ghosts.Values) {
            Destroy(ghost);
        }

        ghosts.Clear();
        ghostsByContext.Clear();
    }
}
