using UnityEngine;

[CreateAssetMenu(menuName = "Footprint/Footprint Setting")]
public class SC_footprintSetting : ScriptableObject
{
    public Texture2D footprintTexture;

    public float radius;

    public float depth;
}

public struct FootprintStampData
{
    public Vector2 uv;
    public float radius;
    public float depth;
    public float rotation;
}