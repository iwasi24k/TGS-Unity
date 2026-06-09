using UnityEngine;

public class SC_WarningTelegraphRectangle : MonoBehaviour, SC_IPoolObject
{
    [Header("Ref")]
    [Tooltip("攻撃範囲全体を表示するMeshFilter"), SerializeField]
    private MeshFilter areaMeshFilter;

    [Tooltip("時間経過ゲージを表示するMeshFilter"), SerializeField]
    private MeshFilter gaugeMeshFilter;

    [Tooltip("外枠を表示するLineRenderer"), SerializeField]
    private LineRenderer borderLineRenderer;

    [Header("Setting")]
    [Tooltip("ゲージメッシュを更新する間隔。小さいほど滑らかだが重くなる"), SerializeField]
    private float meshUpdateInterval = 0.03f;

    [Tooltip("外枠線の太さ"), SerializeField]
    private float borderWidth = 0.03f;

    private SC_ObjectPool ownerPool;

    private Mesh areaMesh;
    private Mesh gaugeMesh;

    private float width;
    private float length;
    private float duration;
    private float timer;
    private float meshUpdateTimer;

    private bool initialized;

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        timer = 0f;
        meshUpdateTimer = 0f;

        width = 0f;
        length = 0f;
        duration = 0f;

        initialized = false;

        gameObject.SetActive(true);
    }

    public void Init(float width, float length, float duration)
    {
        this.width = Mathf.Max(0.01f, width);
        this.length = Mathf.Max(0.01f, length);
        this.duration = Mathf.Max(0.01f, duration);

        timer = 0f;
        meshUpdateTimer = 0f;
        initialized = true;

        BuildRectangleMesh(areaMeshFilter, ref areaMesh, this.width, this.length);
        BuildRectangleMesh(gaugeMeshFilter, ref gaugeMesh, this.width, 0.01f);
        //BuildBorder(this.width, this.length);
    }

    private void Update()
    {
        if (!initialized) return;

        timer += Time.deltaTime;
        meshUpdateTimer += Time.deltaTime;

        if (meshUpdateTimer >= meshUpdateInterval)
        {
            meshUpdateTimer = 0f;

            float rate = Mathf.Clamp01(timer / duration);

            float currentLength = length * rate;
            currentLength = Mathf.Max(currentLength, 0.01f);

            BuildRectangleMesh(
                gaugeMeshFilter,
                ref gaugeMesh,
                width,
                currentLength
            );
        }

        if (timer >= duration)
        {
            ReturnToPool();
        }
    }

    private void BuildRectangleMesh(
        MeshFilter meshFilter,
        ref Mesh mesh,
        float width,
        float length
    )
    {
        if (meshFilter == null) return;

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Warning Rectangle Mesh";
        }
        else
        {
            mesh.Clear();
        }

        float halfWidth = width * 0.5f;

        Vector3[] vertices = new Vector3[4];

        // 原点から前方へ伸びる四角形
        vertices[0] = new Vector3(-halfWidth, 0f, 0f);
        vertices[1] = new Vector3(halfWidth, 0f, 0f);
        vertices[2] = new Vector3(-halfWidth, 0f, length);
        vertices[3] = new Vector3(halfWidth, 0f, length);

        int[] triangles =
        {
            0, 2, 1,
            1, 2, 3
        };

        Vector2[] uvs =
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private void BuildBorder(float width, float length)
    {
        if (borderLineRenderer == null) return;

        borderLineRenderer.startWidth = borderWidth;
        borderLineRenderer.endWidth = borderWidth;

        float halfWidth = width * 0.5f;

        borderLineRenderer.positionCount = 5;
        borderLineRenderer.loop = false;
        borderLineRenderer.useWorldSpace = false;

        borderLineRenderer.SetPosition(0, new Vector3(-halfWidth, 0.01f, 0f));
        borderLineRenderer.SetPosition(1, new Vector3(halfWidth, 0.01f, 0f));
        borderLineRenderer.SetPosition(2, new Vector3(halfWidth, 0.01f, length));
        borderLineRenderer.SetPosition(3, new Vector3(-halfWidth, 0.01f, length));
        borderLineRenderer.SetPosition(4, new Vector3(-halfWidth, 0.01f, 0f));
    }

    public void ReturnToPool()
    {
        initialized = false;

        if (ownerPool != null)
        {
            ownerPool.ReturnObject(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (areaMesh != null)
        {
            Destroy(areaMesh);
            areaMesh = null;
        }

        if (gaugeMesh != null)
        {
            Destroy(gaugeMesh);
            gaugeMesh = null;
        }
    }
}
