using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayTile : MonoBehaviour
{
    public void ShowTile()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }
    public void HideTile()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
}
