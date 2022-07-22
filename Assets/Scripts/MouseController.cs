using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    private void LateUpdate()
    {
        var focusedTileHit = MapManager.Instance.GetFocusedTile();

        if (focusedTileHit.HasValue)
        {
            GameObject overlayTile = focusedTileHit.Value.collider.gameObject;
            OverlayTile overlayTileManager = overlayTile.GetComponent<OverlayTile>();
            transform.position = overlayTile.transform.position;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder + 2;

            if (Input.GetMouseButton(0))
            {
                if (overlayTileManager.occupyingDie != null)
                {
                    overlayTileManager.occupyingDie.Select();
                }
                else
                {
                    Globals.SELECTED_UNIT?.Deselect();
                }

            }
            if (Input.GetMouseButtonDown(1))
            {
                if (Globals.SELECTED_UNIT.GetComponent<DieManager>().isEnemy) return;

                if (!overlayTileManager.IsBlocked && Globals.SELECTED_UNIT != null)
                {
                    if (GameManager.Instance.PlayerPiecesMoved < GameManager.Instance.MaxPlayerMoves || GameManager.Instance.MovedPieces.Contains(Globals.SELECTED_UNIT))
                    {
                        if (Globals.SELECTED_UNIT.movesInStraightLine)
                        {
                            List<OverlayTile> tiles = Globals.SELECTED_UNIT.GetComponent<DieManager>().FollowPath(overlayTileManager);
                            Debug.Log(Utilities.EnumerableString(tiles));
                            Globals.SELECTED_UNIT.GetComponent<DieManager>().Move(tiles.GetEnumerator());
                        }
                        else
                        {
                            Globals.SELECTED_UNIT.GetComponent<DieManager>().Move(overlayTileManager);
                        }
                    }
                }
            }
        }
    }


}
