using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_SimpleRadar : MonoBehaviour
{
    public Transform player;
    public float radarRange = 30f;

    public RectTransform radarUI;
    public GameObject enemyBlipPrefab;
    public Transform blipParent;

    private List<GameObject> blips = new List<GameObject>();

    void Update()
    {
        if (player == null) return;

        foreach (var b in blips)
            Destroy(b);

        blips.Clear();

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            Vector3 dir = enemy.transform.position - player.position;
            float distance = dir.magnitude;

            if (distance > radarRange) continue;

            Vector2 pos = new Vector2(dir.x, dir.z) / radarRange;

            float radius = radarUI.rect.width * 0.5f;

            GameObject blip = Instantiate(enemyBlipPrefab, blipParent);

            var rt = blip.GetComponent<RectTransform>();
            rt.anchoredPosition = pos * radius;

            blips.Add(blip);
        }
    }
}