using UnityEngine;

public class CS_Camera : MonoBehaviour
{
    [Header("Follow Target (Player)")]
    [SerializeField] private Transform followTarget;

    [Header("Look Target (Lock-On)")]
    private Transform lookAtTarget;

    [Header("Normal Camera")]
    [SerializeField] private Vector3 normalOffset = new Vector3(0f, 3f, -6f);
    [SerializeField] private Vector3 normalLookOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Lock-On Camera (Zoom In Only)")]
    [SerializeField] private float lockOnDistanceScale = 0.82f;
    [SerializeField] private float lockOnHeightOffset = 0.15f;
    [SerializeField] private float lockOnLookHeight = 1.2f;

    [Header("Strong Attack Cut-In Camera")]
    [SerializeField] private float strongAttackBackDistance = 2.2f;
    [SerializeField] private float strongAttackHeight = 1.85f;
    [SerializeField] private float strongAttackSideOffset = 0.75f;
    [SerializeField] private float strongAttackLookHeight = 1.25f;
    [SerializeField] private float strongAttackCutInDuration = 0.22f;
    [SerializeField] private float strongAttackBlendInSpeed = 12f;
    [SerializeField] private float strongAttackBlendOutSpeed = 10f;

    [Header("Look Input")]
    [SerializeField] private float sensitivity = 120f;
    private float yaw = 0f;
    private float pitch = 0f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.08f;
    [SerializeField] private float rotationSmoothSpeed = 12f;

    private bool isAttackMode = false;

    private bool strongAttackCutInActive = false;
    private float strongAttackCutInTimer = 0f;
    private float strongAttackCutInWeight = 0f;

    private Vector3 strongAttackCutInForward = Vector3.forward;
    private Vector3 strongAttackCutInRight = Vector3.right;

    private bool enableShake = false;
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private float shakePower = 0f;

    private Vector3 positionVelocity;

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    public void SetAttackMode(bool enable)
    {
        isAttackMode = enable;

        if (!isAttackMode)
        {
            strongAttackCutInActive = false;
            strongAttackCutInTimer = 0f;
        }
    }

    public void SetLookAtTarget(Transform target)
    {
        lookAtTarget = target;

        if (lookAtTarget == null)
        {
            strongAttackCutInActive = false;
            strongAttackCutInTimer = 0f;
        }
    }

    public void StartShake(float duration, float power)
    {
        enableShake = true;
        shakeDuration = duration;
        shakePower = power;
        shakeTimer = duration;
    }

