using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieTranslator : MonoBehaviour {
    private Vector3 startPos;
    private float startTime;
    private List<Vector3> targets = new List<Vector3>();

    public event Action ReachTarget;

    private bool _collapse;
    public bool Collapse {
        get { return _collapse; }
        set { _collapse = value; if (_collapse) CollapseTargets(); }
    }

    void Start() {
        this.startPos = this.transform.parent.localPosition;
    }

    public void Translate(Vector3 translation) {
        if (targets.Count == 0) {
            this.startTime = Time.fixedTime;
            this.startPos = this.transform.parent.localPosition;
        }

        targets.Add(FinalTarget() + translation);
    }

    Vector3 FinalTarget() {
        return targets.Count > 0 ? targets[targets.Count - 1] : startPos;
    }

    void CollapseTargets() {
        if (this.targets.Count > 0) {
            this.targets.RemoveRange(0, this.targets.Count - 1);
        }
    }

    public void TranslateNow() {
        this.startPos = FinalTarget();
        this.targets.Clear();
        this.transform.parent.localPosition = this.startPos;
    }

    public void Update() {
        if (targets.Count > 0) {
            if (Time.fixedTime < startTime + Globals.MOVEMENT_TIME) {
                this.transform.parent.localPosition = Vector3.Lerp(startPos, targets[0], (Time.fixedTime - startTime) / Globals.MOVEMENT_TIME);
            } else {
                this.transform.parent.localPosition = targets[0];

                this.startPos = targets[0];
                this.startTime = Time.fixedTime;

                targets.RemoveAt(0);

                ReachTarget?.Invoke();
            }
        }
    }
}
