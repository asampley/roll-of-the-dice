using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TextureCoords {
    public float xStart;
    public float xStop;
    public float yStart;
    public float yStop;

    public TextureCoords(float xStart, float xStop, float yStart, float yStop) {
        this.xStart = xStart;
        this.xStop = xStop;
        this.yStart = yStart;
        this.yStop = yStop;
    }
}

public class DieTexturer : MonoBehaviour {
    private const int WIDTH = 6;

    public int frontIndex;
    public int backIndex;
    public int leftIndex;
    public int rightIndex;
    public int topIndex;
    public int bottomIndex;

    void Start() {
        var mesh = this.GetComponent<MeshFilter>().mesh;

        var meshUvs = mesh.uv;

        var uvs = new Vector2[mesh.vertices.Length];

        var front = GetCoords(frontIndex);
        var back = GetCoords(backIndex);
        var left = GetCoords(leftIndex);
        var right = GetCoords(rightIndex);
        var top = GetCoords(topIndex);
        var bottom = GetCoords(bottomIndex);

        for (int triangle = 0; triangle < mesh.triangles.Length / 3; ++triangle) {
            var t = new int[] {
                mesh.triangles[0 + triangle * 3],
                mesh.triangles[1 + triangle * 3],
                mesh.triangles[2 + triangle * 3],
            };

            for (int j = 0; j < 3; ++j) {
                TextureCoords coords;
                {
                    var n = mesh.normals[t[j]];

                    Debug.Log(n);

                    if (Mathf.Abs(n.x) > 0.5) {
                        if (mesh.vertices[t[0]].x > 0) {
                            Debug.Log("Right");
                            coords = right;
                        } else {
                            Debug.Log("Left");
                            coords = left;
                        }
                    } else if (Mathf.Abs(n.y) > 0.5) {
                        if (mesh.vertices[t[0]].y > 0) {
                            Debug.Log("Top");
                            coords = top;
                        } else {
                            Debug.Log("Bottom");
                            coords = bottom;
                        }
                    } else {
                        if (mesh.vertices[t[0]].z > 0) {
                            Debug.Log("Front");
                            coords = front;
                        } else {
                            Debug.Log("Back");
                            coords = back;
                        }
                    }
                }

                var vec = new Vector2();

                vec.x = meshUvs[t[j]].x < 0.5 ? coords.xStart : coords.xStop;
                vec.y = meshUvs[t[j]].y < 0.5 ? coords.yStart : coords.yStop;

                uvs[t[j]] = vec;
            }
        }

        mesh.SetUVs(0, uvs);
    }

    static TextureCoords GetCoords(int index) {
        return new TextureCoords((float)index / WIDTH, (float)(index + 1) / WIDTH, 0, 1);
    }
}
