using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static float STEP_TIME = 1f;
    public static DieManager SELECTED_UNIT;
    public static Vector2 TILE_SIZE = new Vector2(1.0f, 0.5f);
    public static float MOVEMENT_TIME = 0.4f;

    public static LayerMask OVERLAY_TILE = 1 << 9;
}
