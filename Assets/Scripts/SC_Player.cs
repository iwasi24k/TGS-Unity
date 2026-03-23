using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SC_Player : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CS_Camera cameraScript;

    [Header("References")]
    [SerializeField] private CharacterController cController;
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Slider heatSlider;
    [SerializeField] private Animator animator;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference weakAttackAction;
    [SerializeField] private InputActionReference strongAttackAction;
    [SerializeField] private InputActionReference evadeBoostAction;
    [SerializeField] private InputActionReference targetAction;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float boostMoveSpeed = 3.8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float groundedY = -2f;
    [SerializeField, Range(0f, 1f)] private float moveInputDeadZone = 0.15f;

    [Header("Evade / Boost")]
    [SerializeField] private float evadeDistance = 3.2f;
    [SerializeField] private float evadeDuration = 0.18f;
    [SerializeField] private float evadeHoldThreshold = 0.18f;
    [SerializeField, Range(0f, 1f)] private float evadeMinInput = 0.2f;

    [Header("Heat")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float currentHeat = 0f;
    [SerializeField] private float weakHitHeatGain = 18f;
    [SerializeField] private float strongAttackHeatCost = 50f;

    [Header("Weak Attack")]
    [SerializeField] private int weakDamage = 10;
    [SerializeField] private float weakAttackRadius = 1.0f;
    [SerializeField] private float weakAttackForwardOffset = 1.0f;
    [SerializeField] private float weakAttackHeightOffset = 0.8f;
    [SerializeField] private float weakAttackStartup = 0.04f;
    [SerializeField] private float weakAttackCooldown = 0.15f;
    [SerializeField] private float weakKnockbackDistance = 0.55f;
    [SerializeField] private float weakKnockbackDuration = 0.10f;

    [Header("Strong Attack")]
    [SerializeField] private int strongDamage = 20;
    [SerializeField] private float strongAttackRadius = 1.2f;
    [SerializeField] private float strongAttackForwardOffset = 1.1f;
    [SerializeField] private float strongAttackHeightOffset = 0.8f;
    [SerializeField] private float strongAttackStartup = 0.06f;
    [SerializeField] private float strongAttackCooldown = 0.35f;

    [Header("Strong Attack Slow")]
    [SerializeField, Range(0.01f, 1f)] private float strongAttackSlowTimeScale = 0.08f;
    [SerializeField, Range(0.05f, 1f)] private float strongAttackRestoreSlideProgress = 0.28f;
    [SerializeField] private float strongAttackMissRestoreDelayRealtime = 0.06f;

    [Header("Strong Attack Kill Cut In")]
    [SerializeField, Range(0.01f, 1f)] private float strongKillCutInTimeScale = 0.03f;
    [SerializeField] private float strongKillCutInDurationRealtime = 0.35f;

    [Header("Strong Slide Knockback")]
    [SerializeField] private float strongSlideDistance = 4.2f;
    [SerializeField] private float strongSlideDuration = 0.55f;

    [Header("Billiard Hit While Sliding")]
    [SerializeField] private int slideHitDamage = 10;
    [SerializeField] private float slideHitRadius = 0.65f;
    [SerializeField] private float slideHitHeightOffset = 0.5f;
    [SerializeField] private float chainSlideDistance = 1.0f;
    [SerializeField] private float chainSlideDuration = 0.16f;

    [Header("Target")]
    [SerializeField] private float targetSearchRadius = 10f;
    [SerializeField] private float targetKeepDistance = 14f;
    [SerializeField, Range(1f, 180f)] private float targetSearchAngle = 110f;
    [SerializeField, Range(0f, 1f)] private float weakAttackAimAssist = 0.45f;
    [SerializeField, Range(0f, 1f)] private float strongAttackAimAssist = 0.80f;
    [SerializeField] private float attackCenterPullMax = 0.45f;

    private Vector3 moveInputDir;
    private Vector3 verticalVelocity;
    private Vector3 lastMoveDirection = Vector3.forward;

    private float weakAttackCooldownTimer;
    private float strongAttackCooldownTimer;

    private bool isAttacking;
    private bool isDodging;
    private bool evadeBoostPressActive;
    private float evadeBoostPressedTime = -999f;

    private EnemyController currentTarget;

    private readonly Collider[] overlapResults = new Collider[64];
    private readonly Dictionary<EnemyController, Coroutine> enemySlideRoutineTable = new Dictionary<EnemyController, Coroutine>();
    private readonly Dictionary<EnemyController, float> enemyMoveSpeedBackupTable = new Dictionary<EnemyController, float>();

    private float baseFixedDeltaTime;

    private bool strongAttackSlowActive;
    private EnemyController strongAttackSlowTrackedEnemy;

    private bool strongKillCutInSlowActive;
    private Coroutine strongKillCutInRoutine;

    public float CurrentHeat => currentHeat;
    public float MaxHeat => maxHeat;
    public EnemyController CurrentTarget => currentTarget;

    private void OnValidate()
    {
        CacheReferences();

        moveSpeed = Mathf.Max(0f, moveSpeed);
        boostMoveSpeed = Mathf.Max(moveSpeed, boostMoveSpeed);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        gravity = Mathf.Max(0f, gravity);
        groundedY = Mathf.Min(groundedY, 0f);
        moveInputDeadZone = Mathf.Clamp01(moveInputDeadZone);

        evadeDistance = Mathf.Max(0.1f, evadeDistance);
        evadeDuration = Mathf.Max(0.01f, evadeDuration);
        evadeHoldThreshold = Mathf.Max(0.01f, evadeHoldThreshold);
        evadeMinInput = Mathf.Clamp01(evadeMinInput);

        maxHeat = Mathf.Max(1f, maxHeat);
        currentHeat = Mathf.Clamp(currentHeat, 0f, maxHeat);
        weakHitHeatGain = Mathf.Max(0f, weakHitHeatGain);
        strongAttackHeatCost = Mathf.Clamp(strongAttackHeatCost, 0f, maxHeat);

        weakDamage = Mathf.Max(1, weakDamage);
        weakAttackRadius = Mathf.Max(0.1f, weakAttackRadius);
        weakAttackForwardOffset = Mathf.Max(0f, weakAttackForwardOffset);
        weakAttackHeightOffset = Mathf.Max(0f, weakAttackHeightOffset);
        weakAttackStartup = Mathf.Max(0f, weakAttackStartup);
        weakAttackCooldown = Mathf.Max(0f, weakAttackCooldown);
        weakKnockbackDistance = Mathf.Max(0f, weakKnockbackDistance);
        weakKnockbackDuration = Mathf.Max(0.01f, weakKnockbackDuration);

        strongDamage = Mathf.Max(1, strongDamage);
        strongAttackRadius = Mathf.Max(0.1f, strongAttackRadius);
        strongAttackForwardOffset = Mathf.Max(0f, strongAttackForwardOffset);
        strongAttackHeightOffset = Mathf.Max(0f, strongAttackHeightOffset);
        strongAttackStartup = Mathf.Max(0f, strongAttackStartup);
        strongAttackCooldown = Mathf.Max(0f, strongAttackCooldown);

        strongAttackSlowTimeScale = Mathf.Clamp(strongAttackSlowTimeScale, 0.01f, 1f);
        strongAttackRestoreSlideProgress = Mathf.Clamp(strongAttackRestoreSlideProgress, 0.05f, 1f);
        strongAttackMissRestoreDelayRealtime = Mathf.Max(0f, strongAttackMissRestoreDelayRealtime);

        strongKillCutInTimeScale = Mathf.Clamp(strongKillCutInTimeScale, 0.01f, 1f);
        strongKillCutInDurationRealtime = Mathf.Max(0.01f, strongKillCutInDurationRealtime);

        strongSlideDistance = Mathf.Max(0.1f, strongSlideDistance);
        strongSlideDuration = Mathf.Max(0.01f, strongSlideDuration);

        slideHitDamage = Mathf.Max(0, slideHitDamage);
        slideHitRadius = Mathf.Max(0.1f, slideHitRadius);
        slideHitHeightOffset = Mathf.Max(0f, slideHitHeightOffset);
        chainSlideDistance = Mathf.Max(0f, chainSlideDistance);
        chainSlideDuration = Mathf.Max(0.01f, chainSlideDuration);

        targetSearchRadius = Mathf.Max(0.1f, targetSearchRadius);
        targetKeepDistance = Mathf.Max(targetSearchRadius, targetKeepDistance);
        targetSearchAngle = Mathf.Clamp(targetSearchAngle, 1f, 180f);
        weakAttackAimAssist = Mathf.Clamp01(weakAttackAimAssist);
        strongAttackAimAssist = Mathf.Clamp01(strongAttackAimAssist);
        attackCenterPullMax = Mathf.Max(0f, attackCenterPullMax);

        if (cameraScript != null)
            cameraScript.SetFollowTarget(transform);
    }

    private void Awake()
    {
        CacheReferences();
        currentHeat = Mathf.Clamp(currentHeat, 0f, maxHeat);
        baseFixedDeltaTime = Time.fixedDeltaTime;

        if (cameraScript != null)
            cameraScript.SetFollowTarget(transform);

        RefreshHeatUI();
    }

    private void OnEnable()
    {
        SetActionEnabled(moveAction, true);
        SetActionEnabled(weakAttackAction, true);
        SetActionEnabled(strongAttackAction, true);
        SetActionEnabled(evadeBoostAction, true);
        SetActionEnabled(targetAction, true);
    }

    private void OnDisable()
    {
        SetActionEnabled(moveAction, false);
        SetActionEnabled(weakAttackAction, false);
        SetActionEnabled(strongAttackAction, false);
        SetActionEnabled(evadeBoostAction, false);
        SetActionEnabled(targetAction, false);

        evadeBoostPressActive = false;
        RestoreAllEnemyMoveSpeeds();
        RestoreAllTimeScaleEffects();
    }

    private void OnDestroy()
    {
        RestoreAllTimeScaleEffects();
    }

    private void Update()
    {
        if (cController == null)
            return;

        TickTimers();
        ValidateCurrentTarget();
        ReadMoveInput();
        HandleTargetInput();
        HandleEvadeBoostInput();
        HandleAttackInput();
        MovePlayer();
        RotateModel();
        UpdateCameraTargetState();
        CleanupDeadEnemyEntries();
        RefreshHeatUI();
    }

    private void CacheReferences()
    {
        if (cController == null)
            cController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (cameraScript == null && Camera.main != null)
            cameraScript = Camera.main.GetComponent<CS_Camera>();
    }

    private void TickTimers()
    {
        if (weakAttackCooldownTimer > 0f)
            weakAttackCooldownTimer -= Time.deltaTime;

        if (strongAttackCooldownTimer > 0f)
            strongAttackCooldownTimer -= Time.deltaTime;
    }

    private void ValidateCurrentTarget()
    {
        if (currentTarget == null)
            return;

        if (!IsEnemyTargetable(currentTarget))
        {
            ClearTarget();
            return;
        }

        float sqrDistance = (currentTarget.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance > targetKeepDistance * targetKeepDistance)
            ClearTarget();
    }

    private void ReadMoveInput()
    {
        Vector2 input2D = ReadMoveValue();
        moveInputDir = new Vector3(input2D.x, 0f, input2D.y);

        if (moveInputDir.sqrMagnitude > 1f)
            moveInputDir.Normalize();

        bool hasMove = moveInputDir.sqrMagnitude > 0.0001f;

        if (hasMove)
            lastMoveDirection = moveInputDir.normalized;

        if (!isDodging && animator != null)
            animator.SetBool("bWalk", hasMove);
    }

    private void HandleTargetInput()
    {
        if (!WasPressedThisFrame(targetAction))
            return;

        EnemyController nextTarget = FindBestTarget();

        if (nextTarget == null)
        {
            ClearTarget();
            return;
        }

        SetCurrentTarget(nextTarget);
    }

    private void HandleEvadeBoostInput()
    {
        if (WasPressedThisFrame(evadeBoostAction))
        {
            evadeBoostPressActive = true;
            evadeBoostPressedTime = Time.time;
        }

        if (WasReleasedThisFrame(evadeBoostAction))
        {
            float heldTime = Time.time - evadeBoostPressedTime;

            bool canEvade =
                evadeBoostPressActive &&
                heldTime < evadeHoldThreshold &&
                !isDodging &&
                !isAttacking &&
                HasEnoughMoveInputForEvade();

            if (canEvade)
                StartCoroutine(EvadeRoutine(GetEvadeDirection()));

            evadeBoostPressActive = false;
        }
    }

    private void HandleAttackInput()
    {
        if (isAttacking || isDodging)
            return;

        if (WasPressedThisFrame(weakAttackAction) && weakAttackCooldownTimer <= 0f)
        {
            if (animator != null)
                animator.SetTrigger("tWeakAttack");

            StartCoroutine(WeakAttackRoutine());
            return;
        }

        if (WasPressedThisFrame(strongAttackAction) && strongAttackCooldownTimer <= 0f && CanUseStrongAttack())
        {
            if (animator != null)
                animator.SetTrigger("tStrongAttack");

            StartCoroutine(StrongAttackRoutine());
        }
    }

    private void MovePlayer()
    {
        if (isDodging)
        {
            if (animator != null)
            {
                animator.SetBool("bWalk", false);
                animator.SetBool("bRun", false);
            }
            return;
        }

        if (cController.isGrounded)
        {
            if (verticalVelocity.y < 0f)
                verticalVelocity.y = groundedY;
        }
        else
        {
            verticalVelocity.y -= gravity * Time.deltaTime;
        }

        bool isBoostMoving = IsBoostMoveActive();
        float currentSpeed = isBoostMoving ? boostMoveSpeed : moveSpeed;

        if (animator != null)
            animator.SetBool("bRun", isBoostMoving);

        Vector3 velocity = moveInputDir * currentSpeed;
        velocity.y = verticalVelocity.y;

        cController.Move(velocity * Time.deltaTime);
    }

    private void RotateModel()
    {
        Vector3 faceDir = Vector3.zero;

        if (IsLockOnActive() && TryGetTargetDirection(out Vector3 targetDir))
            faceDir = targetDir;
        else if (moveInputDir.sqrMagnitude > 0.0001f)
            faceDir = moveInputDir.normalized;

        if (faceDir.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(faceDir, Vector3.up);

        if (modelRoot != null)
        {
            modelRoot.rotation = Quaternion.Slerp(
                modelRoot.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateCameraTargetState()
    {
        if (cameraScript == null)
            return;

        cameraScript.SetFollowTarget(transform);
        cameraScript.SetAttackMode(IsLockOnActive());
        cameraScript.SetLookAtTarget(IsLockOnActive() ? currentTarget.transform : null);
    }

    private IEnumerator EvadeRoutine(Vector3 evadeDir)
    {
        isDodging = true;

        if (animator != null)
        {
            animator.SetBool("bWalk", false);
            animator.SetBool("bRun", false);
        }

        float safeDuration = Mathf.Max(0.01f, evadeDuration);
        float startSpeed = evadeDistance / safeDuration;
        float timer = 0f;

        while (timer < safeDuration)
        {
            if (cController == null)
                yield break;

            if (cController.isGrounded)
            {
                if (verticalVelocity.y < 0f)
                    verticalVelocity.y = groundedY;
            }
            else
            {
                verticalVelocity.y -= gravity * Time.deltaTime;
            }

            float t = timer / safeDuration;
            float speed = Mathf.Lerp(startSpeed, 0f, t);

            Vector3 velocity = evadeDir * speed;
            velocity.y = verticalVelocity.y;

            cController.Move(velocity * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        isDodging = false;
    }

    private IEnumerator WeakAttackRoutine()
    {
        isAttacking = true;
        weakAttackCooldownTimer = weakAttackCooldown;

        Vector3 attackDir = GetAttackDirection(weakAttackAimAssist);

        if (weakAttackStartup > 0f)
            yield return new WaitForSeconds(weakAttackStartup);

        DoWeakAttack(attackDir);

        isAttacking = false;
    }

    private IEnumerator StrongAttackRoutine()
    {
        isAttacking = true;
        strongAttackCooldownTimer = strongAttackCooldown;
        SpendHeat(strongAttackHeatCost);

        Vector3 attackDir = GetAttackDirection(strongAttackAimAssist);

        BeginStrongAttackSlow();

        if (strongAttackStartup > 0f)
            yield return new WaitForSeconds(strongAttackStartup);

        DoStrongAttack(attackDir);

        if (strongAttackSlowActive && strongAttackSlowTrackedEnemy == null)
        {
            if (strongAttackMissRestoreDelayRealtime > 0f)
                yield return new WaitForSecondsRealtime(strongAttackMissRestoreDelayRealtime);

            EndStrongAttackSlow();
        }

        isAttacking = false;
    }

    private void BeginStrongAttackSlow()
    {
        strongAttackSlowTrackedEnemy = null;
        strongAttackSlowActive = true;
        ApplyCombinedTimeScale();
    }

    private void EndStrongAttackSlow()
    {
        strongAttackSlowActive = false;
        strongAttackSlowTrackedEnemy = null;
        ApplyCombinedTimeScale();
    }

    private void RestoreAllTimeScaleEffects()
    {
        strongAttackSlowActive = false;
        strongAttackSlowTrackedEnemy = null;
        strongKillCutInSlowActive = false;

        if (strongKillCutInRoutine != null)
        {
            StopCoroutine(strongKillCutInRoutine);
            strongKillCutInRoutine = null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = baseFixedDeltaTime > 0f ? baseFixedDeltaTime : 0.02f;
    }

    private void ApplyCombinedTimeScale()
    {
        float newTimeScale = 1f;

        if (strongAttackSlowActive)
            newTimeScale = Mathf.Min(newTimeScale, strongAttackSlowTimeScale);

        if (strongKillCutInSlowActive)
            newTimeScale = Mathf.Min(newTimeScale, strongKillCutInTimeScale);

        Time.timeScale = newTimeScale;
        Time.fixedDeltaTime = (baseFixedDeltaTime > 0f ? baseFixedDeltaTime : 0.02f) * Time.timeScale;
    }

    private void StartStrongKillCutIn()
    {
        if (cameraScript != null)
            cameraScript.StartStrongAttackCutIn();

        if (strongKillCutInRoutine != null)
        {
            StopCoroutine(strongKillCutInRoutine);
            strongKillCutInRoutine = null;
        }

        strongKillCutInRoutine = StartCoroutine(StrongKillCutInRoutine());
    }

    private IEnumerator StrongKillCutInRoutine()
    {
        strongKillCutInSlowActive = true;
        ApplyCombinedTimeScale();

        if (strongKillCutInDurationRealtime > 0f)
            yield return new WaitForSecondsRealtime(strongKillCutInDurationRealtime);

        strongKillCutInSlowActive = false;
        strongKillCutInRoutine = null;
        ApplyCombinedTimeScale();
    }

    private void DoWeakAttack(Vector3 attackDir)
    {
        Vector3 center = BuildAttackCenter(attackDir, weakAttackHeightOffset, weakAttackForwardOffset);

        int hitCount = Physics.OverlapSphereNonAlloc(
            center,
            weakAttackRadius,
            overlapResults,
            ~0,
            QueryTriggerInteraction.Collide
        );

        HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();
        bool targetAssignedThisAttack = false;
        int validHitCount = 0;

        for (int i = 0; i < hitCount; i++)
        {
            EnemyController enemy = GetEnemyFromCollider(overlapResults[i]);
            if (enemy == null || !hitEnemies.Add(enemy))
                continue;

            validHitCount++;

            if (!targetAssignedThisAttack)
            {
                SetCurrentTarget(enemy);
                targetAssignedThisAttack = true;
            }

            Vector3 knockDir = GetKnockDirection(enemy.transform.position, attackDir);

            if (cameraScript != null)
                cameraScript.StartShake(0.12f, 0.20f);

            bool dead = ApplyDamageToEnemy(enemy, weakDamage);
            if (dead)
                continue;

            StartOrReplaceSlide(enemy, knockDir, weakKnockbackDistance, weakKnockbackDuration, 0, 0f, 0.01f);
        }

        if (validHitCount > 0)
            GainHeat(weakHitHeatGain * validHitCount);
    }

    private void DoStrongAttack(Vector3 attackDir)
    {
        Vector3 center = BuildAttackCenter(attackDir, strongAttackHeightOffset, strongAttackForwardOffset);

        int hitCount = Physics.OverlapSphereNonAlloc(
            center,
            strongAttackRadius,
            overlapResults,
            ~0,
            QueryTriggerInteraction.Collide
        );

        HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();
        bool targetAssignedThisAttack = false;
        bool slowTargetAssigned = false;
        bool killCutInTriggered = false;

        for (int i = 0; i < hitCount; i++)
        {
            EnemyController enemy = GetEnemyFromCollider(overlapResults[i]);
            if (enemy == null || !hitEnemies.Add(enemy))
                continue;

            if (!targetAssignedThisAttack)
            {
                SetCurrentTarget(enemy);
                targetAssignedThisAttack = true;
            }

            Vector3 slideDir = GetKnockDirection(enemy.transform.position, attackDir);

            if (cameraScript != null)
                cameraScript.StartShake(0.20f, 1.00f);

            bool dead = ApplyDamageToEnemy(enemy, strongDamage);

            if (dead)
            {
                if (!killCutInTriggered)
                {
                    StartStrongKillCutIn();
                    killCutInTriggered = true;
                }
                continue;
            }

            if (strongAttackSlowActive && !slowTargetAssigned)
            {
                strongAttackSlowTrackedEnemy = enemy;
                slowTargetAssigned = true;
            }

            StartOrReplaceSlide(
                enemy,
                slideDir,
                strongSlideDistance,
                strongSlideDuration,
                slideHitDamage,
                chainSlideDistance,
                chainSlideDuration
            );
        }
    }

    private Vector3 GetAttackDirection(float assistWeight)
    {
        Vector3 dir;

        if (IsLockOnActive() && TryGetTargetDirection(out Vector3 targetDir))
        {
            dir = moveInputDir.sqrMagnitude > 0.0001f
                ? Vector3.Slerp(moveInputDir.normalized, targetDir, assistWeight).normalized
                : targetDir;
        }
        else
        {
            dir = GetBaseForward();

            if (TryGetTargetDirection(out Vector3 assistTargetDir))
                dir = Vector3.Slerp(dir, assistTargetDir, assistWeight).normalized;
        }

        return dir;
    }

    private Vector3 BuildAttackCenter(Vector3 attackDir, float heightOffset, float forwardOffset)
    {
        Vector3 center = transform.position + Vector3.up * heightOffset + attackDir * forwardOffset;

        if (currentTarget != null && IsEnemyTargetable(currentTarget))
        {
            Vector3 targetPos = currentTarget.transform.position + Vector3.up * heightOffset;
            Vector3 toTarget = targetPos - center;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
                center += Vector3.ClampMagnitude(toTarget, attackCenterPullMax);
        }

        return center;
    }

    private EnemyController FindBestTarget()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        if (enemies == null || enemies.Length == 0)
            return null;

        Vector3 baseForward = GetTargetSearchForward();

        EnemyController bestFrontTarget = null;
        float bestFrontScore = float.MaxValue;

        EnemyController bestFallbackTarget = null;
        float bestFallbackDistance = float.MaxValue;

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyController enemy = enemies[i];
            if (!IsEnemyTargetable(enemy))
                continue;

            Vector3 toEnemy = enemy.transform.position - transform.position;
            toEnemy.y = 0f;

            float distance = toEnemy.magnitude;
            if (distance > targetSearchRadius)
                continue;

            if (distance < bestFallbackDistance)
            {
                bestFallbackDistance = distance;
                bestFallbackTarget = enemy;
            }

            if (distance <= 0.0001f)
                continue;

            Vector3 dir = toEnemy / distance;
            float angle = Vector3.Angle(baseForward, dir);

            if (angle > targetSearchAngle)
                continue;

            float score = distance + angle * 0.08f;
            if (score < bestFrontScore)
            {
                bestFrontScore = score;
                bestFrontTarget = enemy;
            }
        }

        return bestFrontTarget != null ? bestFrontTarget : bestFallbackTarget;
    }

    private Vector3 GetTargetSearchForward()
    {
        if (IsLockOnActive() && TryGetTargetDirection(out Vector3 targetDir))
            return targetDir;

        return GetBaseForward();
    }

    private Vector3 GetBaseForward()
    {
        Vector3 forward = moveInputDir.sqrMagnitude > 0.0001f ? moveInputDir : lastMoveDirection;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f)
        {
            if (modelRoot != null)
                forward = modelRoot.forward;
            else
                forward = transform.forward;
        }

        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.0001f)
            forward = Vector3.forward;

        return forward.normalized;
    }

    private bool TryGetTargetDirection(out Vector3 dir)
    {
        dir = Vector3.zero;

        if (currentTarget == null || !IsEnemyTargetable(currentTarget))
            return false;

        Vector3 toTarget = currentTarget.transform.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.0001f)
            return false;

        dir = toTarget.normalized;
        return true;
    }

    private Vector3 GetKnockDirection(Vector3 targetPos, Vector3 fallbackDir)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            dir = fallbackDir;

        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            dir = Vector3.forward;

        return dir.normalized;
    }

    private Vector3 GetEvadeDirection()
    {
        Vector3 dir = moveInputDir;

        if (dir.sqrMagnitude <= 0.0001f)
            dir = lastMoveDirection;

        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            if (modelRoot != null)
                dir = modelRoot.forward;
            else
                dir = transform.forward;
        }

        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            dir = Vector3.forward;

        return dir.normalized;
    }

    private bool CanUseStrongAttack()
    {
        return currentHeat >= strongAttackHeatCost;
    }

    private void SpendHeat(float value)
    {
        currentHeat = Mathf.Max(0f, currentHeat - value);
    }

    private void GainHeat(float value)
    {
        currentHeat = Mathf.Min(maxHeat, currentHeat + value);
    }

    private void RefreshHeatUI()
    {
        if (heatSlider == null)
            return;

        heatSlider.minValue = 0f;
        heatSlider.maxValue = maxHeat;
        heatSlider.SetValueWithoutNotify(currentHeat);

        Image fillImage = heatSlider.fillRect != null ? heatSlider.fillRect.GetComponent<Image>() : null;
        if (fillImage != null)
        {
            fillImage.color = CanUseStrongAttack()
                ? new Color(1.0f, 0.25f, 0.1f, 1f)
                : new Color(1.0f, 0.42f, 0.05f, 1f);
        }
    }

    private bool HasEnoughMoveInputForEvade()
    {
        return moveInputDir.sqrMagnitude >= evadeMinInput * evadeMinInput;
    }

    private bool IsBoostMoveActive()
    {
        if (isDodging)
            return false;

        if (!IsPressed(evadeBoostAction))
            return false;

        if (!HasEnoughMoveInputForEvade())
            return false;

        return Time.time - evadeBoostPressedTime >= evadeHoldThreshold;
    }

    private bool IsLockOnActive()
    {
        return currentTarget != null && IsEnemyTargetable(currentTarget);
    }

    private bool IsEnemyTargetable(EnemyController enemy)
    {
        return enemy != null && enemy.IsAlive;
    }

    private EnemyController GetEnemyFromCollider(Collider col)
    {
        if (col == null)
            return null;

        EnemyController enemy = col.GetComponentInParent<EnemyController>();
        if (enemy == null || enemy.gameObject == gameObject || !IsEnemyTargetable(enemy))
            return null;

        return enemy;
    }

    private bool ApplyDamageToEnemy(EnemyController enemy, int damage)
    {
        return enemy != null && enemy.TakeDamage(damage);
    }

    private void SetCurrentTarget(EnemyController target)
    {
        if (target == null || !IsEnemyTargetable(target))
        {
            ClearTarget();
            return;
        }

        currentTarget = target;
    }

    private void ClearTarget()
    {
        currentTarget = null;
    }

    private void StartOrReplaceSlide(
        EnemyController enemy,
        Vector3 dir,
        float distance,
        float duration,
        int collisionDamage,
        float chainDistance,
        float chainDuration)
    {
        if (enemy == null)
            return;

        if (enemySlideRoutineTable.TryGetValue(enemy, out Coroutine running) && running != null)
        {
            StopCoroutine(running);
            RestoreEnemyMoveSpeed(enemy);
            enemySlideRoutineTable.Remove(enemy);
        }

        Coroutine routine = StartCoroutine(
            SlideEnemyRoutine(enemy, dir, distance, duration, collisionDamage, chainDistance, chainDuration)
        );

        enemySlideRoutineTable[enemy] = routine;
    }

    private IEnumerator SlideEnemyRoutine(
        EnemyController enemy,
        Vector3 dir,
        float distance,
        float duration,
        int collisionDamage,
        float chainDistance,
        float chainDuration)
    {
        if (enemy == null || enemy.transform == null)
        {
            if (strongAttackSlowActive && strongAttackSlowTrackedEnemy == enemy)
                EndStrongAttackSlow();

            yield break;
        }

        CacheAndStopEnemyMove(enemy);

        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.0001f)
            dir = Vector3.forward;
        dir.Normalize();

        float safeDuration = Mathf.Max(0.01f, duration);
        float startSpeed = distance / safeDuration;
        float fixedY = enemy.transform.position.y;
        float timer = 0f;

        HashSet<EnemyController> hitDuringSlide = new HashSet<EnemyController> { enemy };

        while (timer < safeDuration)
        {
            if (enemy == null || enemy.transform == null || !enemy.IsAlive)
            {
                RestoreEnemyMoveSpeed(enemy);
                enemySlideRoutineTable.Remove(enemy);

                if (strongAttackSlowActive && strongAttackSlowTrackedEnemy == enemy)
                    EndStrongAttackSlow();

                yield break;
            }

            float t = timer / safeDuration;
            float speed = Mathf.Lerp(startSpeed, 0f, t);

            Vector3 move = dir * speed * Time.deltaTime;
            Vector3 nextPos = enemy.transform.position + move;
            nextPos.y = fixedY;
            enemy.transform.position = nextPos;

            float progress = Mathf.Clamp01((timer + Time.deltaTime) / safeDuration);
            if (strongAttackSlowActive && strongAttackSlowTrackedEnemy == enemy && progress >= strongAttackRestoreSlideProgress)
                EndStrongAttackSlow();

            if (collisionDamage > 0)
            {
                ResolveSlidingEnemyHits(
                    enemy,
                    dir,
                    collisionDamage,
                    chainDistance,
                    chainDuration,
                    hitDuringSlide
                );
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (enemy != null)
        {
            RestoreEnemyMoveSpeed(enemy);
            enemySlideRoutineTable.Remove(enemy);
        }

        if (strongAttackSlowActive && strongAttackSlowTrackedEnemy == enemy)
            EndStrongAttackSlow();
    }

    private void ResolveSlidingEnemyHits(
        EnemyController slidingEnemy,
        Vector3 slideDir,
        int collisionDamage,
        float chainDistance,
        float chainDuration,
        HashSet<EnemyController> hitDuringSlide)
    {
        if (slidingEnemy == null || !slidingEnemy.IsAlive)
            return;

        Vector3 center = slidingEnemy.transform.position + Vector3.up * slideHitHeightOffset;

        int hitCount = Physics.OverlapSphereNonAlloc(
            center,
            slideHitRadius,
            overlapResults,
            ~0,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hitCount; i++)
        {
            EnemyController other = GetEnemyFromCollider(overlapResults[i]);
            if (other == null || other == slidingEnemy || !hitDuringSlide.Add(other))
                continue;

            Vector3 chainDir = other.transform.position - slidingEnemy.transform.position;
            chainDir.y = 0f;

            if (chainDir.sqrMagnitude <= 0.0001f)
                chainDir = slideDir;

            chainDir.Normalize();

            bool dead = ApplyDamageToEnemy(other, collisionDamage);
            if (dead)
                continue;

            if (chainDistance > 0f)
            {
                StartOrReplaceSlide(
                    other,
                    chainDir,
                    chainDistance,
                    chainDuration,
                    0,
                    0f,
                    0.01f
                );
            }
        }
    }

    private void CacheAndStopEnemyMove(EnemyController enemy)
    {
        if (enemy == null)
            return;

        if (!enemyMoveSpeedBackupTable.ContainsKey(enemy))
            enemyMoveSpeedBackupTable.Add(enemy, enemy.moveSpeed);

        enemy.moveSpeed = 0f;
    }

    private void RestoreEnemyMoveSpeed(EnemyController enemy)
    {
        if (enemy == null)
            return;

        if (enemyMoveSpeedBackupTable.TryGetValue(enemy, out float oldMoveSpeed))
        {
            enemy.moveSpeed = oldMoveSpeed;
            enemyMoveSpeedBackupTable.Remove(enemy);
        }
    }

    private void RestoreAllEnemyMoveSpeeds()
    {
        foreach (var pair in enemyMoveSpeedBackupTable)
        {
            if (pair.Key != null)
                pair.Key.moveSpeed = pair.Value;
        }

        enemyMoveSpeedBackupTable.Clear();
        enemySlideRoutineTable.Clear();
    }

    private void CleanupDeadEnemyEntries()
    {
        List<EnemyController> removeSlideKeys = null;
        foreach (var pair in enemySlideRoutineTable)
        {
            if (pair.Key == null)
            {
                if (removeSlideKeys == null) removeSlideKeys = new List<EnemyController>();
                removeSlideKeys.Add(pair.Key);
            }
        }

        if (removeSlideKeys != null)
        {
            for (int i = 0; i < removeSlideKeys.Count; i++)
                enemySlideRoutineTable.Remove(removeSlideKeys[i]);
        }

        List<EnemyController> removeSpeedKeys = null;
        foreach (var pair in enemyMoveSpeedBackupTable)
        {
            if (pair.Key == null)
            {
                if (removeSpeedKeys == null) removeSpeedKeys = new List<EnemyController>();
                removeSpeedKeys.Add(pair.Key);
            }
        }

        if (removeSpeedKeys != null)
        {
            for (int i = 0; i < removeSpeedKeys.Count; i++)
                enemyMoveSpeedBackupTable.Remove(removeSpeedKeys[i]);
        }
    }

    private void SetActionEnabled(InputActionReference actionRef, bool enable)
    {
        if (actionRef == null || actionRef.action == null)
            return;

        if (enable)
            actionRef.action.Enable();
        else
            actionRef.action.Disable();
    }

    private Vector2 ReadMoveValue()
    {
        if (moveAction == null || moveAction.action == null)
            return Vector2.zero;

        Vector2 value = moveAction.action.ReadValue<Vector2>();

        if (value.magnitude < moveInputDeadZone)
            return Vector2.zero;

        if (value.sqrMagnitude > 1f)
            value.Normalize();

        return value;
    }

    private bool IsPressed(InputActionReference actionRef)
    {
        return actionRef != null && actionRef.action != null && actionRef.action.IsPressed();
    }

    private bool WasPressedThisFrame(InputActionReference actionRef)
    {
        return actionRef != null && actionRef.action != null && actionRef.action.WasPressedThisFrame();
    }

    private bool WasReleasedThisFrame(InputActionReference actionRef)
    {
        return actionRef != null && actionRef.action != null && actionRef.action.WasReleasedThisFrame();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 attackDir = Application.isPlaying ? GetAttackDirection(1f) : transform.forward;

        if (attackDir.sqrMagnitude <= 0.0001f)
            attackDir = Vector3.forward;

        attackDir.Normalize();

        Vector3 weakCenter = transform.position + Vector3.up * weakAttackHeightOffset + attackDir * weakAttackForwardOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(weakCenter, weakAttackRadius);

        Vector3 strongCenter = transform.position + Vector3.up * strongAttackHeightOffset + attackDir * strongAttackForwardOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(strongCenter, strongAttackRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * slideHitHeightOffset, slideHitRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, targetSearchRadius);
    }
}