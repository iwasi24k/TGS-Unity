using UnityEngine;

public class SC_FootPrinter : MonoBehaviour
{
    [Header("RT Settings")]
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Shader paintShader;

    [Header("Footprint Settings")]
    [SerializeField] private float walkRadius = 0.02f;
    [SerializeField] private float dragRadius = 0.05f;
    [SerializeField] private float paintIntensity = 1.0f; // 濃さ
    [SerializeField] private LayerMask groundRayer;
    [SerializeField] private float fadeSpeed = 0.05f; // フェード速度 値が大きいほど早く消える

    private Material paintMaterial;
    private RenderTexture tempRT;
    private Vector2 lastUV;
    private bool isFirstFrame = true;

    public bool isKnockback{get; set; } = false;

    void Start()
    {
        if(paintShader == null || renderTexture == null)
        {
            Debug.LogError("Setup components properly.");
            return;
        }

        paintMaterial = new Material(paintShader);

        tempRT = new RenderTexture(renderTexture.width, renderTexture.height, 0, renderTexture.format);

        Graphics.Blit(Texture2D.blackTexture, renderTexture);
    }

    void Update()
    {

        paintMaterial.SetFloat("_DeltaTime", Time.deltaTime);
        paintMaterial.SetFloat("_FadeSpeed", fadeSpeed);

        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1.5f, groundRayer))
        {
            Vector2 uv = hit.textureCoord;

            if (isFirstFrame)
            {
                lastUV = uv;
                isFirstFrame = false;
                return;
            }

            if (isKnockback)
            {
                DrawDragLine(lastUV, uv);
                lastUV = uv;
            }
            else
            {
                if(Vector2.Distance(lastUV, uv) > 0.03f) // 一定量動いたらスタンプ
                {
                    PaintAtUV(uv, walkRadius);
                    lastUV = uv;
                }
                else
                {
                    ApplyOnlyFade(); // 動きが少ない場合はフェードのみ
                }
            }

        }
        else
        {
            ApplyOnlyFade(); // 地面に接触していない場合もフェードのみ
        }
    }

    private void  ApplyOnlyFade()
    {
        paintMaterial.SetVector("_BrushUV", new Vector4(-1, -1, 0, 0)); // UVを無効にする
        paintMaterial.SetFloat("_BrushRadius", 0); // 半径を0にする
        paintMaterial.SetFloat("_BrushIntensity", 0); // 濃さを0にする

        Graphics.Blit(renderTexture, tempRT, paintMaterial);
        Graphics.Blit(tempRT, renderTexture);
    }

    private void PaintAtUV(Vector2 uv,float radius)
    {
        paintMaterial.SetFloat("_FadeSpeed", fadeSpeed * Time.deltaTime);

        paintMaterial.SetVector("_BrushUV", new Vector4(uv.x, uv.y, 0, 0));
        paintMaterial.SetFloat("_BrushRadius", radius);
        paintMaterial.SetFloat("_BrushIntensity", paintIntensity);

        // Render to temporary RT
        Graphics.Blit(renderTexture, tempRT, paintMaterial);
        Graphics.Blit(tempRT, renderTexture);
    }

    private void DrawDragLine(Vector2 startUV, Vector2 endUV)
    {
        float dist = Vector2.Distance(startUV, endUV);
        int steps = Mathf.CeilToInt(dist / (dragRadius * 0.5f));

        for(int i = 0; i <= steps; i++)
        {
            float t = (steps == 0) ? 1.0f : (float)i / steps;
            Vector2 uv = Vector2.Lerp(startUV, endUV, t);
            PaintAtUV(uv, dragRadius);
        }
    }

    private void OnDestroy()
    {
        if(tempRT != null)
        {
            tempRT.Release();
        }
    }
}
