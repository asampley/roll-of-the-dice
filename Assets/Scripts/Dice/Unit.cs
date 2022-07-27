using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    protected UnitManager _manager;
    protected UnitData _data;
    public UnitData Data
    { get => _data; }
    protected Transform _transform;
    public Transform Transform
    { get =>_transform; }
    protected string _uid;
    public string Uid
    {
        get => _uid;
        set { _uid = value; }
    }

    [Header("General")]
    protected string _unitName;
    public string UnitName
    { get => _unitName; }
    protected DiceClass _unitClass;
    public DiceClass UnitClass
    { get => _unitClass; }
    protected bool _isEnemy;
    public bool IsEnemy
    { get => _isEnemy; }


    [Header("References")]
    protected GameObject _prefab;
    protected Material _allyMaterial;
    protected Material _allyGhostMaterial;
    protected Material _enemyMaterial;
    protected Material _enemyGhostMaterial;

    [Header("Movement")]
    protected int _maxMoves;
    protected MovementPattern _movementPattern;

    [Header("Rotation")]
    protected Face[] _faces;
    public Face[] Faces
    {
        get { return _faces; }
        set { _faces = value; }
    }
    protected Vector3 _rotationOffset;

    public static List<Unit> DICE_LIST;

    public Unit(UnitData data, bool isEnemy, DiceOrientation orientation)
    {
        _data = data;
        GameObject g = GameObject.Instantiate(data.prefab) as GameObject;
        _transform = g.transform;

        _uid = System.Guid.NewGuid().ToString();
        _transform.name = data.unitClass + (isEnemy ? " Enemy " : " Player ") + _uid;
        _transform.parent = GameManager.Instance.diceParent.transform;

        _transform.rotation = Quaternion.identity;

        // Setup Manager
        _manager = g.GetComponent<UnitManager>();

        _manager.Unit = this;
        _manager.UnitName = _data.unitName;
        _manager.IsEnemy = isEnemy;

        _manager.alliedMaterial = _data.alliedMaterial;
        _manager.alliedGhostMaterial = _data.alliedGhostMaterial;
        _manager.enemyMaterial = _data.enemyMaterial;
        _manager.enemyGhostMaterial = _data.enemyGhostMaterial;

        _manager.MaxMoves = _data.maxMoves;
        _manager.MovementPattern = _data.movementPattern;

        // Setup faces
        _manager.DieTexturer.Faces = new Face[_data.faces.Length];
        for (int n = 0; n < _data.faces.Length; n++)
            _manager.DieTexturer.Faces[n] = _data.faces[n];
        _manager.DieTexturer.Initialize();

        _manager.Initialize(orientation);


        _manager.DieRotator.OffsetRotation = _data.offsetRotation;

        // Add to dice list for save data
        if (DICE_LIST == null)
            DICE_LIST = new List<Unit>();
        DICE_LIST.Add(this);
    }

    public void SetPosition(Vector2Int pos)
    {
        var placedOnTile = MapManager.Instance.GetTileAtPos(pos);
        Debug.Log(placedOnTile.gridLocation);
        if (placedOnTile == null)
        {
            Debug.LogError("Dice spawning off map.");
            return;
        }

        OverlayTile overlayTileManager = placedOnTile.gameObject.GetComponent<OverlayTile>();

        overlayTileManager.MoveDiceToTile(_manager);
        _transform.position = placedOnTile.transform.position;
    }

    public Vector2Int GetPosition()
    {
        Vector2Int pos = (Vector2Int)_manager.parentTile.gridLocation;

        return pos;
    }
}
