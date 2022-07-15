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
        Globals.SELECTED_UNIT = this;
        Debug.Log("selected");
    }

    public void Deselect()
    {
        _isSelected = false;
        Debug.Log("deselected");
    }
}
