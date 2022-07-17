using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Input.GetKeyDown("space")) {
            GhostManager.Instance.SetEnemyGhostsVisible(true);
        } else if (Input.GetKeyUp("space")) {
            GhostManager.Instance.SetEnemyGhostsVisible(false);
        }
    }
}
