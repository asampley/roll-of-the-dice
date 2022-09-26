using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DieRotator : MonoBehaviour {
    private Quaternion startRot;
    private float startTime;
    private readonly List<Quaternion> targets = new();

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

    private DiceOrientation _currentOrientation;


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
        UpdateOrientation();
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
        UpdateOrientation();
    }

    public void RotateZ(int count)
    {
        RotateAngleAxis(360f / axes.FaceEdges, axes.ZAxis, count);
        UpdateOrientation();
    }

    public Quaternion RotateNow()
    {
        UpdateOrientation();
        this.startRot = FinalTarget();
        this.targets.Clear();
        this.transform.localRotation = this.startRot;

        return this.transform.localRotation;
    }

    public void SetRotation(Quaternion target)
    {
        targets.Add(target);
        UpdateOrientation();
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
        return GetComponent<DieTexturer>().Faces[GetOrientation().FaceNumber];
    }

    public DiceState GetDownFace()
    {
        return GetComponent<DieTexturer>().Faces[axes.OpposingFace(GetOrientation().FaceNumber)];
    }

    public void SetDownFace(DiceState newDiceState)
    {
        var texturer = GetComponent<DieTexturer>();
        texturer.Faces[axes.OpposingFace(GetOrientation().FaceNumber)] = newDiceState;
        texturer.UpdateMesh();
    }

    public void SetOrientation(DiceOrientation orientation)
    {
        _currentOrientation = orientation;

        if (axes.OrientationToQuaternion.TryGetValue(_currentOrientation, out Quaternion quat))
        {
            this.SetRotation(_qOffsetRotation * quat);
            this.RotateNow();
        }
        else
        {
            Debug.LogError(transform.parent.name + " " + _currentOrientation);
        }
    }

    public void SetOrientation(Quaternion quaternion)
    {
        SetRotation(quaternion);
    }

    public void UpdateOrientation()
    {
        _currentOrientation = GetOrientation();
    }

    public DiceOrientation GetOrientation()
    {
        Quaternion rot = Quaternion.Inverse(_qOffsetRotation) * this.FinalTarget();

        if (axes.QuaternionToOrientation.TryGetValue(new HashQuat(rot), out DiceOrientation orientation))
        {
            return orientation;
        }
        else
        {
            Debug.LogError(transform.parent.name + " " + this.FinalTarget().ToString("f7") + " -> " + rot);
            return DiceOrientation.ZERO;
        }
    }
}
