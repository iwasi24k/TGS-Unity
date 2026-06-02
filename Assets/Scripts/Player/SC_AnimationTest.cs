using UnityEngine;
using UnityEngine.InputSystem;

public class SC_AnimationTest : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference weakLeft;
    [SerializeField] private InputActionReference weakRight;
    [SerializeField] private InputActionReference strong;

    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(animator == null) return;

        if (weakLeft && weakLeft.action.WasPressedThisFrame())
        {
            Debug.Log("Weak Left");
            animator.SetTrigger("tWeakLeft");
        }
        if(weakRight && weakRight.action.WasPressedThisFrame())
        {
            Debug.Log("Weak Right");
            animator.SetTrigger("tWeakRight");
        }
        if(strong && strong.action.WasPressedThisFrame())
        {
            Debug.Log("Strong");
            animator.SetTrigger("tStrong");
        }
    }
}
