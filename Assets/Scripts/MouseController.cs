using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    private Vector3 mouseStart;

    private void LateUpdate()
    {
        var mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * transform.position.z);

        if (Input.GetMouseButtonDown(0)) {
            mouseStart = mousePoint;
        } else if (Input.GetMouseButton(0)) {
            Camera.main.transform.position += mouseStart - mousePoint;
        }

        Camera.main.orthographicSize =
            Mathf.Clamp(
                Camera.main.orthographicSize - Input.mouseScrollDelta.y,
                Globals.MIN_CAMERA_SIZE,
                Globals.MAX_CAMERA_SIZE
            );

        var focusedTileHit = MapManager.Instance.GetFocusedTile();

        if (focusedTileHit.HasValue)
        {
            GameObject overlayTile = focusedTileHit.Value.collider.gameObject;
            OverlayTile overlayTileManager = overlayTile.GetComponent<OverlayTile>();
            transform.position = overlayTile.transform.position;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder + 2;

            if (Input.GetMouseButtonDown(0))
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
                if (Globals.SELECTED_UNIT.GetComponent<DieManager>().IsEnemy) return;

                if (!overlayTileManager.IsBlocked && Globals.SELECTED_UNIT != null)
                {
                    if (GameManager.Instance.PlayerPiecesMoved < GameManager.Instance.MaxPlayerMoves || GameManager.Instance.MovedPieces.Contains(Globals.SELECTED_UNIT) && !Globals.SELECTED_UNIT.IsMoving)
                    {
                        if (Globals.SELECTED_UNIT.movesInStraightLine)
                        {
                            var path = Globals.SELECTED_UNIT.GetComponent<DieManager>().PathGenerator(overlayTileManager);
                            Globals.SELECTED_UNIT.GetComponent<DieManager>().Move(path);
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
