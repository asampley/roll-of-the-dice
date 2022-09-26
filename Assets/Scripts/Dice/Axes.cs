using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HashQuat {
    private Quaternion quaternion;

    public HashQuat(Quaternion quaternion) {
        this.quaternion = quaternion;
    }

    override public int GetHashCode() {
        int hash = 23;
        hash = hash * 31 + FloatHash(quaternion.x);
        hash = hash * 31 + FloatHash(quaternion.y);
        hash = hash * 31 + FloatHash(quaternion.z);
        hash = hash * 31 + FloatHash(quaternion.w);
        return hash;
    }

    private static int FloatHash(float f) => Math.Abs(FloatRound(f)).GetHashCode();

    private static int FloatRound(float f) => Mathf.RoundToInt(f * 1000f);

    public static bool operator==(HashQuat a, HashQuat b) {
        return (
            FloatRound(a.quaternion.x) == FloatRound(b.quaternion.x)
            && FloatRound(a.quaternion.y) == FloatRound(b.quaternion.y)
            && FloatRound(a.quaternion.z) == FloatRound(b.quaternion.z)
            && FloatRound(a.quaternion.w) == FloatRound(b.quaternion.w)
        ) || (
            FloatRound(a.quaternion.x) == -FloatRound(b.quaternion.x)
            && FloatRound(a.quaternion.y) == -FloatRound(b.quaternion.y)
            && FloatRound(a.quaternion.z) == -FloatRound(b.quaternion.z)
            && FloatRound(a.quaternion.w) == -FloatRound(b.quaternion.w)
        );
    }

    public static bool operator!=(HashQuat a, HashQuat b) => !(a == b);

    override public bool Equals(object o) {
        if (o != null && o is HashQuat quat) {
            return this == quat;
        } else {
            return false;
        }
    }

    override public string ToString() => quaternion.ToString();
}

[CreateAssetMenu(fileName = "Axes", menuName = "Scriptable Objects/Axes")]
public class Axes : ScriptableObject {
    public Vector3 XAxis;
    public Vector3 YAxis;
    public Vector3 ZAxis;

    public float FaceRotationAngle;
    public int FaceEdges;

    public bool opposingFaces = true;

    public Vector3[] Faces { get => _rotations.Value.Item3; }
    public Dictionary<HashQuat, DiceOrientation> QuaternionToOrientation { get => _rotations.Value.Item1; }
    public Dictionary<DiceOrientation, Quaternion> OrientationToQuaternion { get => _rotations.Value.Item2; }
    private Lazy<(
        Dictionary<HashQuat, DiceOrientation>,
        Dictionary<DiceOrientation, Quaternion>,
        Vector3[]
    )> _rotations = new(CreateRotations);

    // I promise to do all the math right and make opposite faces indices added
    // together to be the number of faces minus one.
    private static (
        Dictionary<HashQuat, DiceOrientation>,
        Dictionary<DiceOrientation, Quaternion>,
        Vector3[]
    ) CreateRotations() {
        Dictionary<HashQuat, DiceOrientation> dict1 = new();
        Dictionary<DiceOrientation, Quaternion> dict2 = new();
        Vector3[] faces = new Vector3[6];

        float sqrt2on2 = Mathf.Sqrt(2f) / 2f;

        Quaternion[] quats = new Quaternion[] {
            new Quaternion(0f, 0f, 0f, 1f),
            new Quaternion(0f, sqrt2on2, 0f, sqrt2on2),
            new Quaternion(sqrt2on2, 0f, 0f, sqrt2on2),
            new Quaternion(sqrt2on2, 0f, 0f, -sqrt2on2),
            new Quaternion(0f, sqrt2on2, 0f, -sqrt2on2),
            new Quaternion(1f, 0f, 0f, 0f),
        };

        for (int q = 0; q < quats.Length; ++q) {
            faces[q] = quats[q] * Vector3.forward;

            for (int i = 0; i < 4; ++i) {
                var offset = Quaternion.AngleAxis(90.0f * i, Vector3.forward);

                var quat = offset * quats[q];
                dict1.Add(new HashQuat(quat), new DiceOrientation(q, i));
                dict2.Add(new DiceOrientation(q, i), quat);
            }
        }

        return (dict1, dict2, faces);
    }

    public int ClosestFaceNumber(Vector3 position) {
        int face_index = 0;
        float dist = Vector3.Distance(Faces[0], position);

        for (int i = 1; i < Faces.Length; ++i) {
            var d = Vector3.Distance(Faces[i], position);

            if (d < dist) {
                face_index = i;
                dist = d;
            }
        }

        return face_index;
    }

    public int OpposingFace(int faceNumber) {
        return Faces.Length - faceNumber - 1;
    }
}
