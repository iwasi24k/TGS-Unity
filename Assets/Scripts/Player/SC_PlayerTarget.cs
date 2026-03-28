using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerTarget : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("メインカメラ"),SerializeField] private Camera goMainCamera;
    [Tooltip("ターゲットトグル用入力"),SerializeField] private InputActionReference iaTarget;

    [Header("Don't Attach from Inspector")]
    public GameObject currentTarget;
    
    private GameObject[] enemys;
    private int targetIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(currentTarget != null) currentTarget = null;
        if(goMainCamera == null) goMainCamera = Camera.main;

        if (iaTarget == null)
        {
            Debug.LogError("ターゲットトグル用のInputActionReferenceがアタッチされていません。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        var targetInput = iaTarget.action.WasPressedThisFrame();

        if(targetInput)
        {
            if(currentTarget == null)
            {
                currentTarget = GetTargetInView();
            }
            else
            {
                //ターゲットがいる場合、ターゲットを解除する
                currentTarget = null;
            }
        }
    }

    private void FixedUpdate()
    {
        enemys = GameObject.FindGameObjectsWithTag("Enemy");
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

}
