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

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            StartCoroutine(ChangeScene(sceneMap.sceneNames[0]));
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            StartCoroutine(ChangeScene(sceneMap.sceneNames[1]));
        }
    }
}