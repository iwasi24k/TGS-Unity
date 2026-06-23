using UnityEngine;

[CreateAssetMenu(menuName = "Footprint/Setting")]
public class SC_footprintSetting : ScriptableObject
{
    [Header("Footprint Texture")]
    public Texture2D footprintTexture;

    [Header("Footprint Size")]
    public float radius = 0.03f;

    [Header("Footprint Depth")]
    public float depth = 1.0f;
}