using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneConditions : MonoBehaviour
{
    [SerializeField] private SceneMap sceneMap;

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

        yield return FadeManager.FadeIn();
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
    }

    // リザルトへ
    public void GoResult()
    {
        StartCoroutine(ChangeScene(sceneMap.sceneNames[3]));
    }

}