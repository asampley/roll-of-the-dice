using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static string FirstCharToUpper(string input)
    {
        return input[0].ToString().ToUpper() + input.Substring(1);
    }

    public static string EnumerableString(IEnumerable enumerable) {
        string str = "[";
        foreach (var a in enumerable) {
            str += a + ",";
        }
        str += "]";

        return str;
    }
}
