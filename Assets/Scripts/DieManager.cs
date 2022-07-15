using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieManager : MonoBehaviour
{
    private bool _isSelected;


    public void Move(OverlayTile newTile)
    {
        transform.position = newTile.transform.position;
        newTile.occupyingDie = this;
    }

    public void Select()
    {
        _isSelected = true;
    }

    public void Deselect()
    {
        _isSelected = false;
    }
}
