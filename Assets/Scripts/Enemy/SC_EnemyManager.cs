using System.Collections.Generic;
using UnityEngine;

public class SC_EnemyManager : MonoBehaviour
{
    private List<GameObject> enemies =
        new List<GameObject>();

    private GameObject player;

    void Start()
    {
        player =
            GameObject.FindGameObjectWithTag("Player");
    }

    // Enemy登録
    public void AddEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }

    // Enemy全削除
    public void ClearEnemies()
    {
        enemies.Clear();
    }

    // Enemy数
    public int GetEnemyCount()
    {
        enemies.RemoveAll(e => e == null);

        return enemies.Count;
    }

    // Enemyリスト
    public List<GameObject> GetEnemies()
    {
        enemies.RemoveAll(e => e == null);

        return enemies;
    }

    // 一番近いEnemy
    public GameObject GetNearestEnemy()
    {
        SortEnemiesByPlayerDistance();

        if (enemies.Count <= 0)
            return null;

        return enemies[0];
    }

    // Enemy座標
    public List<Vector3> GetEnemyPositions()
    {
        List<Vector3> list =
            new List<Vector3>();

        foreach (var e in enemies)
        {
            if (e != null)
            {
                list.Add(
                    e.transform.position);
            }
        }

        return list;
    }

    // 距離順ソート
    private void SortEnemiesByPlayerDistance()
    {
        if (player == null) return;

        enemies.RemoveAll(e => e == null);

        Vector3 playerPos =
            player.transform.position;

        enemies.Sort((a, b) =>
        {
            float distA =
                Vector3.SqrMagnitude(
                    a.transform.position - playerPos);

            float distB =
                Vector3.SqrMagnitude(
                    b.transform.position - playerPos);

            return distA.CompareTo(distB);
        });
    }
}