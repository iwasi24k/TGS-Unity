using UnityEngine;

public class SC_EnemyStartGate : MonoBehaviour
{
    public static bool IsOpened { get; private set; }

    private void Awake()
    {
        IsOpened = false;
    }

    public static void OpenGate()
    {
        IsOpened = true;
        Debug.Log("Enemy Start Gate Open");
    }

    private void OnDestroy()
    {
        IsOpened = true;
    }

    public static void ResetGate()
    {
        IsOpened = false;
        Debug.Log("Enemy Start Gate Reset");
    }
}
