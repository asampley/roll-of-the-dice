using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Face {
    public DiceState state;
    public Vector3 position;

    public Face(Face original) {
        this.state = original.state;
        this.position = original.position;
    }
}

[RequireComponent(typeof(MeshFilter))]
public class DieTexturer : MonoBehaviour {
    public static readonly Vector2 uvScale = new(1f / Globals.DICE_STATES, 1f);

    [SerializeField]
    [HideInInspector]
    // field which tracks the original mesh, and should only be automatically set
    private Mesh originalMesh;

    [SerializeField]
    private DiceState[] _faces;
    public DiceState[] Faces
    {
        get { return _faces; }
        set { _faces = value; }
    }

    [SerializeField]
    private Axes axes;
    public Axes Axes {
        get { return axes; }
        set { axes = value; }
    }

    public void Initialize()
    {
        if (originalMesh == null)
        {
            originalMesh = this.GetComponent<MeshFilter>().sharedMesh;

            // by calling retexture only if the original mesh is not set
            // we only create a new mesh when the original mesh was not
            // inherited
            UpdateMesh();
        }
    }

    public void UpdateMesh()
    {
        Mesh mesh;
        if (Application.isPlaying)
        {
            mesh = this.GetComponent<MeshFilter>().mesh;
        }
        else
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            Mesh meshCopy = Mesh.Instantiate(mf.sharedMesh);
            mesh = mf.mesh = meshCopy;
        }


        var originalUvs = originalMesh.uv;
        var meshTriangles = mesh.triangles;
        var meshNormals = mesh.normals;
        var meshVertices = mesh.vertices;

        Logging.LogNotification(("Vertices: " + meshVertices.StrJoin()).ToString(), LogType.UNIT_SPAWN);

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

                uvs[t[j]] = Vector2.Scale(uv, uvScale) + Offset(face);
            }
        }

        Logging.LogNotification(("Retexture " + this.name + ": uvs = " + uvs.StrJoin()).ToString(), LogType.UNIT_SPAWN);

        mesh.SetUVs(0, uvs);
    }

    public int ClosestFaceIndex(Vector3 position)
    {
        int face_index = -1;
        float dist = float.PositiveInfinity;

        for (int i = 0; i < axes.Faces.Length; ++i) {
            var d = Vector3.Distance(axes.Faces[i], position);

            if (d < dist) {
                face_index = i;
                dist = d;
            }
        }

        if (face_index == -1)
        {
            Debug.LogError("No face found for texturing, make sure faces is not empty");
            throw new System.IndexOutOfRangeException();
        } else {
            return face_index;
        }
    }

    public DiceState ClosestFace(Vector3 position)
    {
        return Faces[ClosestFaceIndex(position)];
    }

    static Vector2 Offset(DiceState index)
    {
        return new Vector2((float)((uint)index) / Globals.DICE_STATES, 0);
    }
}
