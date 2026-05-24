using UnityEngine;

public class SC_PlayerStateManager : MonoBehaviour
{
    [field: SerializeField] public CharacterController cController { get; private set; }
    [field: SerializeField] public Animator animator { get; private set; }

    [Header("State Settings")]
    [SerializeField] private SC_PlayerBaseState idleState;
    [SerializeField] private SC_PlayerBaseState walkState;
    [SerializeField] private SC_PlayerBaseState attackState;

    private SC_PlayerBaseState _currentState;

    // Describe the getter for each state pattern.

    private void Awake()
    {
        if(cController == null) cController = GetComponent<CharacterController>();
        if(animator == null) animator = GetComponent<Animator>();

        // Instance the defined states.

    }

    private void Start()
    {
        //ChangeState();
    }

    private void Update()
    {
        _currentState.Update();
    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdate();
    }

    public void ChangeState(SC_PlayerBaseState next)
    {
        if(_currentState == next) return;

        _currentState?.Exit();
        _currentState = next;
        _currentState.Enter();
    }
}
