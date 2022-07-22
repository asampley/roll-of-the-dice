using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostManager : MonoBehaviour {
    private static GhostManager _instance;
    public static GhostManager Instance { get { return _instance; } }

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

    public GameObject CreateGhost(GameObject toGhost, Vector3? translation, Vector2Int? tileDelta, int rotationCount = 1) {
        var dieManager = toGhost.GetComponent<DieManager>();
        var ghostComponents = dieManager.ghostComponents;

        var ghost = Instantiate(ghostContainer, toGhost.transform.position, toGhost.transform.rotation, this.transform);
        Instantiate(ghostComponents, ghost.transform);

        ghost.GetComponentInChildren<MeshRenderer>().sharedMaterial = dieManager.ghostMaterial;

        if (tileDelta != null) {
            var rotator = ghost.GetComponentInChildren<DieRotator>();

            rotator.RotateTileDelta(tileDelta.Value, rotationCount);
        }

        if (translation != null) {
            var translator = ghost.GetComponentInChildren<DieTranslator>();

            translator.Translate(translation.Value);
        }

        if (!ghostsByContext.ContainsKey(toGhost)) {
            ghostsByContext.Add(toGhost, new List<GameObject>());
        }

        ghostsByContext[toGhost].Add(ghost);

        return ghost;
    }

    public void RemoveGhosts(GameObject context) {
        if (!ghostsByContext.ContainsKey(context)) return;

        var ghostsToRemove = ghostsByContext[context];

        foreach (var ghost in ghostsByContext[context]) {
            Destroy(ghost);
        }

        ghostsByContext.Remove(context);
    }

    public void SetEnemyGhostsVisible(bool visible) {
        ghostsByContext
            .Where(kv => kv.Key.GetComponent<DieManager>().isEnemy)
            .ToList()
            .ForEach(kv => {
                foreach (var ghost in kv.Value) {
                    ghost.SetActive(visible);
                }
            });
    }

    public void SetGhostsVisible(GameObject context, bool visible) {
        if (!ghostsByContext.ContainsKey(context)) return;

        foreach (var ghosts in ghostsByContext[context]) {
            ghosts.SetActive(visible);
        }
    }

    public void Clear() {
        foreach (var ghosts in ghostsByContext) {
            foreach (var ghost in ghosts.Value) {
                Destroy(ghost);
            }
        }

        ghostsByContext.Clear();
    }
}
