using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerSprintManager : MonoBehaviour
{
    [Header("IA Ref")]
    [SerializeField] private InputActionReference sprintInput; // Reference to the player input script

    [Header("UI Red")]
    [SerializeField] private SC_PlayerSprintUI sprintUI; // Reference to the sprint UI

    [Header("Sprint Settings")]
    [SerializeField] private float sprintSpeedMultiplier = 1.5f; // Multiplier for sprinting speed
    [SerializeField] private float boostSpeedMultiplier = 4f; // Multiplier for boost speed when sprinting
    [SerializeField] private float boostLerpTime = 2f; // Time it takes to transition from boost speed back to normal sprint speed
    [SerializeField] private float boostCoolTime = 10f; // Stamina cost per second while sprinting
    public float GetBoostCoolTime() => boostCoolTime;

    private float _currentBoostCoolTime = 0f; // Current cooldown time remaining
    private float _currentSprintSpeedMultiplier; // Current sprint speed multiplier

    private bool wasPressed = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentSprintSpeedMultiplier = sprintSpeedMultiplier;
        wasPressed = false;
        sprintUI.InitializeUI(this);
    }

    // Update is called once per frame
    void Update()
    {
        if(_currentBoostCoolTime > 0)
        {
            _currentBoostCoolTime -= Time.deltaTime;
        }
        else
        {
            _currentBoostCoolTime = 0; // Ensure it doesn't go below zero
        }
        sprintUI.UpdateUI(this);

        if(_currentSprintSpeedMultiplier > sprintSpeedMultiplier)
        {
            _currentSprintSpeedMultiplier = Mathf.Lerp(_currentSprintSpeedMultiplier, sprintSpeedMultiplier, Time.deltaTime * boostLerpTime);
        }

        if(wasPressed && sprintInput.action.WasReleasedThisFrame())
        {
            wasPressed = false;
        }
    }

    public bool TrySprint()
    {
        if (_currentBoostCoolTime <= 0 && !wasPressed)
        {
            _currentSprintSpeedMultiplier = boostSpeedMultiplier; // Set to boost speed
            _currentBoostCoolTime = boostCoolTime; // Reset the cooldown
            wasPressed = true;
            return true;
        }
        _currentSprintSpeedMultiplier = sprintSpeedMultiplier;

        return false;
    }

    public float GetSprintSpeedMultiplier()
    {
        return _currentSprintSpeedMultiplier;
    }

    public float GetCurrentBoostCoolTime()
    {
        return _currentBoostCoolTime;
    }
}
