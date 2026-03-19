using System.Security.Claims;
using UnityEngine;
using UnityEngine.InputSystem;
public class SC_Player : MonoBehaviour
{

    [SerializeField] CharacterController cController;

    [SerializeField] float moveSpeed = 10f;

    private InputAction moveInput;

    [Header("DeadZoon")]
    public float DeadZoonX = 0.2f;
    public float DeadZoonY = 0.2f;

    private Vector3 moveDirection = Vector3.zero;

    //--------------------------
    //カメラ
    private CS_Camera cam;
    // カメラテスト用（攻撃、肩越し、シェイクの入力アクション）
    private InputAction attackInput;
    private InputAction shakeInput;
    private InputAction lookInput;
    private Vector2 lookDelta;
    // 仮の攻撃対象用のTransform
    private Transform targetTransform;
    //--------------------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveInput = InputSystem.actions.FindAction("Move");

        //--------------------------
        attackInput = InputSystem.actions.FindAction("Attack");
        shakeInput = InputSystem.actions.FindAction("Interact");
        lookInput = InputSystem.actions.FindAction("Look");
        //--------------------------

        if (cController == null) cController = GetComponent<CharacterController>();

        //--------------------------
        // メインカメラから取得
        cam = Camera.main.GetComponent<CS_Camera>();
        // カメラの追従対象を自分に設定
        cam.SetFollowTarget(transform);
        // 仮の攻撃対象用のTransformを作成
        GameObject obj = new GameObject("DummyTarget");
        targetTransform = obj.transform;
        //--------------------------
    }

    // Update is called once per frame
    void Update()
    {
        var moveValue = moveInput.ReadValue<Vector2>();

        //横軸移動デッドゾーン
        if (Mathf.Abs(moveValue.x) < DeadZoonX)
        {
            moveValue.x = 0;
        }
        //縦軸移動デッドゾーン
        if (Mathf.Abs(moveValue.y) < DeadZoonY)
        {
            moveValue.y = 0;
        }

        var InputMag  = moveValue.magnitude;

        if(InputMag > 0f)
        {
            moveDirection = new Vector3(moveValue.x, 0, moveValue.y).normalized;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        cController.Move(moveDirection * moveSpeed * Time.deltaTime);


        //--------------------------
        // Enter押してる間 → 攻撃モード(肩越し固定でターゲットに向く)
        bool isAttacking = attackInput.IsPressed();
        cam.SetAttackMode(isAttacking);
        //仮の攻撃対象（プレイヤーの前方2m）
        targetTransform.position = transform.position + transform.forward * 10.0f;
        cam.SetLookAtTarget(targetTransform);

        // E押した瞬間 → シェイク
        if (shakeInput.WasPressedThisFrame())
        {
            cam.StartShake(0.2f, 0.4f);
        }

        // マウス又は右スティックの移動量をカメラに渡す
        lookDelta = lookInput.ReadValue<Vector2>();
        cam.AddLookInput(lookDelta);
        //--------------------------
    }
}
