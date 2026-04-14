using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerMove : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Camera goMainCamera;
    [SerializeField] private CharacterController ccPlayer;
    [Tooltip("移動用インプットアクション")]
    [SerializeField]private InputActionReference iaMove;
    [Tooltip("スプリント用インプットアクション")]
    [SerializeField] private InputActionReference iaSprint;
    [Tooltip("ターゲット情報")]
    [SerializeField] SC_PlayerTarget scTarget;

    [Header("Settings")]
    [Tooltip("移動速度")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("回転速度")]
    [SerializeField] private float rotationSpeed = 10f;
    [Tooltip("スプリント速度倍率")]
    [SerializeField] private float sprintMultiplier = 2f;
    [Tooltip("スプリント初期の速度")]
    [SerializeField] private float blinkPower = 5f;
    [Tooltip("ブリンククールダウン時間")]
    [SerializeField] private float blinkCooldown = 5f;

    private bool wasBlink = false;
    private float currentSplintMul = 1f;
    private float BlinkTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(goMainCamera == null) goMainCamera = Camera.main;
        if (ccPlayer == null) ccPlayer = GetComponent<CharacterController>();
        if(scTarget == null) scTarget = this.GetComponent<SC_PlayerTarget>();
        currentSplintMul = sprintMultiplier;

        if(iaMove == null)
        {
            Debug.LogError("移動用のInputActionReferenceがアタッチされていません。");
        }
        if(iaSprint == null)
        {
            Debug.LogError("スプリント用のInputActionReferenceがアタッチされていません。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        var MoveInput = iaMove.action.ReadValue<Vector2>();

        // カメラの向きを XZ 平面に投影してから移動方向を計算
        Vector3 camForward = Vector3.ProjectOnPlane(goMainCamera.transform.forward, Vector3.up);
        Vector3 camRight = Vector3.ProjectOnPlane(goMainCamera.transform.right, Vector3.up);

        // 極端なケース（カメラが真上/真下を向いている等）を保険
        if (camForward.sqrMagnitude < 0.0001f) camForward = Vector3.forward;
        if (camRight.sqrMagnitude < 0.0001f) camRight = Vector3.right;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * MoveInput.y + camRight * MoveInput.x;

        //ターゲット方向に向く
        if (scTarget.GetCurrentTarget() != null)
        {
            Vector3 targetDir = scTarget.GetCurrentTarget().transform.position - transform.position;
            targetDir.y = 0; // 水平方向のみにする

            Quaternion targetRot = Quaternion.LookRotation(targetDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        }

        if (moveDir.sqrMagnitude > 0.001f)
        {

            if (scTarget.GetCurrentTarget() == null)
            { 
                Vector3 lookDir = moveDir.normalized;
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            //スプリント
            var sprintInput = iaSprint.action.ReadValue<float>();
            bool isSprint = sprintInput > 0.5f; // スプリント入力があるかどうかを判定

            if (isSprint && !wasBlink)
            {//スプリントが開始された瞬間にブリンクを試みる
                TryBlink(moveDir);
            }
            else
            {
                BlinkTimer -= Time.deltaTime; // ブリンクのクールダウンを減少させる
            }
            wasBlink = isSprint; // ブリンクの使用状態をリセット

            float SprintFactor = isSprint ? currentSplintMul : 1f;

            //移動
            ccPlayer.Move(moveDir.normalized * (moveSpeed * SprintFactor) * Time.deltaTime);
        }

        // ブリンクの効果がスプリント倍率を超えている場合、徐々に減少させる
        if (currentSplintMul > sprintMultiplier)
        {
            currentSplintMul = Mathf.Lerp(currentSplintMul, sprintMultiplier, Time.deltaTime * 3f); // ブリンクの効果を徐々に減少させる
        }
    }

    private void TryBlink(Vector3 moveDir)
    {
        if (BlinkTimer <= 0.0f)
        {
            currentSplintMul = blinkPower; // ブリンクの効果でスプリント倍率を一時的に上げる
            BlinkTimer = blinkCooldown; // ブリンクのクールダウンをリセット
        }
        else
        {
            Debug.Log("ブリンクはクールダウン中です。残り時間: " + BlinkTimer.ToString("F2") + "秒");
        }
    }
}
