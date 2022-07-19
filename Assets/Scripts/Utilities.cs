using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static string FirstCharToUpper(string input)
    {
        return input[0].ToString().ToUpper() + input.Substring(1);
    }
}
