using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieRotator : MonoBehaviour {
    private Quaternion startRot;
    private float startTime;
    private List<Quaternion> targets = new List<Quaternion>();

    [SerializeField]
    private Vector3 _offsetRotation;
    private Quaternion offsetRotation;

    public Axes axes;

    private bool _collapse;
    public bool Collapse {
        get { return _collapse; }
        set { _collapse = value; if (_collapse) CollapseTargets(); }
    }

    void Awake() {
        this.offsetRotation = Quaternion.Euler(_offsetRotation);
        this.startRot = this.transform.localRotation;
    }

    Quaternion FinalTarget() {
        return targets.Count > 0 ? targets[targets.Count - 1] : startRot;
    }

    void AddTarget(Quaternion target) {
        if (Collapse && targets.Count == 1) {
            targets[0] = target;
        } else {
            targets.Add(target);
        }
    }

    void CollapseTargets() {
        if (this.targets.Count > 0) {
            this.targets.RemoveRange(0, this.targets.Count - 1);
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
            AddTarget(rotation * FinalTarget());
        }
    }

    // rotate around an axis that is relative to the mesh original orientation
    void RotateAngleAxis(float angle, Vector3 axis, int count) {
        Rotate(Quaternion.AngleAxis(angle, offsetRotation * axis), count);
    }

    public void RotateX(int count) {
        RotateAngleAxis(axes.FaceRotationAngle, axes.XAxis, count);
    }

    public void RotateY(int count) {
        RotateAngleAxis(axes.FaceRotationAngle, axes.YAxis, count);
    }

    public void RotateZ(int count) {
        RotateAngleAxis(360f / axes.FaceEdges, axes.ZAxis, count);
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

    public DiceState GetUpFace() {
        var topFaceDir = Vector3Int.RoundToInt(Quaternion.Inverse(FinalTarget()) * Vector3.up);

        return GetComponent<DieTexturer>().ClosestFace(topFaceDir).state;
    }

    public DiceState GetDownFace()
    {
        var bottomFaceDir = Vector3Int.RoundToInt(Quaternion.Inverse(FinalTarget()) * -Vector3.up);

        return GetComponent<DieTexturer>().ClosestFace(bottomFaceDir).state;
    }

    public void SetDownFace(DiceState newDiceState)
    {
        var bottomFaceDir = Vector3Int.RoundToInt(Quaternion.Inverse(FinalTarget()) * -Vector3.up);

        var texturer = GetComponent<DieTexturer>();
        var face = texturer.ClosestFace(bottomFaceDir);
        face.state = newDiceState;
        texturer.UpdateMesh();
    }
}
