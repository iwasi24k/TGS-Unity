using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private Field field; // Field‚ðInspector‚ÅƒZƒbƒg

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            field.NextStage();
        }
    }
}
