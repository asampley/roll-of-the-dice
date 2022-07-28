using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreBooter : MonoBehaviour
{
    public static CoreBooter Instance;
    public UnityEngine.UI.Image sceneTransitioner;
    private string _prevLevel;
    

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        LoadMenu();
    }

    public void LoadMenu() => StartCoroutine(_SwitchingScene("menu"));

    public void LoadLevel(LevelData level)
    {
        if (CoreDataHandler.Instance.LevelData != null)
            _prevLevel = CoreDataHandler.Instance.LevelData.sceneName;
        CoreDataHandler.Instance.SetLevelData(level);
        string s = level.sceneName;
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
            {
                SceneManager.UnloadSceneAsync(s);
                SceneManager.UnloadSceneAsync(CoreDataHandler.Instance.LevelData.sceneName);
            }                
        };
        return op;
    }

    private AsyncOperation _LoadLevel(string scene)
    {
        AsyncOperation op;
        AudioListener prevListener = Object.FindObjectOfType<AudioListener>();

        if (prevListener != null) prevListener.enabled = false;

        if (SceneManager.GetSceneByName(_prevLevel) != null && SceneManager.GetSceneByName(_prevLevel).IsValid())
            op = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_prevLevel));
        else
            op = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("MainMenu"));

        op.completed += (_) =>
        {
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive).completed += (_) =>
            {
                if (SceneManager.GetSceneByName("GameScene") != null && SceneManager.GetSceneByName("GameScene").IsValid())
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("GameScene")).completed += (_) =>
                        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
                else
                    SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
                    
            };
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
                  
        };
        return op;
    }

    private IEnumerator _SwitchingScene(string to, string scene = "")
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
            op = _LoadLevel(scene);

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
