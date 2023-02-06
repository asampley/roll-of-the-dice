using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Utilities
{
    public static string FirstCharToUpper(string input)
    {
        return input[0].ToString().ToUpper() + input.Substring(1);
    }

    public static string StrJoin<T>(this IEnumerable<T> enumerable, string separator = ", ") {
        return "[" + string.Join(separator, enumerable) + "]";
    }

    public static T DebugLog<T>(this T t, params object[] additional) {
        Debug.Log(t.ToString() + ": " + additional.StrJoin());
        return t;
    }
}
