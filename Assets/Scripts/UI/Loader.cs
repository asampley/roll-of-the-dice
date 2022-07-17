using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene : int
    {
        Loading = -1,
        MainMenu = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
    }

    private static Action onLoaderCallback;

    public static void Load(Scene scene)
    {
        onLoaderCallback = () =>
        {
            SceneManager.LoadScene(Scene.Loading.ToString());
        };

        SceneManager.LoadScene(scene.ToString());
    }

    public static void LoaderCallback()
    {
        if (onLoaderCallback != null)
        {
            onLoaderCallback();
            onLoaderCallback = null;
        }
    }

    public static void LoadNext() {
        if (!Enum.TryParse(SceneManager.GetActiveScene().name, out Scene current)) return;

        if (!LoadLevelNum((int)current + 1)) {
            Load(Scene.MainMenu);
        }
    }

    public static bool LoadLevelNum(int num) {
        if (num > 0) {
            if (!Enum.IsDefined(typeof(Scene), num)) {
                return false;
            } else {
                Load((Scene)Enum.ToObject(typeof(Scene), num));
                return true;
            }
        }
        return false;
    }
}
