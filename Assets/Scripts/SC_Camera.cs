using UnityEngine;

public class CS_Camera : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform followTarget;

    private Transform lookAtTarget;

    [Header("Normal Camera")]
    [SerializeField] private Vector3 normalOffset = new Vector3(0f, 3f, -6f);
    [SerializeField] private Vector3 normalLookOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Lock-On Camera")]
    [SerializeField] private float lockOnDistanceScale = 0.82f;
    [SerializeField] private float lockOnHeightOffset = 0.15f;
    [SerializeField] private float lockOnLookHeight = 1.2f;

    [Header("Strong Attack Shoulder Camera")]
    [SerializeField] private float strongAttackBackDistance = 2.2f;
    [SerializeField] private float strongAttackHeight = 1.85f;
    [SerializeField] private float strongAttackSideOffset = 0.75f;
    [SerializeField] private float strongAttackLookHeight = 1.25f;
    [SerializeField] private float strongAttackFallbackLookDistance = 4.0f;
    [SerializeField] private float strongAttackBlendInSpeed = 12f;
    [SerializeField] private float strongAttackBlendOutSpeed = 4f;

    [Header("Look Input")]
    [SerializeField] private float sensitivity = 120f;
    private float yaw;
    private float pitch;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.08f;
    [SerializeField] private float lookSmoothTime = 0.06f;
    [SerializeField] private float rotationSmoothSpeed = 12f;

    private bool isAttackMode;

    private bool strongAttackCutInRequested;
    private float strongAttackCutInWeight;

    private Vector3 strongAttackCutInForward = Vector3.forward;
    private Vector3 strongAttackCutInRight = Vector3.right;

    private bool enableShake;
    private float shakeTimer;
    private float shakeDuration;
    private float shakePower;

    private Vector3 positionVelocity;
    private Vector3 lookPointVelocity;
    private Vector3 currentLookPoint;

    public bool IsStrongAttackCutInCameraActive => strongAttackCutInRequested || strongAttackCutInWeight > 0.0001f;
    public bool IsStrongAttackCutInFullyBlended => strongAttackCutInWeight >= 0.98f;

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    public void SetAttackMode(bool enable)
    {
        isAttackMode = enable;
    }

    public void SetLookAtTarget(Transform target)
    {
        lookAtTarget = target;
    }

    public void StartShake(float duration, float power)
    {
        if (IsStrongAttackCutInCameraActive)
            return;

        enableShake = true;
        shakeDuration = Mathf.Max(0.0001f, duration);
        shakePower = power;
        shakeTimer = shakeDuration;
    }

    public void AddLookInput(Vector2 input)
    {
        float dt = Time.deltaTime;
        yaw += input.x * sensitivity * dt;
        pitch += input.y * sensitivity * dt;
        pitch = Mathf.Clamp(pitch, -10f, 35f);
    }

    public void StartStrongAttackCutIn()
    {
        if (followTarget == null)
            return;

        Vector3 toTarget = Vector3.zero;

        if (lookAtTarget != null)
            toTarget = lookAtTarget.position - followTarget.position;

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

        enableShake = false;
        shakeTimer = 0f;
        strongAttackCutInRequested = true;
    }

    public void EndStrongAttackCutIn()
    {
        strongAttackCutInRequested = false;
    }

    private void LateUpdate()
    {
        if (followTarget == null)
            return;

        bool useRealtimeCamera = IsStrongAttackCutInCameraActive;

        float dt = useRealtimeCamera ? Time.unscaledDeltaTime : Time.deltaTime;

        UpdateStrongAttackCutInState(dt);

        Vector3 desiredPos = GetDesiredPosition();
        Vector3 desiredLookPoint = GetDesiredLookPosition();

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref positionVelocity,
            Mathf.Max(0.0001f, positionSmoothTime),
            Mathf.Infinity,
            dt
        );

        currentLookPoint = Vector3.SmoothDamp(
            currentLookPoint == Vector3.zero ? desiredLookPoint : currentLookPoint,
            desiredLookPoint,
            ref lookPointVelocity,
            Mathf.Max(0.0001f, lookSmoothTime),
            Mathf.Infinity,
            dt
        );

        Vector3 lookDir = currentLookPoint - transform.position;
        if (lookDir.sqrMagnitude <= 0.0001f)
            lookDir = transform.forward;

        Quaternion desiredRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        float rotLerp = 1f - Mathf.Exp(-rotationSmoothSpeed * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotLerp);

        UpdateShake(dt);
    }

    private void UpdateStrongAttackCutInState(float dt)
    {
        float targetWeight = strongAttackCutInRequested ? 1f : 0f;
        float speed = strongAttackCutInRequested ? strongAttackBlendInSpeed : strongAttackBlendOutSpeed;

        strongAttackCutInWeight = Mathf.MoveTowards(
            strongAttackCutInWeight,
            targetWeight,
            speed * dt
        );
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 basePos = isAttackMode && lookAtTarget != null
            ? GetLockOnZoomPosition()
            : GetNormalPosition();

        if (strongAttackCutInWeight <= 0.0001f)
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

        if (strongAttackCutInWeight <= 0.0001f)
            return baseLookPos;

        Vector3 cutInLookPos;

        if (lookAtTarget != null)
        {
            cutInLookPos = lookAtTarget.position + Vector3.up * strongAttackLookHeight;
        }
        else
        {
            cutInLookPos =
                followTarget.position
                + strongAttackCutInForward * strongAttackFallbackLookDistance
                + Vector3.up * strongAttackLookHeight;
        }

        return Vector3.Lerp(baseLookPos, cutInLookPos, strongAttackCutInWeight);
    }

    private void UpdateShake(float dt)
    {
        if (IsStrongAttackCutInCameraActive)
        {
            enableShake = false;
            shakeTimer = 0f;
            return;
        }

        if (!enableShake || shakeTimer <= 0f)
            return;

        float t = shakeTimer / Mathf.Max(0.0001f, shakeDuration);
        float power = shakePower * t;

        transform.position += Random.insideUnitSphere * power;

        shakeTimer -= dt;

        if (shakeTimer <= 0f)
        {
            enableShake = false;
            shakeTimer = 0f;
        }
    }
}