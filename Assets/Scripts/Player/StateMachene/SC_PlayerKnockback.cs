using UnityEngine;

public class SC_PlayerKnockback : MonoBehaviour
{
    [Header("Refarence")]
    [SerializeField] private MonoBehaviour stopScript;
    [SerializeField] private CharacterController cController;
    [SerializeField] private Animator animator;

    [Header("Knockback Settings")]
    [SerializeField] private float verticalPower = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float horizontalDrag = 5f;

    private Vector3 velocity;
    private float knockbackTimer;
    private bool isKnockbackActive = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (!stopScript) stopScript = GetComponent<SC_PlayerStateManager>();
        if (!cController) cController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // ノックバックがアクティブな間は常に Move を呼ぶ（着地判定を正しく得るため）
        if (!isKnockbackActive) return;

        // 移動
        Vector3 move = velocity * Time.deltaTime;
        cController.Move(move);

        // 重力適用
        velocity.y += gravity * Time.deltaTime;

        // 水平減衰
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        horizontal = Vector3.MoveTowards(horizontal, Vector3.zero, horizontalDrag * Time.deltaTime);
        velocity.x = horizontal.x;
        velocity.z = horizontal.z;

        // タイマー更新（0 未満になっても動作は継続して地面に着地するまで待つ）
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
        }

        // タイマーが切れていてかつ地面に着地したらノックバック終了
        if (knockbackTimer <= 0f && cController.isGrounded)
        {
            EndKnockback();
        }
    }

    // direction: ノックバックの水平方向ベクトル
    // power: 水平方向の初速
    // duration: ノックバック制御を強制停止する最短時間（着地待ちで延長される）
    public void AddKnockback(Vector3 direction, float power, float duration ,bool knockup = true)
    {
        if (!cController) cController = GetComponent<CharacterController>();
        if (stopScript)
        {
            stopScript.enabled = false;
        }

        Vector3 dir = direction;
        dir.y = 0f;
        dir = dir.normalized;

        // 初速設定（水平 + 垂直）
        velocity = dir * power;
        if (knockup)
        {
            velocity.y = verticalPower;
        }
        else
        {
            velocity.y = 0f;
        }

        knockbackTimer = Mathf.Max(0f, duration);
        isKnockbackActive = true;
        if(animator.GetBool("bCharge") || animator.GetBool("bStraight"))
        {
            animator.SetBool("bCharge", false);
            animator.SetBool("bStraight", false);
        }

        animator.SetBool("bKnockback", true);
    }

    private void EndKnockback()
    {
        knockbackTimer = 0f;
        velocity = Vector3.zero;
        isKnockbackActive = false;
        if (stopScript)
        {
            stopScript.enabled = true;
        }
        animator.SetBool("bKnockback", false);
    }
}
