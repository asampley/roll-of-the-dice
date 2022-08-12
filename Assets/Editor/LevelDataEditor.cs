using System;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private GUIStyle _buttonsStyle;

    public override void OnInspectorGUI()
    {
        if (_buttonsStyle == null)
        {
            _buttonsStyle = new GUIStyle(GUI.skin.button);
            _buttonsStyle.padding.right = 0;
            _buttonsStyle.padding.left = 0;
        }

        serializedObject.Update();

        GUILayoutOption[] mainOptions = new GUILayoutOption[]
        {

        };

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10f);
        EditorGUIUtility.labelWidth = 75f;


        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelName"), true, mainOptions);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneName"), true, mainOptions);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nextLevel"), true, mainOptions);
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);

        EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
        EditorGUIUtility.labelWidth = 85f;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("camStartPos"), true, mainOptions);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("camStartDist"), true, mainOptions);
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);

        EditorGUILayout.LabelField("Rules", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 75f;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gameRules"), true, mainOptions);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gridType"), true, mainOptions);
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);

        EditorGUILayout.LabelField("Allied Dice", EditorStyles.boldLabel);
        SerializedProperty alliedDice = serializedObject.FindProperty("alliedDice");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Dice Class");

        EditorGUILayout.LabelField("Tile Position");
        GUILayout.Space(-25f);
        EditorGUILayout.LabelField("Random Orientation");
        GUILayout.Space(-15f);
        EditorGUILayout.LabelField("Start Orientation");
        EditorGUILayout.EndHorizontal();
        var alliedEnumerator = alliedDice.GetEnumerator();
        while (alliedEnumerator.MoveNext())
        {
            var prop = alliedEnumerator.Current as SerializedProperty;
            if (prop == null) continue;
            //Add your treatment to the current child property...
            
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.fieldWidth = 50f;
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("diceClass"), GUIContent.none);
            EditorGUIUtility.fieldWidth = 50f;
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("tilePosition"), GUIContent.none);
            GUILayout.Space(10f);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("randomOrientation"), GUIContent.none);
            //if (prop.FindPropertyRelative("randomOrientation").Equals(true))
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("diceOrientation"), GUIContent.none);
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(10f);


        EditorGUILayout.LabelField("Enemy Dice", EditorStyles.boldLabel);
        SerializedProperty enemyDice = serializedObject.FindProperty("enemyDice");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Dice Class");

        EditorGUILayout.LabelField("Tile Position");
        GUILayout.Space(-25f);
        EditorGUILayout.LabelField("Random Orientation");
        GUILayout.Space(-15f);
        EditorGUILayout.LabelField("Start Orientation");
        EditorGUILayout.EndHorizontal();
        var enemyEnumerator = enemyDice.GetEnumerator();
        while (enemyEnumerator.MoveNext())
        {
            var prop = enemyEnumerator.Current as SerializedProperty;
            if (prop == null) continue;
            //Add your treatment to the current child property...

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.fieldWidth = 50f;
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("diceClass"), GUIContent.none);
            EditorGUIUtility.fieldWidth = 50f;
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("tilePosition"), GUIContent.none);
            GUILayout.Space(10f);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("randomOrientation"), GUIContent.none);
            //if (prop.FindPropertyRelative("randomOrientation").Equals(true))
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("diceOrientation"), GUIContent.none);
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        GUILayout.EndHorizontal();


        serializedObject.ApplyModifiedProperties();
    }
}



