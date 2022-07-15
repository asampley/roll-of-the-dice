using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    private void LateUpdate()
    {
        var focusedTileHit = GameManager.Instance.GetFocusedTile();

        if (focusedTileHit.HasValue)
        {
            GameObject overlayTile = focusedTileHit.Value.collider.gameObject;
            OverlayTile overlayTileManager = overlayTile.GetComponent<OverlayTile>();
            transform.position = overlayTile.transform.position;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder + 2;

            if (Input.GetMouseButton(0))
            {
                overlayTile.GetComponent<OverlayTile>().ShowTile();
                if (overlayTileManager.occupyingDie)
                {
                    Globals.SELECTED_UNIT = overlayTileManager.occupyingDie;
                }
                else if (!overlayTileManager.isBlocked && Globals.SELECTED_UNIT != null)
                {
                    Globals.SELECTED_UNIT.GetComponent<DieManager>().Move(overlayTileManager);
                }
            }
        }
    }

    
}
