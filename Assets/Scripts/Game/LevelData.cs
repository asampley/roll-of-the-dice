using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.SceneManagement;
using Unity.EditorCoroutines.Editor;

public enum GridType
{
    Square,
    Triangle,
    Hexagon,
}

public enum WorldName
{
    Meadows,
}


[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData", order = 2)]
public class LevelData : ScriptableObject
{
    public string levelName;
    public WorldName worldName;
    public string sceneName;
    public LevelData nextLevel;

    // Camera
    public Vector3 camStartPos = new(-0.25f, 1f, -200f);
    public float camStartDist = 2.5f;

    // Rules
    public GameRulesData gameRules;
    public GridType gridType;

    // Dice
    public DiceSpawn[] alliedDice;
    public DiceSpawn[] enemyDice;
    

    private void OnValidate()
    {
        List<Vector2Int> positions = new();
        foreach (DiceSpawn dice in alliedDice)
        {
            if (positions.Contains(dice.tilePosition))
            {
                Debug.LogError("Identical spawn positions in a start position");
            }
            else
            {
                positions.Add(dice.tilePosition);
            }
        }
        foreach (DiceSpawn dice in enemyDice)
        {
            if (positions.Contains(dice.tilePosition))
            {
                Debug.LogError("Identical spawn positions in a start position");
            }
            else
            {
                positions.Add(dice.tilePosition);
            }
        }
    }

    public void LoadScene()
    {
        DataHandler.LoadGameData();
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        string scene = Globals.SCENES_FOLDER + "/" + worldName.ToString() + "/" + sceneName + ".unity";
        EditorSceneManager.OpenScene(scene);

        MapManager mapManager = FindObjectOfType(typeof(MapManager)) as MapManager;

        mapManager.DeclareInstance();
        mapManager.GenerateMap();

        foreach (DiceSpawn die in alliedDice)
            SpawnDie(die.tilePosition, die.diceClass, false, die.diceOrientation);
        foreach (DiceSpawn die in enemyDice)
            SpawnDie(die.tilePosition, die.diceClass, true, die.diceOrientation);        
    }

    public void SpawnDie(Vector2Int startPos, DiceClass diceClass, bool isEnemy, DiceOrientation orientation)
    {
        EditorCoroutineUtility.StartCoroutine(SleepyLoadUnit(2, startPos, diceClass, isEnemy, orientation), this);
    }

    IEnumerator SleepyLoadUnit(float time, Vector2Int startPos, DiceClass diceClass, bool isEnemy, DiceOrientation orientation)
    {
        yield return new WaitForSeconds(time);

        UnitData unitData = Globals.UNIT_DATA.Where((UnitData x) => x.unitClass == diceClass).First();
        Unit die = new(unitData, isEnemy, orientation);
        die.SetPosition(startPos);
    }
}
