using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Axes", menuName = "Scriptable Objects/Axes")]
public class Axes : ScriptableObject {
    public Vector3 XAxis;
    public Vector3 YAxis;
    public Vector3 ZAxis;

    public float FaceRotationAngle;
    public int FaceEdges;
}
