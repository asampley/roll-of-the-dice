using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieRotator : MonoBehaviour {
    private Quaternion startRot;
    private float startTime;
    private List<Quaternion> targets = new List<Quaternion>();

    private static Quaternion originalOffset;
    private static Vector3 localUp;
    private static Vector3 localForward;
    private static Vector3 localRight;

    private bool _collapse;
    public bool Collapse {
        get { return _collapse; }
        set { _collapse = value; if (_collapse) CollapseTargets(); }
    }

    void Awake() {
        this.startRot = this.transform.localRotation;

        if (localUp == Vector3.zero) {
            originalOffset = this.startRot;
            localUp = this.startRot * Vector3.up;
            localForward = this.startRot * Vector3.forward;
            localRight = this.startRot * Vector3.right;
        }
    }

    void Rotate(Quaternion rotation, int count) {
        if (count < 0) {
            rotation = Quaternion.Inverse(rotation);
        }

        if (targets.Count == 0) {
            this.startTime = Time.fixedTime;
            this.startRot = this.transform.localRotation;
        }

        for (int i = 0; i < Math.Abs(count); ++i) {
            targets.Add(FinalTarget() * rotation);
            //target *= rotation;
        }

        //this.current = this.transform.localRotation;
        //this.startTime = Time.fixedTime;
    }

    Quaternion FinalTarget() {
        return targets.Count > 0 ? targets[targets.Count - 1] : startRot;
    }

    void CollapseTargets() {
        if (this.targets.Count > 0) {
            this.targets.RemoveRange(0, this.targets.Count - 1);
        }
    }

    public void RotateX(int count) {
        var targetInverse = Quaternion.Inverse(FinalTarget());
        var rotation = Quaternion.FromToRotation(targetInverse * localForward, targetInverse * localRight);

        Rotate(rotation, count);
    }

    public void RotateY(int count) {
        var targetInverse = Quaternion.Inverse(FinalTarget());
        var rotation = Quaternion.FromToRotation(targetInverse * localForward, targetInverse * localUp);

        Rotate(rotation, count);
    }

    public void RotateZ(int count)
    {
        var targetInverse = Quaternion.Inverse(FinalTarget());
        var rotation = Quaternion.FromToRotation(targetInverse * localRight, targetInverse * localUp);

        Rotate(rotation, count);
    }

    public void RotateNow() {
        this.startRot = FinalTarget();
        this.targets.Clear();
        this.transform.localRotation = this.startRot;
    }

    public void Update() {
        if (targets.Count > 0) {
            if (Time.fixedTime < startTime + Globals.MOVEMENT_TIME) {
                this.transform.localRotation = Quaternion.Slerp(startRot, targets[0], (Time.fixedTime - startTime) / Globals.MOVEMENT_TIME);
            } else {
                this.transform.localRotation = targets[0];

                this.startRot = targets[0];
                this.startTime = Time.fixedTime;

                targets.RemoveAt(0);
            }
        }
    }

    public DiceState UpFace() {
        var topFaceDir = Vector3Int.RoundToInt(Quaternion.Inverse(originalOffset * FinalTarget()) * localUp);

        var texturer = GetComponent<DieTexturer>();
        if (topFaceDir.x == 1) {
            return texturer.rightIndex;
        } else if (topFaceDir.x == -1) {
            return texturer.leftIndex;
        } else if (topFaceDir.y == 1) {
            return texturer.topIndex;
        } else if (topFaceDir.y == -1) {
            return texturer.bottomIndex;
        } else if (topFaceDir.z == 1) {
            return texturer.frontIndex;
        } else {
            return texturer.backIndex;
        }
    }
}
