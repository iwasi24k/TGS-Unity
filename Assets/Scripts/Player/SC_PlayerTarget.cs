using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerTarget : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("メインカメラ"), SerializeField] private Camera goMainCamera;
    [Tooltip("ターゲットトグル用入力"), SerializeField] private InputActionReference iaTarget;
    [Tooltip("ターゲット変更用入力"), SerializeField] private InputActionReference iaTargetChange;

    // ターゲット中かどうかのフラグ
    private bool isTargeting = false;

    private GameObject currentTarget;
    private GameObject[] targets;
    private GameObject[] enemys;
    private int targetIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (currentTarget != null) currentTarget = null;
        if (goMainCamera == null) goMainCamera = Camera.main;

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
        enemys = GameObject.FindGameObjectsWithTag("Enemy");
        targets = GetTargetsInView() != null ? GetTargetsInView().ToArray() : enemys;
    }

    private void LateUpdate()
    {
        //ターゲットがいなくなったら、切り替え
        if (isTargeting)
        {
            if (currentTarget == null && targets.Length > 0) 
            {
                UpdateEnemys();
                currentTarget = targets[targetIndex];
                return;
            }

            if (targets.Length == 0)
            {
                isTargeting = false;
                currentTarget = null;
                return;
            }
        }
        else
        {
            currentTarget = null;
        }
    }

    private GameObject GetTargetInView()
    {
        if (enemys == null || enemys.Length == 0)
            return null;

        Camera cam = goMainCamera != null ? goMainCamera : Camera.main;
        Plane[] planes = cam != null ? GeometryUtility.CalculateFrustumPlanes(cam) : null;

        var inView = new System.Collections.Generic.List<GameObject>();
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

            if (isVisible)
                inView.Add(enemy);
        }

        if (inView.Count == 0)
            return null;

        // プレイヤーからの距離で昇順ソート（処理軽量化のため平方距離を使用）
        inView.Sort((a, b) =>
        {
            float da = Vector3.SqrMagnitude(a.transform.position - myPos);
            float db = Vector3.SqrMagnitude(b.transform.position - myPos);
            return da.CompareTo(db);
        });

        // targetIndex を 0 始まりとして扱う（範囲外なら最後尾にクランプ）
        int idx = Mathf.Clamp(targetIndex, 0, enemys.Length);
        return inView[idx];
    }

    public void ChangeTarget(int direction)
    {
        if (isTargeting)
        {
            targetIndex += direction;
            if (targetIndex < 0) targetIndex = targets.Length - 1;
            else if (targetIndex >= targets.Length) targetIndex = 0;
            currentTarget = targets[targetIndex];
        }
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    //emenysを更新するための関数
    private void UpdateEnemys()
    {
        if (enemys == null || enemys.Length == 0)
            return;

        targetIndex = 0;

        //プレイヤーとの距離が近い順にソート  
        System.Array.Sort(enemys, (a, b) =>
        {
            float da = Vector3.SqrMagnitude(a.transform.position - transform.position);
            float db = Vector3.SqrMagnitude(b.transform.position - transform.position);
            return da.CompareTo(db);
        });
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

            if (isVisible)
                inView.Add(enemy);
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

}