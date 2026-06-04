using UnityEngine;

[CreateAssetMenu(menuName = "Scene/SceneMap")]
public class SceneMap : ScriptableObject
{
    public string[] sceneNames;

    public bool Contains(string sceneName)
    {
        foreach (var scene in sceneNames)
        {
            if (scene == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}