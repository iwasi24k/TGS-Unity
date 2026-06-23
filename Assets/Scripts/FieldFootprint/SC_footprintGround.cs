using UnityEngine;

public class SC_footprintGround : MonoBehaviour
{
    [SerializeField]
    private Renderer _targetRenderer;

    private Material _footprintMaterial;

    void Start()
    {
        _footprintMaterial = _targetRenderer.material;

        _footprintMaterial.SetTexture("_FootprintTex", SC_footprintManager.instance.FootprintRT);
    }
}
