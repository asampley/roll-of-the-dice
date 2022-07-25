using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    protected UnitData _data;
    protected Transform _transform;
    protected string _uid;

    public Unit(UnitData data)
    {
        _data = data;
        GameObject g = GameObject.Instantiate(data.prefab) as GameObject;
        _transform = g.transform;

        _uid = System.Guid.NewGuid().ToString();
        _transform.name = _uid;
    }
}
