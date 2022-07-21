using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct Face {
    public DiceState state;
    public Vector3 position;
}

[RequireComponent(typeof(MeshFilter))]
public class DieTexturer : MonoBehaviour {
    public static readonly Vector2 uvScale = new Vector2(1f / Globals.DICE_STATES, 1f);

    [SerializeField]
    [HideInInspector]
    // field which tracks the original mesh, and should only be automatically set
    private Mesh originalMesh;

    [SerializeField]
    private Face[] faces;

    void Start() {
        if (originalMesh == null) {
            originalMesh = this.GetComponent<MeshFilter>().sharedMesh;

            // by calling retexture only if the original mesh is not set
            // we only create a new mesh when the original mesh was not
            // inherited
            UpdateMesh();
        }
    }

    public void UpdateMesh()
    {
        var mesh = this.GetComponent<MeshFilter>().mesh;

        var originalUvs = originalMesh.uv;
        var meshTriangles = mesh.triangles;
        var meshNormals = mesh.normals;
        var meshVertices = mesh.vertices;

        Debug.Log("Vertices: " + Utilities.EnumerableString(meshVertices));

        var uvs = new Vector2[mesh.vertices.Length];

        for (int triangle = 0; triangle < mesh.triangles.Length / 3; ++triangle)
        {
            var t = new int[] {
                meshTriangles[0 + triangle * 3],
                meshTriangles[1 + triangle * 3],
                meshTriangles[2 + triangle * 3],
            };

            var v = new Vector3[] {
                meshVertices[t[0]],
                meshVertices[t[1]],
                meshVertices[t[2]],
            };

            var face = ClosestFace((v[0] + v[1] + v[2]) / 3f);

            for (int j = 0; j < 3; ++j) {
                var n = meshNormals[t[j]];
                var uv = originalUvs[t[j]];

                uvs[t[j]] = Vector2.Scale(uv, uvScale) + Offset(face.state);
            }
        }

        Debug.Log("Retexture " + this.name + ": uvs = " + Utilities.EnumerableString(uvs));

        mesh.SetUVs(0, uvs);
    }

    public Face ClosestFace(Vector3 position) {
        Face? face = null;
        float dist = float.PositiveInfinity;

        foreach (var f in this.faces) {
            var d = Vector3.Distance(f.position, position);

            if (d < dist) {
                face = f;
                dist = d;
            }
        }

        if (face == null) {
            Debug.LogError("No face found for texturing, make sure faces is not empty");
            throw new System.IndexOutOfRangeException();
        } else {
            return face.Value;
        }
    }

    static Vector2 Offset(DiceState index) {
        return new Vector2((float)((uint)index) / Globals.DICE_STATES, 0);
    }
}
