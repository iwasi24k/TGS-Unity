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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveInput = InputSystem.actions.FindAction("Move");
        if (cController == null) cController = GetComponent<CharacterController>();
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
            moveDirection = new Vector3(moveValue.x, 0, moveValue.y).normalized * moveSpeed * Time.deltaTime;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        cController.Move(moveDirection * moveSpeed * Time.deltaTime);


    }
}
