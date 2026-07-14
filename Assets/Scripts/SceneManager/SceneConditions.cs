using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneConditions : MonoBehaviour
{
    [SerializeField] private SceneMap sceneMap;

    private string currentScene;

    private void Awake()
    {
        if (sceneMap == null)
        {
            Debug.LogError("SceneMap not found.");
        }
    }

    private IEnumerator ChangeScene(string sceneName)
    {
        yield return FadeManager.FadeOut();

        SceneChanger.Instance.GoTo(sceneName);
        currentScene = SceneChanger.Instance.GetCurrentScene();
        Debug.Log(currentScene);
        yield return FadeManager.FadeIn();
    }

    public void Update()
    {
        Debug.Log(currentScene);
        if (currentScene == sceneMap.sceneNames[2] && GameData.Result.IsGameEnd)
        {
            GoResult();
        }
    }

    // タイトルへ
    public void GoTitle()
    {
        StartCoroutine(ChangeScene(sceneMap.sceneNames[0]));
    }

    // チュートリアルへ
    public void GoTutorial()
    {
        StartCoroutine(ChangeScene(sceneMap.sceneNames[1]));
    }

    // ゲームへ
    public void GoGame()
    {
        StartCoroutine(ChangeScene(sceneMap.sceneNames[2]));
        Debug.Log(sceneMap.sceneNames[2]);

    }

    // リザルトへ
    public void GoResult()
    {
        StartCoroutine(ChangeScene(sceneMap.sceneNames[3]));
    }

}