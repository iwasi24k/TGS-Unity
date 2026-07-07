using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Minimap : MonoBehaviour
{
    // =========================
    // プレイヤー
    // =========================

    private Transform player;


    // =========================
    // フィールド
    // =========================

    private Transform field;


    // =========================
    // 表示中のUI管理
    // =========================

    private List<GameObject> blips =
        new List<GameObject>();


    // =========================
    // レーダー設定
    // =========================

    [Header("Radar Settings")]

    [Tooltip("感知範囲")]
    [SerializeField] private float radarRange = 30f;

    [SerializeField] private RectTransform radarUI;

    [Tooltip("ミニマップの縮尺（1mあたり何px動かすか）")]
    [SerializeField] private float mapScale = 5f;


    // =========================
    // プレハブ
    // =========================

    [Header("Prefabs")]

    [SerializeField] private GameObject enemyBlipPrefab;

    [SerializeField] private GameObject enemyArrowPrefab;


    // =========================
    // UI参照
    // =========================

    [Header("UI References")]

    [SerializeField] private Transform blipParent;

    [SerializeField] private RectTransform playerArrow;

    [SerializeField] private RectTransform fieldImage;


    // =========================
    // 初期化
    // =========================

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");

        if (p != null)
        {
            player = p.transform;
        }

        GameObject f = GameObject.FindWithTag("Field");

        if (f != null)
        {
            field = f.transform;

            Renderer renderer = field.GetComponent<Renderer>();

            if (renderer != null)
            {
                Vector3 size = renderer.bounds.size;

                fieldImage.sizeDelta = new Vector2(
                    size.x * mapScale,
                    size.z * mapScale
                );
            }
        }
    }


    // =========================
    // 更新処理
    // =========================

    void Update()
    {
        if (player == null) return;


        // =========================
        // プレイヤー矢印回転
        // =========================

        playerArrow.localEulerAngles =
            new Vector3(
                0,
                0,
                -player.localEulerAngles.y
            );


        // =========================
        // フィールド移動
        // =========================

        if (fieldImage != null)
        {
            fieldImage.anchoredPosition =
                new Vector2(
                    -player.position.x * mapScale,
                    -player.position.z * mapScale
                );
        }


        // =========================
        // 既存UI削除
        // =========================

        foreach (var b in blips)
        {
            Destroy(b);
        }

        blips.Clear();


        // =========================
        // Enemyタグ取得
        // =========================

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");


        // レーダー半径
        float radius =
            radarUI.rect.width * 0.35f;


        // =========================
        // 敵ごとの処理
        // =========================

        foreach (var enemy in enemies)
        {
            Vector3 dir =
                enemy.transform.position - player.position;

            float distance = dir.magnitude;

            Vector2 pos =
                new Vector2(dir.x, dir.z);

            if (distance <= radarRange)
            {
                Vector2 radarPos =
                    (pos / radarRange) * radius;

                GameObject blip =
                    Instantiate(
                        enemyBlipPrefab,
                        blipParent
                    );

                RectTransform rt =
                    blip.GetComponent<RectTransform>();

                rt.anchoredPosition =
                    radarPos;

                blips.Add(blip);
            }
            else
            {
                Vector2 dirNormalized =
                    pos.normalized;

                Vector2 edgePos =
                    dirNormalized * radius;

                GameObject arrow =
                    Instantiate(
                        enemyArrowPrefab,
                        blipParent
                    );

                RectTransform rt =
                    arrow.GetComponent<RectTransform>();

                rt.anchoredPosition =
                    edgePos;

                float angle =
                    Mathf.Atan2(
                        dirNormalized.y,
                        dirNormalized.x
                    ) * Mathf.Rad2Deg;

                rt.localEulerAngles =
                    new Vector3(
                        0,
                        0,
                        angle - 90f
                    );

                blips.Add(arrow);
            }
        }
    }
}