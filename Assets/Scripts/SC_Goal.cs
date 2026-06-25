using UnityEngine;

public class SC_Goal : MonoBehaviour
{
    private Renderer goalRenderer;

    private Collider goalCollider;

    private Vector3 defaultScale;

    private bool isActive = false;

    private SC_Field field;

    //--------------------------------
    // ‰Šú‰»
    //--------------------------------
    public void Setup(SC_Field f)
    {
        field = f;

        goalRenderer =
            GetComponent<Renderer>();

        goalCollider = 
            GetComponent<Collider>();

        defaultScale =
            transform.localScale;

        CloseGoal();
    }

    //--------------------------------
    // ƒS[ƒ‹‰ğ•ú
    //--------------------------------
    public void ActivateGoal()
    {
        if (isActive) return;

        isActive = true;

        Debug.Log("ƒS[ƒ‹‰ğ•úI");

        goalRenderer.enabled = true;
        goalCollider.enabled = true;

        transform.localScale =
            defaultScale * 1.3f;
    }

    //--------------------------------
    // ƒS[ƒ‹•Â‚¶‚é
    //--------------------------------
    public void CloseGoal()
    {
        isActive = false;

        transform.localScale =
            defaultScale;

        goalRenderer.enabled = false;
        goalCollider.enabled = false;
    }

    //--------------------------------
    // ÚG”»’è
    //--------------------------------
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("‰½‚©G‚ê‚½ : " + other.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Player‚¶‚á‚È‚¢");
            return;
        }

        if (!isActive)
        {
            Debug.Log("‚Ü‚¾ƒS[ƒ‹•Â½’†");
            return;
        }

        Debug.Log("ŸƒXƒe[ƒW‚Ö");

        field.NextStage();
    }
}