    public void AddLookInput(Vector2 input)
    {
        yaw += input.x * sensitivity * Time.deltaTime;
        pitch += input.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -10f, 35f);
    }

    public void StartStrongAttackCutIn()
    {
        if (!isAttackMode || lookAtTarget == null || followTarget == null)
            return;

        Vector3 toTarget = lookAtTarget.position - followTarget.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.0001f)
            toTarget = followTarget.forward;

        if (toTarget.sqrMagnitude <= 0.0001f)
            toTarget = Vector3.forward;

        strongAttackCutInForward = toTarget.normalized;

        Vector3 right = Vector3.Cross(Vector3.up, strongAttackCutInForward).normalized;
        if (right.sqrMagnitude <= 0.0001f)
            right = transform.right;

        Vector3 fromPlayerToCamera = transform.position - followTarget.position;
        fromPlayerToCamera.y = 0f;

        float sideSign = Vector3.Dot(fromPlayerToCamera, right) >= 0f ? 1f : -1f;
        strongAttackCutInRight = right * sideSign;

        strongAttackCutInActive = true;
        strongAttackCutInTimer = strongAttackCutInDuration;
    }

    public void StopStrongAttackCutIn()
    {
        strongAttackCutInActive = false;
        strongAttackCutInTimer = 0f;
    }

    private void LateUpdate()
    {
        if (followTarget == null)
            return;

        UpdateStrongAttackCutInState();

        Vector3 desiredPos = GetDesiredPosition();
        Quaternion desiredRot = GetDesiredRotation(desiredPos);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref positionVelocity,
            Mathf.Max(0.0001f, positionSmoothTime)
        );

        float rotLerp = 1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotLerp);

        UpdateShake();
    }

    private void UpdateStrongAttackCutInState()
    {
        if (!isAttackMode || lookAtTarget == null)
        {
            strongAttackCutInActive = false;
            strongAttackCutInTimer = 0f;
        }

        if (strongAttackCutInActive)
        {
            strongAttackCutInTimer -= Time.deltaTime;
            if (strongAttackCutInTimer <= 0f)
            {
                strongAttackCutInActive = false;
                strongAttackCutInTimer = 0f;
            }
        }

        float targetWeight = strongAttackCutInActive ? 1f : 0f;
        float speed = strongAttackCutInActive ? strongAttackBlendInSpeed : strongAttackBlendOutSpeed;
        strongAttackCutInWeight = Mathf.MoveTowards(
            strongAttackCutInWeight,
            targetWeight,
            speed * Time.deltaTime
        );
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 basePos;

        if (isAttackMode && lookAtTarget != null)
            basePos = GetLockOnZoomPosition();
        else
            basePos = GetNormalPosition();

        if (strongAttackCutInWeight <= 0.0001f || !isAttackMode || lookAtTarget == null)
            return basePos;

        Vector3 cutInPos = GetStrongAttackCutInPosition();
        return Vector3.Lerp(basePos, cutInPos, strongAttackCutInWeight);
    }

    private Vector3 GetNormalPosition()
    {
        Quaternion cameraRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 rotatedOffset = cameraRot * normalOffset;
        return followTarget.position + rotatedOffset;
    }

    private Vector3 GetLockOnZoomPosition()
    {
        Quaternion cameraRot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 zoomOffset = normalOffset * lockOnDistanceScale;
        zoomOffset.y += lockOnHeightOffset;

        Vector3 rotatedOffset = cameraRot * zoomOffset;
        return followTarget.position + rotatedOffset;
    }

    private Vector3 GetStrongAttackCutInPosition()
    {
        Vector3 basePos = followTarget.position + Vector3.up * strongAttackHeight;
        return basePos
             - strongAttackCutInForward * strongAttackBackDistance
             + strongAttackCutInRight * strongAttackSideOffset;
    }

    private Quaternion GetDesiredRotation(Vector3 cameraPos)
    {
        Vector3 lookPos = GetDesiredLookPosition();
        Vector3 lookDir = lookPos - cameraPos;

        if (lookDir.sqrMagnitude <= 0.0001f)
            lookDir = transform.forward;

        return Quaternion.LookRotation(lookDir.normalized, Vector3.up);
    }

    private Vector3 GetDesiredLookPosition()
    {
        Vector3 baseLookPos;

        if (isAttackMode && lookAtTarget != null)
        {
            Vector3 playerPos = followTarget.position + Vector3.up * lockOnLookHeight;
            Vector3 targetPos = lookAtTarget.position + Vector3.up * lockOnLookHeight;
            baseLookPos = (playerPos + targetPos) * 0.5f;
        }
        else
        {
            baseLookPos = followTarget.position + normalLookOffset;
        }

        if (strongAttackCutInWeight <= 0.0001f || !isAttackMode || lookAtTarget == null)
            return baseLookPos;

        Vector3 cutInLookPos = lookAtTarget.position + Vector3.up * strongAttackLookHeight;
        return Vector3.Lerp(baseLookPos, cutInLookPos, strongAttackCutInWeight);
    }

    private void UpdateShake()
    {
        if (!enableShake || shakeTimer <= 0f)
            return;

        float t = shakeTimer / Mathf.Max(0.0001f, shakeDuration);
        float power = shakePower * t;

        transform.position += Random.insideUnitSphere * power;

        shakeTimer -= Time.deltaTime;

        if (shakeTimer <= 0f)
            enableShake = false;
    }
}