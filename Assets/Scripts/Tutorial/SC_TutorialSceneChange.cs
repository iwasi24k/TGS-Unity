using UnityEngine;
using UnityEngine.SceneManagement;

public class SC_TutorialSceneChange : MonoBehaviour
{
    [Header("‘JˆÚæƒV[ƒ“–¼")]
    [SerializeField]
    private string sceneName = "Scene_Game";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log(
            "Scene Change : " +
            sceneName);

        SceneManager.LoadScene(sceneName);
    }
}