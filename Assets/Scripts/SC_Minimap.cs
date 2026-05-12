using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Minimap : MonoBehaviour
{
    public Transform player;
    public float radarRange = 30f;

    public RectTransform radarUI;
    public GameObject enemyBlipPrefab;
    public Transform blipParent;

    private List<GameObject> blips = new List<GameObject>();

    public RectTransform playerArrow;

    void Update()
    {
        if (player == null) return;

        playerArrow.localEulerAngles =
            new Vector3(0, 0, -player.localEulerAngles.y);

        // 既存の点を削除
        foreach (var b in blips)
        {
            Destroy(b);
        }
        blips.Clear();

        // Enemyタグを取得
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            Vector3 dir = enemy.transform.position - player.position;
            float distance = dir.magnitude;

            // 範囲外は無視
            if (distance > radarRange) continue;

            // 2D化（XZ平面）
            Vector2 pos = new Vector2(dir.x, dir.z) / radarRange;

            // レーダー内の座標に変換
            float radius = radarUI.rect.width / 2f;
            Vector2 radarPos = pos * radius;

            // 点を生成
            GameObject blip = Instantiate(enemyBlipPrefab, blipParent);
            RectTransform rt = blip.GetComponent<RectTransform>();
            rt.anchoredPosition = radarPos;

            blips.Add(blip);
        }
    }
}
