using UnityEngine;

public class SC_Result : MonoBehaviour
{
    [SerializeField] private SceneConditions sceneConditions;

    // ƒ^ƒCƒgƒ‹‚Ö–ß‚é
    public void OnClickTitle()
    {
        sceneConditions.GoTitle();
    }
}