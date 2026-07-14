using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance { get; private set; }

    [SerializeField] private SceneMap sceneMap;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (sceneMap == null)
        {
            Debug.LogError("SceneMap not found.");
        }
    }

    public void GoTo(string sceneName)
    {
        if (!sceneMap.Contains(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not registered.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public string GetCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }
}