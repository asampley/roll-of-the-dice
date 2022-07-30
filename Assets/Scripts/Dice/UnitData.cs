using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData", order = 4)]
public class UnitData : ScriptableObject
{
    [Header("General")]
    public string unitName;
    public DiceClass unitClass;

    [Header("References")]
    public GameObject prefab;
    public Material alliedMaterial;
    public Material alliedGhostMaterial;
    public Material enemyMaterial;
    public Material enemyGhostMaterial;

    [Header("Movement")]
    public int maxMoves;
    public MovementPattern movementPattern;

    [Header("Rotation")]
    public DiceState[] faces;
}
