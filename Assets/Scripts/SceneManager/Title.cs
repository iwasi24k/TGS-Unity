using UnityEngine;

public class SC_Title : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject tutorialPanel;

    [Header("Scene")]
    [SerializeField] private SceneConditions sceneConditions;

    private void Start()
    {
        tutorialPanel.SetActive(false);
        mainMenu.SetActive(true);
    }

    // STARTƒ{ƒ^ƒ“
    public void OnClickStart()
    {
        mainMenu.SetActive(false);
        tutorialPanel.SetActive(true);
        //Debug.Log("START‰Ÿ‚³‚ê‚½");
       
    }

    // ‚Í‚¢
    public void OnClickYes()
    {
        sceneConditions.GoTutorial();
    }

    // ‚¢‚¢‚¦
    public void OnClickNo()
    {
        sceneConditions.GoGame();
    }

    // OPTION
    public void OnClickOption()
    {
        Debug.Log("Option");
        Debug.Log("OPTION‰Ÿ‚³‚ê‚½");
    }

    // EXIT
    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
