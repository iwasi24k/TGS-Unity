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


        // =========================
        // 敵ごとの処理
        // =========================

        foreach (var enemy in enemies)
        {
            // プレイヤー → 敵方向
            Vector3 dir =
                enemy.transform.position
                - player.position;

            // 距離
            float distance = dir.magnitude;

            // XZ平面へ変換
            Vector2 pos =
                new Vector2(dir.x, dir.z);

            // レーダー半径
            float radius =
                radarUI.rect.width * 0.35f;


            // =========================
            // レーダー内
            // =========================

            if (distance <= radarRange)
            {
                // レーダー座標へ変換
                Vector2 radarPos =
                    (pos / radarRange) * radius;

                // 赤点生成
                GameObject blip =
                    Instantiate(
                        enemyBlipPrefab,
                        blipParent
                    );

                // UI位置設定
                RectTransform rt =
                    blip.GetComponent<RectTransform>();

                rt.anchoredPosition =
                    radarPos;

                // 管理リスト追加
                blips.Add(blip);
            }
            // =========================
            // レーダー外
            // =========================
            else
            {
                // 方向だけ取得
                Vector2 dirNormalized =
                    pos.normalized;

                // 円端位置
                Vector2 edgePos =
                    dirNormalized * radius;

                // 矢印生成
                GameObject arrow =
                    Instantiate(
                        enemyArrowPrefab,
                        blipParent
                    );

                RectTransform rt =
                    arrow.GetComponent<RectTransform>();

                // 位置設定
                rt.anchoredPosition =
                    edgePos;

                // 回転角度計算
                float angle =
                    Mathf.Atan2(
                        dirNormalized.y,
                        dirNormalized.x
                    ) * Mathf.Rad2Deg;

                // 矢印回転
                rt.localEulerAngles =
                    new Vector3(
                        0,
                        0,
                        angle - 90f
                    );

                // 管理リスト追加
                blips.Add(arrow);
            }
        }
    }
}