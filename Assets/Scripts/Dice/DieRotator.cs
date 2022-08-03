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
    public Vector3 OffsetRotation
    {
        get { return _offsetRotation; }
        set { _offsetRotation = value; }
    }

    private Quaternion _qOffsetRotation;
    public Axes axes;

    private bool _collapse;
    public bool Collapse
    {
        get { return _collapse; }
        set { _collapse = value; if (_collapse) CollapseTargets(); }
    }

    void Awake()
    {
        this._qOffsetRotation = Quaternion.Euler(_offsetRotation);
        this.startRot = this.transform.localRotation;
    }

    public Quaternion FinalTarget()
    {
        return targets.Count > 0 ? targets[targets.Count - 1] : startRot;
    }

    void AddTarget(Quaternion target)
    {
        if (Collapse && targets.Count == 1) {
            targets[0] = target;
        } else {
            targets.Add(target);
        }
    }

    void CollapseTargets()
    {
        if (this.targets.Count > 0) {
            this.targets.RemoveRange(0, this.targets.Count - 1);
        }
    }

    void Rotate(Quaternion rotation, int count)
    {
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
    void RotateAngleAxis(float angle, Vector3 axis, int count)
    {
        Rotate(RotationAngleAxis(angle, axis), count);
    }

    Quaternion RotationAngleAxis(float angle, Vector3 axis)
    {
        return Quaternion.AngleAxis(angle, _qOffsetRotation * axis);
    }

    // Computes an axis based on the tile delta.
    // This should only be called for single space moves.
    public void RotateTileDelta(Vector2Int delta, int count = 1)
    {
        var axis = delta.x * axes.XAxis + delta.y * axes.YAxis;

        RotateAngleAxis(axes.FaceRotationAngle, axis, count);
    }

    // Computes an axis based on the tile delta.
    // This collapses multiple rotations into one timestep.
    public void RotateTileDeltas(IEnumerable<Vector2Int> deltas)
    {
        Quaternion rotation = Quaternion.identity;

        foreach (var delta in deltas) {
            var axis = delta.x * axes.XAxis + delta.y * axes.YAxis;
            rotation = RotationAngleAxis(axes.FaceRotationAngle, axis) * rotation;
        }

        Rotate(rotation, 1);
    }

    public void RotateZ(int count)
    {
        RotateAngleAxis(360f / axes.FaceEdges, axes.ZAxis, count);
    }

    public Quaternion RotateNow()
    {
        this.startRot = FinalTarget();
        this.targets.Clear();
        this.transform.localRotation = this.startRot;
        return this.transform.localRotation;
    }

    public void SetRotation(Quaternion target)
    {
        targets.Add(target);
        this.startRot = FinalTarget();
        this.targets.Clear();
        this.transform.localRotation = this.startRot;
    }

    public void Update()
    {
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

    public DiceState GetUpFace()
    {
        var topFaceDir = Vector3Int.RoundToInt(Quaternion.Inverse(FinalTarget()) * Vector3.up);

        return GetComponent<DieTexturer>().ClosestFace(topFaceDir);
    }

    public DiceState GetDownFace()
    {
        var bottomFaceDir = Vector3Int.RoundToInt(Quaternion.Inverse(FinalTarget()) * -Vector3.up);

        return GetComponent<DieTexturer>().ClosestFace(bottomFaceDir);
    }

    public void SetDownFace(DiceState newDiceState)
    {
        var bottomFaceDir = Vector3Int.RoundToInt(Quaternion.Inverse(FinalTarget()) * -Vector3.up);

        var texturer = GetComponent<DieTexturer>();
        var face = texturer.ClosestFaceIndex(bottomFaceDir);
        texturer.Faces[face] = newDiceState;
        texturer.UpdateMesh();
    }
}
