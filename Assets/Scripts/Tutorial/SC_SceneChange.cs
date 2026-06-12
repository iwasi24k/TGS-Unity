using UnityEngine;
using UnityEngine.SceneManagement;

public class SC_SceneChange : MonoBehaviour
{
    [SerializeField]
    private string sceneName = "Game";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}