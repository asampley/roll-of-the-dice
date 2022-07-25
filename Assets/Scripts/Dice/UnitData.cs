using UnityEngine;

public class UnitData : ScriptableObject
{
    [Header("General")]
    public string code;
    public string unitName;
    public GameObject prefab;
    public Material allyMaterial;
    public Material enemyMaterial;
    public int maxMoves;
    public MovementPattern movementPattern;


    [Header("Rotation")]
    public Face[] faces;
    public Vector3 rotationOffset;
}
