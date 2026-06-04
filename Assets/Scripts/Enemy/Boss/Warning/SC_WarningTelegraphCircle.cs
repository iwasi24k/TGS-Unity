using UnityEngine;

public class SC_WarningTelegraphCircle : MonoBehaviour, SC_IPoolObject
{
    [Header("Ref")]
    [Tooltip("攻撃範囲全体を表示するMeshFilter"), SerializeField]
    private MeshFilter areaMeshFilter;

    [Tooltip("時間経過ゲージを表示するMeshFilter"), SerializeField]
    private MeshFilter gaugeMeshFilter;

    [Tooltip("外周線を表示するLineRenderer"), SerializeField]
    private LineRenderer borderLineRenderer;

    [Header("Setting")]
    [Tooltip("円を何分割するか。大きいほど滑らかになる"), SerializeField]
    private int segmentCount = 64;

    [Tooltip("ゲージメッシュを更新する間隔。小さいほど滑らかだが重くなる"), SerializeField]
    private float meshUpdateInterval = 0.03f;

    [Tooltip("外枠線の太さ"), SerializeField]
    private float borderWidth = 0.03f;

    private SC_ObjectPool ownerPool;

    private Mesh areaMesh;
    private Mesh gaugeMesh;

    private float radius;
    private float duration;
    private float timer;
    private float meshUpdateTimer;
    private bool isInitialized;

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        timer = 0f;
        meshUpdateTimer = 0f;
        isInitialized = false;

        gameObject.SetActive(true);
    }

    public void Init(float radius, float duration)
    {
        this.radius = Mathf.Max(0.01f, radius);
        this.duration = Mathf.Max(0.01f, duration);

        timer = 0f;
        meshUpdateTimer = 0f;
        isInitialized = true;

        BuildCircleMesh(
            areaMeshFilter,
            ref areaMesh,
            radius,
            360f
        );

        BuildCircleMesh(
            gaugeMeshFilter,
            ref gaugeMesh,
            0.01f,
            360f
        );

        BuildCircleBorder(this.radius);
    }

    private void Update()
    {
        if (!isInitialized) return;

        timer += Time.deltaTime;
        meshUpdateTimer += Time.deltaTime;

        if (meshUpdateTimer >= meshUpdateInterval)
        {
            meshUpdateTimer = 0f;

            float rate = Mathf.Clamp01(timer / duration);

            float currentRadius = radius * rate;
            currentRadius = Mathf.Max(currentRadius, 0.01f);

            BuildCircleMesh(
                gaugeMeshFilter,
                ref gaugeMesh,
                currentRadius,
                360f
            );
        }

        if (timer >= duration)
        {
            ReturnToPool();
        }
    }

    private void BuildCircleMesh(
        MeshFilter meshFilter,
        ref Mesh mesh,
        float radius,
        float angleRange
    )
    {
        if (meshFilter == null) return;

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Warning Circle Mesh";
        }
        else
        {
            mesh.Clear();
        }

        angleRange = Mathf.Clamp(angleRange, 0.1f, 360f);

        int currentSegmentCount = Mathf.Max(
            1,
            Mathf.CeilToInt(segmentCount * (angleRange / 360f))
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
            float angle = angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * radius;
            float z = Mathf.Cos(rad) * radius;

            vertices[i + 1] = new Vector3(x, 0f, z);
            uvs[i + 1] = new Vector2(
                (x / radius + 1f) * 0.5f,
                (z / radius + 1f) * 0.5f
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

    private void BuildCircleBorder(float radius)
    {
        if (borderLineRenderer == null) return;

        borderLineRenderer.startWidth = borderWidth;
        borderLineRenderer.endWidth = borderWidth;

        int pointCount = segmentCount + 1;

        borderLineRenderer.positionCount = pointCount;
        borderLineRenderer.loop = true;
        borderLineRenderer.useWorldSpace = false;

        for (int i = 0; i < pointCount; i++)
        {
            float angle = 360f / segmentCount * i;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * radius;
            float z = Mathf.Cos(rad) * radius;

            borderLineRenderer.SetPosition(i, new Vector3(x, 0.01f, z));
        }
    }

    public void ReturnToPool()
    {
        isInitialized = false;

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