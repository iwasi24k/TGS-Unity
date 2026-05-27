using UnityEngine;
using System.Collections;

public class SC_AfterImage : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 0.02f; // 残像の生成間隔（秒）
    [SerializeField] private float lifeTime = 0.05f; // 残像の寿命（秒）

    [Header("Reference")]
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer; // プレイヤーのMesh
    [SerializeField] private Material ghostMaterial;        // 残像用マテリアル

    [Header("Fade Settings")]
    [SerializeField, Range(0f, 1f)] private float startAlpha = 0.2f; // 残像の初期透明度

    private const string AFTER_IMAGE_NAME = "AfterImage";
    private const float END_ALPHA = 0f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private Coroutine spawnCoroutine;
    private bool isActive = false;

  
    ///残像生成を開始する
    public void StartTrail()
    {
        if (isActive) return;

        isActive = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    ///残像生成を停止する
    public void StopTrail()
    {
        if (!isActive) return;

        isActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    ///一定間隔で残像を生成し続けるループ
    private IEnumerator SpawnLoop()
    {
        // WaitForSecondsをキャッシュ
        var wait = new WaitForSeconds(spawnInterval);

        while (isActive)
        {
            CreateAfterImage();
            yield return wait;
        }
    }

    ///現在のポーズでMeshを複製して残像オブジェクトを生成する
    private void CreateAfterImage()
    {
        // 現在のアニメーションポーズをMeshとして取得
        Mesh bakedMesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(bakedMesh);

        // 残像オブジェクト生成・Mesh設定
        GameObject obj = new GameObject(AFTER_IMAGE_NAME);
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();

        mf.sharedMesh = bakedMesh;
        mr.sharedMaterial = ghostMaterial;

        // プレイヤーのTransformをそのままコピー
        obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
        obj.transform.localScale = transform.localScale;

        // フェードアウト開始
        StartCoroutine(FadeAndDestroy(obj, mr));
    }

    private IEnumerator FadeAndDestroy(GameObject obj, MeshRenderer renderer)
    {
        float elapsed = 0f;
        Material mat = renderer.material;

        Color color = mat.GetColor(BaseColorId);

        while (elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;

            // 経過時間に応じてalphaを後半ほど薄くする
            color.a = Mathf.Lerp(startAlpha, END_ALPHA, elapsed / lifeTime);

            mat.SetColor(BaseColorId, color);

            yield return null;
        }

        Destroy(obj);
    }

    private void OnDisable()
    {
        // オブジェクトが無効化されたときに確実に停止する
        StopTrail();
    }
}