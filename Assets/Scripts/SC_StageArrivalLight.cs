using System.Collections;
using UnityEngine;

public class SC_StageArrivalLight : MonoBehaviour, SC_IPoolObject
{
    [Header("Settings")]
    [Tooltip("上空から降りる時間")]
    [SerializeField] private float descendTime = 0.7f;

    [Tooltip("表示を維持する時間")]
    [SerializeField] private float holdTime = 1.5f;

    [Tooltip("地面到着後、上端が落ちて消える時間")]
    [SerializeField] private float collapseTime = 0.5f;

    [Tooltip("プレイヤー上空の開始位置")]
    [SerializeField] private float startHeight = 15f;

    [Tooltip("プレイヤー足元からの高さ調整")]
    [SerializeField] private float groundOffset = -1.0f;

    private SC_ObjectPool ownerPool;

    private Renderer beamRenderer;
    private Material beamMaterial;

    private Vector3 defaultScale;
    private float defaultAlpha = 1f;

    private Coroutine playCoroutine;

    private static readonly int AlphaProperty =
        Shader.PropertyToID("_Alpha");

    private void Awake()
    {
        beamRenderer = GetComponent<Renderer>();
        defaultScale = transform.localScale;

        if (beamRenderer == null)
        {
            Debug.LogError(
                "PF_ArrivalLightにRendererがありません。",
                this
            );

            return;
        }

        beamMaterial = beamRenderer.material;

        if (beamMaterial.HasProperty(AlphaProperty))
        {
            defaultAlpha =
                beamMaterial.GetFloat(AlphaProperty);
        }
        else
        {
            Debug.LogWarning(
                "Shader Graphに_Alphaがありません。",
                this
            );
        }
    }

    public void SetPool(SC_ObjectPool pool)
    {
        ownerPool = pool;
    }

    public void OnGetFromPool()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        transform.localScale = defaultScale;

        SetAlpha(defaultAlpha);

        if (beamRenderer != null)
        {
            beamRenderer.enabled = false;
        }
    }

    public void OnReturnToPool()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        transform.localScale = defaultScale;

        SetAlpha(defaultAlpha);

        if (beamRenderer != null)
        {
            beamRenderer.enabled = false;
        }
    }

    public static void Play()
    {
        if (SC_EnemyObjectPoolManager.Instance == null)
        {
            Debug.LogWarning(
                "SC_EnemyObjectPoolManagerが存在しません。"
            );

            return;
        }

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning(
                "Playerタグのオブジェクトが見つかりません。"
            );

            return;
        }

        GameObject effectObject =
            SC_EnemyObjectPoolManager.Instance.GetObject(
                SC_EnemyObjectPoolManager.EnemyPoolType.ArrivalLight,
                player.transform.position,
                Quaternion.identity
            );

        if (effectObject == null)
        {
            return;
        }

        SC_StageArrivalLight effect =
            effectObject.GetComponent<SC_StageArrivalLight>();

        if (effect == null)
        {
            Debug.LogError(
                "PF_ArrivalLightにSC_StageArrivalLightがありません。",
                effectObject
            );

            effectObject.SetActive(false);
            return;
        }

        effect.StartEffect(player.transform);
    }

    private void StartEffect(Transform playerTransform)
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }

        playCoroutine =
            StartCoroutine(PlayCoroutine(playerTransform));
    }

    private IEnumerator PlayCoroutine(Transform playerTransform)
    {
        if (beamRenderer != null)
        {
            beamRenderer.enabled = true;
        }

        SetAlpha(defaultAlpha);

        Vector3 playerPosition = playerTransform.position;

        float groundY =
            playerPosition.y + groundOffset;

        /*
         * 円柱の開始上端。
         * Playerの上空から光が伸び始める。
         */
        float topY =
            groundY + startHeight;

        /*
         * 1段階目
         * 上端を固定したまま、
         * 下端だけ地面まで伸ばす。
         */
        transform.position = new Vector3(
            playerPosition.x,
            topY,
            playerPosition.z
        );

        transform.localScale = new Vector3(
            defaultScale.x,
            0f,
            defaultScale.z
        );

        float timer = 0f;

        while (timer < descendTime)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / Mathf.Max(descendTime, 0.0001f)
            );

            float easedT =
                1f - Mathf.Pow(1f - t, 3f);

            float currentBottomY =
                Mathf.Lerp(
                    topY,
                    groundY,
                    easedT
                );

            float currentHeight =
                topY - currentBottomY;

            float currentCenterY =
                currentBottomY + currentHeight * 0.5f;

            transform.position = new Vector3(
                playerPosition.x,
                currentCenterY,
                playerPosition.z
            );

            /*
             * Unity標準Cylinderは高さ2なので、
             * Scale.y = 高さの半分。
             */
            transform.localScale = new Vector3(
                defaultScale.x,
                currentHeight * 0.5f,
                defaultScale.z
            );

            yield return null;
        }

        /*
         * 地面まで完全に伸びた状態。
         */
        float fullHeight =
            topY - groundY;

        transform.position = new Vector3(
            playerPosition.x,
            groundY + fullHeight * 0.5f,
            playerPosition.z
        );

        transform.localScale = new Vector3(
            defaultScale.x,
            fullHeight * 0.5f,
            defaultScale.z
        );

        yield return new WaitForSeconds(holdTime);

        /*
         * 2段階目
         * 下端を地面に固定したまま、
         * 上端を下へ落として高さを0にする。
         */
        timer = 0f;

        while (timer < collapseTime)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / Mathf.Max(collapseTime, 0.0001f)
            );

            float easedT =
                t * t * (3f - 2f * t);

            float currentHeight =
                Mathf.Lerp(
                    fullHeight,
                    0f,
                    easedT
                );

            float currentCenterY =
                groundY + currentHeight * 0.5f;

            transform.position = new Vector3(
                playerPosition.x,
                currentCenterY,
                playerPosition.z
            );

            transform.localScale = new Vector3(
                defaultScale.x,
                currentHeight * 0.5f,
                defaultScale.z
            );

            yield return null;
        }

        transform.localScale = new Vector3(
            defaultScale.x,
            0f,
            defaultScale.z
        );

        transform.position = new Vector3(
            playerPosition.x,
            groundY,
            playerPosition.z
        );

        playCoroutine = null;
        ReturnToPool();
    }

    private void SetAlpha(float alpha)
    {
        if (beamMaterial == null)
        {
            return;
        }

        if (!beamMaterial.HasProperty(AlphaProperty))
        {
            return;
        }

        beamMaterial.SetFloat(
            AlphaProperty,
            alpha
        );
    }

    public void ReturnToPool()
    {
        if (ownerPool != null)
        {
            ownerPool.ReturnObject(gameObject);
        }
        else
        {
            Debug.LogWarning(
                "ArrivalLightのownerPoolが設定されていません。",
                this
            );

            gameObject.SetActive(false);
        }
    }
}