using UnityEngine;

public class SC_footprintManager : MonoBehaviour
{

    public static SC_footprintManager instance;

    [Header("Footprint Settings")]
    [SerializeField] private RenderTexture _footprintRT;
    [SerializeField] private Material _footprintMaterial;

    public RenderTexture FootprintRT => _footprintRT;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of SC_footprintManager found! Destroying the new one.");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void Stamp(FootprintStampData data)
    {

    }

    public void LateUpdate()
    {
        
    }

}
