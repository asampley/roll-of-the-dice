using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData", order = 4)]
public class UnitData : ScriptableObject
{
    [Header("General")]
    public string unitName;
    public DiceClass unitClass;

    [Header("References")]
    public GameObject prefab;
    public Material diceMaterial;
    public Material ghostMaterial;

    [Header("Appearance")]
    public Color32 allyColor;
    public Color32 enemyColor;

    [Header("Movement")]
    public int maxMoves;
    public MovementPattern movementPattern;
    public MovementStrategy movementStrategy;

    [Header("Rotation")]
    public DiceState[] faces;
}
