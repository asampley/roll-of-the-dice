using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private static readonly uint WIDTH = Enum.GetValues(typeof(DiceState)).Cast<uint>().Max() + 1;

    public DiceState frontIndex;
    public DiceState backIndex;
    public DiceState leftIndex;
    public DiceState rightIndex;
    public DiceState topIndex;
    public DiceState bottomIndex;

    void Start()
    {
        UpdateMesh();
    }

    static TextureCoords GetCoords(DiceState index) {
        return new TextureCoords((float)index / WIDTH, (float)(index + 1) / WIDTH, 0, 1);
    }

    public void UpdateMesh()
    {
        var mesh = this.GetComponent<MeshFilter>().mesh;

        var meshUvs = mesh.uv;

        var uvs = new Vector2[mesh.vertices.Length];

        var front = GetCoords(frontIndex);
        var back = GetCoords(backIndex);
        var left = GetCoords(leftIndex);
        var right = GetCoords(rightIndex);
        var top = GetCoords(topIndex);
        var bottom = GetCoords(bottomIndex);

        for (int triangle = 0; triangle < mesh.triangles.Length / 3; ++triangle)
        {
            var t = new int[] {
                mesh.triangles[0 + triangle * 3],
                mesh.triangles[1 + triangle * 3],
                mesh.triangles[2 + triangle * 3],
            };

            for (int j = 0; j < 3; ++j)
            {
                float x, y;
                {
                    TextureCoords coords;
                    var n = mesh.normals[t[j]];
                    var v = mesh.vertices[t[j]];

                    if (Mathf.Abs(n.x) > 0.5)
                    {
                        if (v.x > 0)
                        {
                            coords = right;
                        }
                        else
                        {
                            coords = left;
                        }

                        x = v.z > 0 ? coords.xStop : coords.xStart;
                        y = v.y > 0 ? coords.yStop : coords.yStart;
                    }
                    else if (Mathf.Abs(n.y) > 0.5)
                    {
                        if (v.y > 0)
                        {
                            coords = top;
                        }
                        else
                        {
                            coords = bottom;
                        }

                        x = v.x > 0 ? coords.xStop : coords.xStart;
                        y = v.z > 0 ? coords.yStop : coords.yStart;
                    }
                    else
                    {
                        if (v.z > 0)
                        {
                            coords = front;
                        }
                        else
                        {
                            coords = back;
                        }

                        x = v.x > 0 ? coords.xStop : coords.xStart;
                        y = v.y > 0 ? coords.yStop : coords.yStart;
                    }
                }

                uvs[t[j]] = new Vector2(x, y);
            }
        }

        mesh.SetUVs(0, uvs);
    }
}
