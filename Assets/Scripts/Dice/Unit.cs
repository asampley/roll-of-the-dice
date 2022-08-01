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

    private bool _loadFromSave = false;
    public bool LoadFromSave
    {
        get { return _loadFromSave; }
        set { _loadFromSave = value; }
    }

    [Header("General")]
    protected string _unitName;
    public string UnitName { get => _unitName; }
    public DiceClass UnitClass { get => _data.unitClass; }
    public bool IsEnemy { get => _manager.IsEnemy; }


    [Header("References")]
    protected GameObject _prefab;
    protected Material _allyMaterial;
    protected Material _allyGhostMaterial;
    protected Material _enemyMaterial;
    protected Material _enemyGhostMaterial;

    [Header("Movement")]
    protected int _maxMoves;
    protected MovementPattern _movementPattern;
    public int MovesRemainging { get => _manager.movesAvailable; }
    public List<Vector2Int> Path { get => _manager.path; }

    [Header("Rotation")]
    protected Axes _axes;
    public Axes Axes {
        get { return _axes; }
        set { _axes = value; }
    }

    protected DiceState[] _faces;
    public DiceState[] Faces
    {
        get { return _faces; }
        set { _faces = value; }
    }
    protected Vector3 _orientation;
    public Vector3 orientation
    {
        get { return _orientation; }
        set { _orientation = value; }
    }
    protected Vector3 _rotationOffset;

    public static List<Unit> DICE_LIST;


    public Unit(UnitData data, bool isEnemy, Vector3 diceOrientation, Vector2Int position, int moves, bool fromSave = false, List<Vector2Int> path = null) : this(data, isEnemy, new DiceOrientationData(), fromSave)
    {
        SetOrientation(diceOrientation);
        SetPosition(position);
        _manager.movesAvailable = moves;
        if (path != null && isEnemy)
        {
            _manager.path = path;
            _manager.MapPath();
        }
            
    }
    public Unit(UnitData data, bool isEnemy, DiceOrientationData startOrientation, bool fromSave = false)
    {
        _loadFromSave = fromSave;
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

        _manager.MaxMoves = _data.maxMoves;
        _manager.MovementPattern = _data.movementPattern;

        // Setup faces
        _manager.DieTexturer.Faces = new DiceState[_data.faces.Length];
        for (int n = 0; n < _data.faces.Length; n++)
            _manager.DieTexturer.Faces[n] = _data.faces[n];
        _manager.DieTexturer.Initialize();
        _faces = _manager.DieTexturer.Faces;
        _manager.Initialize(startOrientation);

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
        _transform.position = MapManager.Instance.TileToWorldSpace(placedOnTile.gridLocation);
    }

    public Vector2Int GetPosition()
    {
        Vector2Int pos = (Vector2Int)_manager.parentTile.gridLocation;

        return pos;
    }

    public void SetOrientation(Vector3 newOrientation)
    {
        _manager.SetOrientation(newOrientation);
        orientation = newOrientation;
    }
}
