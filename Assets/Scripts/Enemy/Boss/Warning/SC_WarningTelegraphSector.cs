using UnityEngine;

public class SC_WarningTelegraphSector : MonoBehaviour, SC_IPoolObject
{
    [Header("Ref")]
    [Tooltip("攻撃範囲全体を表示するMeshFilter"), SerializeField]
    private MeshFilter areaMeshFilter;

    [Tooltip("時間経過ゲージを表示するMeshFilter"), SerializeField]
    private MeshFilter gaugeMeshFilter;

    [Tooltip("扇形の外周線"), SerializeField]
    private LineRenderer borderOuterLineRenderer;

    [Tooltip("扇形の左側境界線"), SerializeField]
    private LineRenderer borderLeftLineRenderer;

    [Tooltip("扇形の右側境界線"), SerializeField]
    private LineRenderer borderRightLineRenderer;

    [Header("Setting")]
    [Tooltip("扇形を何分割するか。大きいほど滑らか"), SerializeField]
    private int segmentCount = 32;

    [Tooltip("ゲージメッシュを更新する間隔。小さいほど滑らかだが重くなる"), SerializeField]
    private float meshUpdateInterval = 0.03f;

    [Tooltip("外枠線の太さ"), SerializeField]
    private float borderWidth = 0.03f;

    private SC_ObjectPool ownerPool;

    private Mesh areaMesh;
    private Mesh gaugeMesh;

    private float radius;
    private float startAngle;
    private float angleRange;
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
        radius = 0f;
        startAngle = 0f;
        angleRange = 0f;
        duration = 0f;

        timer = 0f;
        meshUpdateTimer = 0f;

        initialized = false;

        gameObject.SetActive(true);
    }

    public void Init(
        float radius,
        float centerAngle,
        float angleRange,
        float duration
    )
    {
        this.radius = Mathf.Max(0.01f, radius);
        this.angleRange = Mathf.Clamp(angleRange, 0.1f, 360f);
        this.startAngle = centerAngle - this.angleRange * 0.5f;
        this.duration = Mathf.Max(0.01f, duration);

        timer = 0f;
        meshUpdateTimer = 0f;
        initialized = true;

        BuildSectorMesh(
            areaMeshFilter,
            ref areaMesh,
            this.radius,
            this.startAngle,
            this.angleRange
        );

        BuildSectorMesh(
            gaugeMeshFilter,
            ref gaugeMesh,
            0.01f,
            this.startAngle,
            this.angleRange
        );

        BuildSectorBorder();
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

            float currentRadius = radius * rate;
            currentRadius = Mathf.Max(currentRadius, 0.01f);

            BuildSectorMesh(
                gaugeMeshFilter,
                ref gaugeMesh,
                currentRadius,
                startAngle,
                angleRange
            );
        }

        if (timer >= duration)
        {
            ReturnToPool();
        }
    }

    private void BuildSectorMesh(
    MeshFilter meshFilter,
    ref Mesh mesh,
    float drawRadius,
    float startAngle,
    float angleRange
)
    {
        if (meshFilter == null) return;

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Warning Sector Mesh";
        }
        else
        {
            mesh.Clear();
        }

        drawRadius = Mathf.Max(0.01f, drawRadius);
        angleRange = Mathf.Clamp(angleRange, 0.1f, 360f);

        int safeSegmentCount = Mathf.Max(1, segmentCount);

        int currentSegmentCount = Mathf.Max(
            1,
            Mathf.CeilToInt(safeSegmentCount * (angleRange / 360f))
        );

        int vertexCount = currentSegmentCount + 2;

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[currentSegmentCount * 3];
        Vector2[] uvs = new Vector2[vertexCount];

        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);

        float angleStep = angleRange / currentSegmentCount;

        for (int i = 0; i <= currentSegmentCount; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            float sin = Mathf.Sin(rad);
            float cos = Mathf.Cos(rad);

            float x = sin * drawRadius;
            float z = cos * drawRadius;

            vertices[i + 1] = new Vector3(x, 0f, z);

            uvs[i + 1] = new Vector2(
                (sin + 1f) * 0.5f,
                (cos + 1f) * 0.5f
            );
        }

        for (int i = 0; i < currentSegmentCount; i++)
        {
            int triIndex = i * 3;

            triangles[triIndex + 0] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private void BuildSectorBorder()
    {
        BuildOuterBorder();
        BuildSideBorder(borderLeftLineRenderer, startAngle);
        BuildSideBorder(borderRightLineRenderer, startAngle + angleRange);
    }

    private void BuildOuterBorder()
    {
        if (borderOuterLineRenderer == null) return;

        borderOuterLineRenderer.startWidth = borderWidth;
        borderOuterLineRenderer.endWidth = borderWidth;

        int safeSegmentCount = Mathf.Max(1, segmentCount);

        int currentSegmentCount = Mathf.Max(
            1,
            Mathf.CeilToInt(safeSegmentCount * (angleRange / 360f))
        );

        borderOuterLineRenderer.positionCount = currentSegmentCount + 1;
        borderOuterLineRenderer.loop = false;
        borderOuterLineRenderer.useWorldSpace = false;

        float angleStep = angleRange / currentSegmentCount;

        for (int i = 0; i <= currentSegmentCount; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * radius;
            float z = Mathf.Cos(rad) * radius;

            borderOuterLineRenderer.SetPosition(
                i,
                new Vector3(x, 0.01f, z)
            );
        }
    }

    private void BuildSideBorder(LineRenderer lineRenderer, float angle)
    {
        if (lineRenderer == null) return;

        lineRenderer.startWidth = borderWidth;
        lineRenderer.endWidth = borderWidth;

        float rad = angle * Mathf.Deg2Rad;

        float x = Mathf.Sin(rad) * radius;
        float z = Mathf.Cos(rad) * radius;

        lineRenderer.positionCount = 2;
        lineRenderer.loop = false;
        lineRenderer.useWorldSpace = false;

        lineRenderer.SetPosition(0, new Vector3(0f, 0.01f, 0f));
        lineRenderer.SetPosition(1, new Vector3(x, 0.01f, z));
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