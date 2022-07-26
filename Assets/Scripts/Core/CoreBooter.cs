using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreBooter : MonoBehaviour
{
    public static CoreBooter instance;
    public UnityEngine.UI.Image sceneTransitioner;
    

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        LoadMenu();
    }

    public void LoadMenu() => StartCoroutine(_SwitchingScene("menu"));

    public void LoadLevel(LevelData level)
    {
        CoreDataHandler.instance.SetLevelData(level);
        string s = level.levelName;
        StartCoroutine(_SwitchingScene("game", s));
    }

    private AsyncOperation _LoadMenu()
    {
        AudioListener prevListener = Object.FindObjectOfType<AudioListener>();
        if (prevListener != null) prevListener.enabled = false;
        AsyncOperation op = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        op.completed += (_) =>
        {
            Scene s = SceneManager.GetSceneByName("GameScene");
            if (s != null && s.IsValid())
                SceneManager.UnloadSceneAsync(s);
        };
        return op;
    }

    private AsyncOperation _LoadLevel(string map)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(map, LoadSceneMode.Additive);
        AudioListener prevListener = Object.FindObjectOfType<AudioListener>();
        op.completed += (_) =>
        {
            if (prevListener != null) prevListener.enabled = false;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(map));
            Scene s = SceneManager.GetSceneByName("MainMenu");
            if (s != null && s.IsValid())
                SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive).completed += (_) =>
                {
                    SceneManager.UnloadSceneAsync(s);
                };
            else
                SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);                
        };
        return op;
    }

    private IEnumerator _SwitchingScene(string to, string map = "")
    {
        sceneTransitioner.color = Color.clear;

        float t = 0;
        while (t < 1f)
        {
            sceneTransitioner.color = Color.Lerp(Color.clear, Color.black, t);
            t += Time.deltaTime;
            yield return null;
        }

        AsyncOperation op;
        if (to == "menu")
            op = _LoadMenu();
        else
            op = _LoadLevel(map);

        yield return new WaitUntil(() => op.isDone);

        t = 0;
        while (t < 1f)
        {
            sceneTransitioner.color = Color.Lerp(Color.black, Color.clear, t);
            t += Time.deltaTime;
            yield return null;
        }

        sceneTransitioner.color = Color.clear;
    }
}
