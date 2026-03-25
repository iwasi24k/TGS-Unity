using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerMove : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private CharacterController ccPlayer;
    [Tooltip("移動用インプットアクション")]
    [SerializeField]private InputActionReference iaMove;
    [Tooltip("スプリント用インプットアクション")]
    [SerializeField] private InputActionReference iaSprint;

    [Header("Settings")]
    [Tooltip("移動速度")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("回転速度")]
    [SerializeField] private float rotationSpeed = 10f;
    [Tooltip("スプリント速度倍率")]
    [SerializeField] private float sprintMultiplier = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(ccPlayer == null) ccPlayer = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        var MoveInput = iaMove.action.ReadValue<Vector2>();

        Vector3 moveDir = new Vector3(MoveInput.x, 0, MoveInput.y);

        //進行方向に身体を向ける
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Vector3 lookDir = moveDir.normalized;
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        //スプリント
        var sprintInput = iaSprint.action.ReadValue<float>();
        
        Debug.Log($"Sprint Input: {sprintInput}");
        float SprintFactor = sprintInput > 0.5f ? sprintMultiplier : 1f;

        //移動
        ccPlayer.Move(moveDir.normalized * (moveSpeed * SprintFactor) * Time.deltaTime); 
    }
}
