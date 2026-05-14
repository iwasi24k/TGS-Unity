using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerTarget : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("メインカメラ"), SerializeField] private Camera goMainCamera;
    [Tooltip("ターゲットトグル用入力"), SerializeField] private InputActionReference iaTarget;
    [Tooltip("ターゲット変更用入力"), SerializeField] private InputActionReference iaTargetChange;
    [Tooltip("フィールド管理"), SerializeField] private SC_Field field;

    [Header("Target")]
    [Tooltip("ターゲット切り替え距離"), SerializeField] private float targetLockDistance = 15.0f;
    [Tooltip("ターゲット解除距離"), SerializeField] private float targetReleaseDistance = 20.0f;

    // ターゲット中かどうかのフラグ
    private bool isTargeting = false;

    private GameObject currentTarget;
    private GameObject[] targets = new GameObject[0];
    private GameObject[] enemys = new GameObject[0];
    private int targetIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (currentTarget != null) currentTarget = null;
        if (goMainCamera == null) goMainCamera = Camera.main;

        if (field == null)
        {
            field = FindFirstObjectByType<SC_Field>();
        }
        if (iaTarget == null)
        {
            Debug.LogError("ターゲットトグル用のInputActionReferenceがアタッチされていません。");
        }
        if (iaTargetChange == null)
        {
            Debug.LogError("ターゲット変更用のInputActionReferenceがアタッチされていません。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        var targetInput = iaTarget.action.WasPressedThisFrame();

        if (targetInput)
        {
            if (currentTarget == null)
            {
                targetIndex = 0;
                isTargeting = true;
            }
            else
            {
                isTargeting = false;
            }
        }

        var targetChangeInput = iaTargetChange.action.WasPressedThisFrame();

        if (targetChangeInput)
        {
            ChangeTarget(1);
        }

    }

    private void FixedUpdate()
    {
        if (field == null)
            return;

        List<GameObject> enemyList = field.GetEnemies();

        if (enemyList == null || enemyList.Count == 0)
        {
            enemys = new GameObject[0];
            targets = new GameObject[0];
            return;
        }

        enemys = enemyList.Where(e => e != null && e.activeInHierarchy).ToArray();

        List<GameObject> inViewTargets = GetTargetsInView();

        if (inViewTargets.Count > 0)
        {
            targets = inViewTargets.ToArray();
        }
        else
        {
            targets = enemys.Where(e => IsInTargetLockDistance(e)).ToArray();
        }

        if (targets.Length == 0)
        {
            targetIndex = 0;
        }
        else if (targetIndex >= targets.Length)
        {
            targetIndex = 0;
        }
    }

    private void LateUpdate()
    {
        if (!isTargeting)
        {
            currentTarget = null;
            return;
        }

        if (targets == null || targets.Length == 0)
        {
            isTargeting = false;
            currentTarget = null;
            return;
        }

        if (currentTarget == null)
        {
            targetIndex = 0;
            currentTarget = targets[targetIndex];
            return;
        }


        // ロック距離ではなく、解除距離で判定する
        if (IsOutOfTargetReleaseDistance(currentTarget))
        {
            isTargeting = false;
            currentTarget = null;
            targetIndex = 0;
            return;
        }
    }


    public void ChangeTarget(int direction)
    {
        if (!isTargeting)
            return;

        if (targets == null || targets.Length == 0)
        {
            currentTarget = null;
            isTargeting = false;
            targetIndex = 0;
            return;
        }

        targetIndex += direction;

        if (targetIndex < 0)
            targetIndex = targets.Length - 1;
        else if (targetIndex >= targets.Length)
            targetIndex = 0;

        currentTarget = targets[targetIndex];
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    //カメラに映っている敵を取得する関数
    private List<GameObject> GetTargetsInView()
    {
        var inView = new List<GameObject>();

        if (enemys == null || enemys.Length == 0)
            return inView;

        Camera cam = goMainCamera != null ? goMainCamera : Camera.main;
        Plane[] planes = cam != null ? GeometryUtility.CalculateFrustumPlanes(cam) : null;

        Vector3 myPos = transform.position;

        foreach (GameObject enemy in enemys)
        {
            if (enemy == null || !enemy.activeInHierarchy)
                continue;

            bool isVisible = true;

            if (cam != null)
            {
                Renderer rend = enemy.GetComponentInChildren<Renderer>();

                if (rend != null)
                {
                    if (!GeometryUtility.TestPlanesAABB(planes, rend.bounds))
                    {
                        isVisible = false;
                    }
                    else
                    {
                        Vector3 vp = cam.WorldToViewportPoint(rend.bounds.center);

                        if (vp.z <= 0f || vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f)
                            isVisible = false;
                    }
                }
                else
                {
                    Vector3 vp = cam.WorldToViewportPoint(enemy.transform.position);

                    if (vp.z <= 0f || vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f)
                        isVisible = false;
                }
            }

            if (isVisible && IsInTargetLockDistance(enemy))
            {
                inView.Add(enemy);
            }
        }

        // プレイヤーから近い順に並べる
        inView.Sort((a, b) =>
        {
            float da = Vector3.SqrMagnitude(a.transform.position - myPos);
            float db = Vector3.SqrMagnitude(b.transform.position - myPos);
            return da.CompareTo(db);
        });

        return inView;
    }


    private bool IsInTargetLockDistance(GameObject target)
    {
        if (target == null)
            return false;

        float sqrDistance =
            Vector3.SqrMagnitude(target.transform.position - transform.position);

        return sqrDistance <= targetLockDistance * targetLockDistance;
    }

    private bool IsOutOfTargetReleaseDistance(GameObject target)
    {
        if (target == null)
            return true;

        float sqrDistance =
            Vector3.SqrMagnitude(target.transform.position - transform.position);

        return sqrDistance > targetReleaseDistance * targetReleaseDistance;
    }

}