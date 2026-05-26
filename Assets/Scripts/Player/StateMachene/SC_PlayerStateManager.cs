using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public struct PlayerState
{
    public SC_PlayerBaseState Idle;
    public SC_PlayerBaseState Move;
    public SC_PlayerBaseState WeakAttack;
    public SC_PlayerBaseState StrongAttack;
}

public class SC_PlayerStateManager : MonoBehaviour
{
    [Header("References")]
    [field: SerializeField] public CharacterController cController { get; private set; }
    [field: SerializeField] public Animator animator { get; private set; }

    [Header("Input Actions")]
    [field: SerializeField] public InputActionReference moveInput { get; private set; }
    [field: SerializeField] public InputActionReference weakAttackInput { get; private set; }
    [field: SerializeField] public InputActionReference strongAttackInput { get; private set; }
    [field: SerializeField] public InputActionReference sprintInput { get; private set; }

    [Header("State Settings")]
    [SerializeField] private SC_PlayerBaseState idle;
    [SerializeField] private SC_PlayerBaseState move;
    [SerializeField] private SC_PlayerBaseState weakAttack;
    [SerializeField] private SC_PlayerBaseState strongAttack;

    private PlayerState stateList;

    private SC_PlayerBaseState _currentState;

    // Describe the getter for each state pattern.

    private void Awake()
    {
        if(cController == null) cController = GetComponent<CharacterController>();
        if(animator == null) animator = GetComponent<Animator>();

        stateList.Idle = idle;
        stateList.Move = move;
        stateList.WeakAttack = weakAttack;
        stateList.StrongAttack = strongAttack;
    }

    private void Start()
    {
        ChangeState(stateList.Idle);
    }

    private void Update()
    {
        _currentState.UpdateState(this.gameObject,stateList);
    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdateState(this.gameObject,stateList);
    }

    public void ChangeState(SC_PlayerBaseState next)
    {
        if(next == null)
        {
            Debug.LogError("Next state is null.");
            return;
        }

        if (_currentState == next) return;

        _currentState?.Exit(this.gameObject,stateList);
        _currentState = next;
        _currentState.Enter(this.gameObject,stateList);
    }
}
