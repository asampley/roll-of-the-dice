using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostManager : MonoBehaviour {
    private static GhostManager _instance;
    public static GhostManager Instance { get { return _instance; } }

    private readonly Dictionary<GameObject, List<GameObject>> ghostsByContext = new();
    private readonly Dictionary<GameObject, GameObject> arrowsByContext = new();

    public GameObject ghostContainer;
    public GameObject arrowPrefab;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    public void SetupGhostEffects(GameObject ghostObject, Vector2Int next, List<Vector3> trans, List<Vector2Int> deltas)
    {
        GhostManager.Instance.PushArrow(ghostObject, next);
        var ghost = GhostManager.Instance.CreateGhost(ghostObject, null, null);

        var translator = ghost.GetComponentInChildren<DieTranslator>();
        foreach (var t in trans)
            translator.Translate(t);

        var rotator = ghost.GetComponentInChildren<DieRotator>();
        foreach (var delta in deltas)
            rotator.RotateTileDelta(delta);
    }

    public GameObject CreateGhost(GameObject toGhost, Vector3? translation, Vector2Int? tileDelta, int rotationCount = 1) {
        var dieManager = toGhost.GetComponent<UnitManager>();
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

    public void PushArrow(GameObject context, Vector2Int next) {
        if (!arrowsByContext.ContainsKey(context)) {
            var a = Instantiate(arrowPrefab, this.transform);
            arrowsByContext.Add(context, a);
        }

        // add position
        var arrow = arrowsByContext[context].GetComponentInChildren<LineRenderer>();
        ++arrow.positionCount;
        var positions = new Vector3[arrow.positionCount];
        arrow.GetPositions(positions);
        positions[^1] = MapManager.Instance.TileToWorldSpace2(next);
        arrow.SetPositions(positions);

        // add arrow head
        //var anim = arrow.widthCurve;

        //var frame = anim[1];
        //frame.time = 1f - 0.25f / arrow.positionCount;
        //anim.MoveKey(2, frame);
    }

    public void RemoveGhosts(GameObject context) {
        if (!ghostsByContext.ContainsKey(context)) return;

        var ghostsToRemove = ghostsByContext[context];
        foreach (var ghost in ghostsByContext[context]) {
            Destroy(ghost);
        }

        ghostsByContext.Remove(context);
    }

    public void RemoveArrow(GameObject context) {
        if (!arrowsByContext.ContainsKey(context)) return;

        Destroy(arrowsByContext[context]);

        arrowsByContext.Remove(context);
    }

    public void SetEnemyGhostsVisible(bool visible) {
        ghostsByContext
            .Where(kv => kv.Key.GetComponent<UnitManager>().IsEnemy)
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
