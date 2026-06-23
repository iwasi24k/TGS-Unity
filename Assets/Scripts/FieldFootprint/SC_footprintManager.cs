using UnityEngine;
using System.Collections.Generic;

public class SC_footprintManager : MonoBehaviour
{

    public static SC_footprintManager instance;

    [Header("Render Texture Settings")]
    [SerializeField] private int _rtSize = 1024;
    [SerializeField] private RenderTexture _footprintRT;

    [Header("Material Settings")]
    [SerializeField] private Material _footprintMaterial;
    [SerializeField] private Material _decayMaterial;

    public RenderTexture FootprintRT => _footprintRT;

    private Queue<FootprintStampData> _stampQueue = new Queue<FootprintStampData>();

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of SC_footprintManager found! Destroying the new one.");
            Destroy(gameObject);
            return;
        }
        instance = this;

        CreateRenderTexture();
    }

    private void CreateRenderTexture()
    {
        _footprintRT = new RenderTexture(_rtSize, _rtSize, 0, RenderTextureFormat.RFloat);

        _footprintRT.enableRandomWrite = false;
        _footprintRT.Create();
    }

    public void EnqueueStamp(FootprintStampData data)
    {
        _stampQueue.Enqueue(data);
    }

    private void ProcessStampQueue()
    {
        while (_stampQueue.Count > 0)
        {
            FootprintStampData data = _stampQueue.Dequeue();
            Stamp(data);
        }
    }

    private void Stamp(FootprintStampData data)
    {
        _footprintMaterial.SetVector(
            "_StampPos",
            data.uv
        );

        _footprintMaterial.SetFloat(
            "_StampRadius",
            data.radius
        );

        _footprintMaterial.SetFloat(
            "_StampDepth",
            data.depth
        );

        RenderTexture temp = RenderTexture.GetTemporary(_footprintRT.width, _footprintRT.height);

        Graphics.Blit(_footprintRT, temp, _footprintMaterial);

        Graphics.Blit(temp, _footprintRT);

        RenderTexture.ReleaseTemporary(temp);
    }

    private void ApplyDecay()
    {
        if (_decayMaterial == null) return;

        RenderTexture temp = RenderTexture.GetTemporary(
            _footprintRT.width, 
            _footprintRT.height,
            0,
            _footprintRT.format
            );

        Graphics.Blit(_footprintRT, temp, _decayMaterial);

        Graphics.Blit(temp, _footprintRT);

        RenderTexture.ReleaseTemporary(temp);
    }

    public void LateUpdate()
    {
        ProcessStampQueue();

        ApplyDecay();
    }

}
