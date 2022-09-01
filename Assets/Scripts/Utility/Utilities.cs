using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class Utilities
{
    public static string FirstCharToUpper(string input)
    {
        return input[0].ToString().ToUpper() + input.Substring(1);
    }

    public static string StrJoin<T>(this IEnumerable<T> enumerable, string separator = ", ") {
        return "[" + string.Join(separator, enumerable) + "]";
    }

    /// <summary>
    /// Gets all children of `SerializedProperty` at 1 level depth.
    /// </summary>
    /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
    /// <returns>Collection of `SerializedProperty` children.</returns>
    public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty serializedProperty)
    {
        SerializedProperty currentProperty = serializedProperty.Copy();
        SerializedProperty nextSiblingProperty = serializedProperty.Copy();
        {
            nextSiblingProperty.Next(false);
        }

        if (currentProperty.Next(true))
        {
            do
            {
                if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                    break;

                yield return currentProperty;
            }
            while (currentProperty.Next(false));
        }
    }

    /// <summary>
    /// Gets visible children of `SerializedProperty` at 1 level depth.
    /// </summary>
    /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
    /// <returns>Collection of `SerializedProperty` children.</returns>
    public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
    {
        SerializedProperty currentProperty = serializedProperty.Copy();
        SerializedProperty nextSiblingProperty = serializedProperty.Copy();
        {
            nextSiblingProperty.NextVisible(false);
        }

        if (currentProperty.NextVisible(true))
        {
            do
            {
                if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                    break;

                yield return currentProperty;
            }
            while (currentProperty.NextVisible(false));
        }
    }
}
