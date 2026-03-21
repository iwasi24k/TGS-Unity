using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SC_Player : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private GameObject Camera;
    [SerializeField] private CS_Camera cameraScript;

    [Header("References")]
    [SerializeField] private CharacterController cController;
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Slider heatSlider;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float boostMoveSpeed = 3.8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float groundedY = -2f;

    [Header("Heat")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float currentHeat = 0f;
    [SerializeField] private float weakHitHeatGain = 18f;
    [SerializeField] private float strongAttackHeatCost = 50f;

    [Header("Enemy HP (Player side temporary management)")]
    [SerializeField] private int enemyMaxHP = 40;

    [Header("Weak Attack (J)")]
    [SerializeField] private int weakDamage = 10;
    [SerializeField] private float weakAttackRadius = 1.0f;
    [SerializeField] private float weakAttackForwardOffset = 1.0f;
    [SerializeField] private float weakAttackHeightOffset = 0.8f;
    [SerializeField] private float weakAttackStartup = 0.04f;
    [SerializeField] private float weakAttackCooldown = 0.15f;
    [SerializeField] private float weakKnockbackDistance = 0.55f;
    [SerializeField] private float weakKnockbackDuration = 0.10f;

    [Header("Strong Attack (K)")]
    [SerializeField] private int strongDamage = 20;
    [SerializeField] private float strongAttackRadius = 1.2f;
    [SerializeField] private float strongAttackForwardOffset = 1.1f;
    [SerializeField] private float strongAttackHeightOffset = 0.8f;
    [SerializeField] private float strongAttackStartup = 0.06f;
    [SerializeField] private float strongAttackCooldown = 0.35f;

    [Header("Strong Slide Knockback")]
    [SerializeField] private float strongSlideDistance = 4.2f;
    [SerializeField] private float strongSlideDuration = 0.55f;

    [Header("Billiard Hit While Sliding")]
    [SerializeField] private int slideHitDamage = 10;
    [SerializeField] private float slideHitRadius = 0.65f;
    [SerializeField] private float slideHitHeightOffset = 0.5f;
    [SerializeField] private float chainSlideDistance = 1.0f;
    [SerializeField] private float chainSlideDuration = 0.16f;

    [Header("Keys (Input System / Variable)")]
    [SerializeField] private Key keyUp = Key.W;
    [SerializeField] private Key keyDown = Key.S;
    [SerializeField] private Key keyLeft = Key.A;
    [SerializeField] private Key keyRight = Key.D;
    [SerializeField] private Key boostMoveKey = Key.LeftShift;
    [SerializeField] private Key weakAttackKey = Key.J;
    [SerializeField] private Key strongAttackKey = Key.K;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private Vector3 moveInputDir;
    private Vector3 verticalVelocity;
    private Vector3 lastMoveDirection = Vector3.forward;

    private float weakAttackCooldownTimer;
    private float strongAttackCooldownTimer;

    private bool isAttacking;

    private readonly Dictionary<EnemyController, int> enemyHpTable = new Dictionary<EnemyController, int>();
    private readonly Dictionary<EnemyController, Coroutine> enemySlideRoutineTable = new Dictionary<EnemyController, Coroutine>();
    private readonly Dictionary<EnemyController, float> enemyMoveSpeedBackupTable = new Dictionary<EnemyController, float>();

    private readonly Collider[] overlapResults = new Collider[64];

    private static Sprite runtimeWhiteSprite;

    public float CurrentHeat => currentHeat;
    public float MaxHeat => maxHeat;

    private static Sprite RuntimeWhiteSprite
    {
        get
        {
            if (runtimeWhiteSprite == null)
            {
                runtimeWhiteSprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f)
                );
                runtimeWhiteSprite.name = "RuntimeWhiteSprite";
            }

            return runtimeWhiteSprite;
        }
    }

    private void OnValidate()
    {
        if (cController == null)
            cController = GetComponent<CharacterController>();

        if(animator == null)
            animator = GetComponentInChildren<Animator>();

        if (Camera == null)
            Camera = GameObject.FindWithTag("MainCamera");

        if (cameraScript == null)
            cameraScript = Camera.GetComponent<CS_Camera>();

        keyUp = SanitizeKey(keyUp, Key.W);
        keyDown = SanitizeKey(keyDown, Key.S);
        keyLeft = SanitizeKey(keyLeft, Key.A);
        keyRight = SanitizeKey(keyRight, Key.D);
        boostMoveKey = SanitizeKey(boostMoveKey, Key.LeftShift);
        weakAttackKey = SanitizeKey(weakAttackKey, Key.J);
        strongAttackKey = SanitizeKey(strongAttackKey, Key.K);

        moveSpeed = Mathf.Max(0f, moveSpeed);
        boostMoveSpeed = Mathf.Max(moveSpeed, boostMoveSpeed);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        gravity = Mathf.Max(0f, gravity);
        groundedY = Mathf.Min(groundedY, 0f);

        maxHeat = Mathf.Max(1f, maxHeat);
        currentHeat = Mathf.Clamp(currentHeat, 0f, maxHeat);
        weakHitHeatGain = Mathf.Max(0f, weakHitHeatGain);
        strongAttackHeatCost = Mathf.Clamp(strongAttackHeatCost, 0f, maxHeat);

        enemyMaxHP = Mathf.Max(1, enemyMaxHP);

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

        strongSlideDistance = Mathf.Max(0.1f, strongSlideDistance);
        strongSlideDuration = Mathf.Max(0.01f, strongSlideDuration);

        slideHitDamage = Mathf.Max(0, slideHitDamage);
        slideHitRadius = Mathf.Max(0.1f, slideHitRadius);
        slideHitHeightOffset = Mathf.Max(0f, slideHitHeightOffset);
        chainSlideDistance = Mathf.Max(0f, chainSlideDistance);
        chainSlideDuration = Mathf.Max(0.01f, chainSlideDuration);

        cameraScript.SetFollowTarget(this.transform);
    }

    private void Awake()
    {
        if (cController == null)
            cController = GetComponent<CharacterController>();

        keyUp = SanitizeKey(keyUp, Key.W);
        keyDown = SanitizeKey(keyDown, Key.S);
        keyLeft = SanitizeKey(keyLeft, Key.A);
        keyRight = SanitizeKey(keyRight, Key.D);
        boostMoveKey = SanitizeKey(boostMoveKey, Key.LeftShift);
        weakAttackKey = SanitizeKey(weakAttackKey, Key.J);
        strongAttackKey = SanitizeKey(strongAttackKey, Key.K);

        currentHeat = Mathf.Clamp(currentHeat, 0f, maxHeat);

        CreateHeatGaugeIfNeeded();
        RefreshHeatUI();
    }

    private void Update()
    {
        if (cController == null)
            return;

        TickTimers();
        ReadMoveInput();
        HandleAttackInput();
        MovePlayer();
        RotateModel();
        CleanupDeadEnemyEntries();
        RefreshHeatUI();
    }

    private void TickTimers()
    {
        if (weakAttackCooldownTimer > 0f)
            weakAttackCooldownTimer -= Time.deltaTime;

        if (strongAttackCooldownTimer > 0f)
            strongAttackCooldownTimer -= Time.deltaTime;
    }

    private void ReadMoveInput()
    {
        float x = 0f;
        float z = 0f;

        if (GetKeySafe(keyLeft)) x -= 1f;
        if (GetKeySafe(keyRight)) x += 1f;
        if (GetKeySafe(keyUp)) z += 1f;
        if (GetKeySafe(keyDown)) z -= 1f;

        moveInputDir = new Vector3(x, 0f, z);

        if (moveInputDir.sqrMagnitude > 1f)
            moveInputDir.Normalize();

        if (moveInputDir.sqrMagnitude > 0.0001f)
        {
            lastMoveDirection = moveInputDir.normalized;
            animator.SetBool("bWalk", true);
        }
        else
        {
            animator.SetBool("bWalk", false);
        }
    }

    private void HandleAttackInput()
    {
        if (isAttacking)
            return;

        if (GetKeyDownSafe(weakAttackKey) && weakAttackCooldownTimer <= 0f)
        {
            animator.SetTrigger("tWeakAttack");
            StartCoroutine(WeakAttackRoutine());
            return;
        }

        if (GetKeyDownSafe(strongAttackKey) && strongAttackCooldownTimer <= 0f)
        {
            if (!CanUseStrongAttack())
                return;

            animator.SetTrigger("tStrongAttack");
            StartCoroutine(StrongAttackRoutine());
        }
    }

    private bool CanUseStrongAttack()
    {
        return currentHeat >= strongAttackHeatCost;
    }

    private void SpendHeat(float value)
    {
        currentHeat -= value;
        if (currentHeat < 0f)
            currentHeat = 0f;
    }

    private void GainHeat(float value)
    {
        currentHeat += value;
        if (currentHeat > maxHeat)
            currentHeat = maxHeat;
    }

    private void CreateHeatGaugeIfNeeded()
    {
        if (heatSlider != null)
            return;

        GameObject canvasObj = GameObject.Find("RuntimeHeatCanvas");
        Canvas canvas;

        if (canvasObj == null)
        {
            canvasObj = new GameObject(
                "RuntimeHeatCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
        else
        {
            canvas = canvasObj.GetComponent<Canvas>();
            if (canvas == null)
                canvas = canvasObj.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (canvasObj.GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (canvasObj.GetComponent<GraphicRaycaster>() == null)
                canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject gaugeRoot = new GameObject(
            "HeatGaugeRoot",
            typeof(RectTransform),
            typeof(Image),
            typeof(Slider)
        );
        gaugeRoot.transform.SetParent(canvasObj.transform, false);

        RectTransform rootRect = gaugeRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.sizeDelta = new Vector2(320f, 24f);
        rootRect.anchoredPosition = new Vector2(30f, -30f);

        Image backgroundImage = gaugeRoot.GetComponent<Image>();
        backgroundImage.sprite = RuntimeWhiteSprite;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(gaugeRoot.transform, false);

        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(3f, 3f);
        fillAreaRect.offsetMax = new Vector2(-3f, -3f);

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(fillArea.transform, false);

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObj.GetComponent<Image>();
        fillImage.sprite = RuntimeWhiteSprite;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = new Color(1.0f, 0.42f, 0.05f, 1f);

        Slider slider = gaugeRoot.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = maxHeat;
        slider.wholeNumbers = false;
        slider.fillRect = fillRect;
        slider.handleRect = null;
        slider.targetGraphic = fillImage;
        slider.SetValueWithoutNotify(currentHeat);

        heatSlider = slider;
    }

    private void RefreshHeatUI()
    {
        if (heatSlider == null)
            return;

        heatSlider.minValue = 0f;
        heatSlider.maxValue = maxHeat;
        heatSlider.SetValueWithoutNotify(currentHeat);

        Image fillImage = null;
        if (heatSlider.fillRect != null)
            fillImage = heatSlider.fillRect.GetComponent<Image>();

        if (fillImage != null)
        {
            if (CanUseStrongAttack())
                fillImage.color = new Color(1.0f, 0.25f, 0.1f, 1f);
            else
                fillImage.color = new Color(1.0f, 0.42f, 0.05f, 1f);
        }
    }

    private void MovePlayer()
    {
        if (cController.isGrounded)
        {
            if (verticalVelocity.y < 0f)
                verticalVelocity.y = groundedY;
        }
        else
        {
            verticalVelocity.y -= gravity * Time.deltaTime;
        }

        bool isBoostMoving = GetKeySafe(boostMoveKey);
        float currentSpeed = isBoostMoving ? boostMoveSpeed : moveSpeed;

        if(isBoostMoving)
            animator.SetBool("bRun", true);
        else
            animator.SetBool("bRun", false);

        Vector3 horizontalVelocity = moveInputDir * currentSpeed;

        Vector3 finalVelocity = horizontalVelocity;
        finalVelocity.y = verticalVelocity.y;

        cController.Move(finalVelocity * Time.deltaTime);
    }

    private void RotateModel()
    {
        Vector3 faceDir = moveInputDir.sqrMagnitude > 0.0001f ? moveInputDir : Vector3.zero;

        if (faceDir == Vector3.zero)
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

    private IEnumerator WeakAttackRoutine()
    {
        isAttacking = true;
        weakAttackCooldownTimer = weakAttackCooldown;

        Vector3 attackDir = GetAttackDirection();

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

        Vector3 attackDir = GetAttackDirection();

        if (strongAttackStartup > 0f)
            yield return new WaitForSeconds(strongAttackStartup);

        DoStrongAttack(attackDir);

        isAttacking = false;
    }

    private Vector3 GetAttackDirection()
    {
        Vector3 dir = moveInputDir.sqrMagnitude > 0.0001f ? moveInputDir : lastMoveDirection;
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

    private void DoWeakAttack(Vector3 attackDir)
    {
        Vector3 center = transform.position
                       + Vector3.up * weakAttackHeightOffset
                       + attackDir * weakAttackForwardOffset;

        int hitCount = Physics.OverlapSphereNonAlloc(
            center,
            weakAttackRadius,
            overlapResults,
            ~0,
            QueryTriggerInteraction.Collide
        );

        HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();
        int validHitCount = 0;

        for (int i = 0; i < hitCount; i++)
        {
            EnemyController enemy = GetEnemyFromCollider(overlapResults[i]);
            if (enemy == null)
                continue;

            if (!hitEnemies.Add(enemy))
                continue;

            validHitCount++;

            Vector3 knockDir = GetKnockDirection(enemy.transform.position, attackDir);

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
        Vector3 center = transform.position
                       + Vector3.up * strongAttackHeightOffset
                       + attackDir * strongAttackForwardOffset;

        int hitCount = Physics.OverlapSphereNonAlloc(
            center,
            strongAttackRadius,
            overlapResults,
            ~0,
            QueryTriggerInteraction.Collide
        );

        HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

        for (int i = 0; i < hitCount; i++)
        {
            EnemyController enemy = GetEnemyFromCollider(overlapResults[i]);
            if (enemy == null)
                continue;

            if (!hitEnemies.Add(enemy))
                continue;

            Vector3 slideDir = GetKnockDirection(enemy.transform.position, attackDir);

            cameraScript.StartShake(0.20f, 1.00f);
            bool dead = ApplyDamageToEnemy(enemy, strongDamage);
            if (dead)
                continue;

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

    private EnemyController GetEnemyFromCollider(Collider col)
    {
        if (col == null)
            return null;

        EnemyController enemy = col.GetComponentInParent<EnemyController>();
        if (enemy == null)
            return null;

        if (enemy.gameObject == gameObject)
            return null;

        return enemy;
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

    private int GetEnemyHP(EnemyController enemy)
    {
        if (enemy == null)
            return 0;

        if (!enemyHpTable.TryGetValue(enemy, out int hp))
        {
            hp = enemyMaxHP;
            enemyHpTable.Add(enemy, hp);
        }

        return hp;
    }

    private bool ApplyDamageToEnemy(EnemyController enemy, int damage)
    {
        if (enemy == null)
            return false;

        int currentEnemyHP = GetEnemyHP(enemy);
        currentEnemyHP -= Mathf.Max(0, damage);

        enemyHpTable[enemy] = currentEnemyHP;

        if (currentEnemyHP <= 0)
        {
            KillEnemy(enemy);
            return true;
        }

        return false;
    }

    private void KillEnemy(EnemyController enemy)
    {
        if (enemy == null)
            return;

        if (enemySlideRoutineTable.TryGetValue(enemy, out Coroutine running) && running != null)
        {
            StopCoroutine(running);
            enemySlideRoutineTable.Remove(enemy);
        }

        RestoreEnemyMoveSpeed(enemy);
        enemyHpTable.Remove(enemy);

        if (enemy != null && enemy.gameObject != null)
            Destroy(enemy.gameObject);
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
        if (enemy == null)
            yield break;

        Transform target = enemy.transform;
        if (target == null)
            yield break;

        CacheAndStopEnemyMove(enemy);

        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.0001f)
            dir = Vector3.forward;
        dir.Normalize();

        float safeDuration = Mathf.Max(0.01f, duration);
        float startSpeed = distance / safeDuration;
        float fixedY = target.position.y;

        HashSet<EnemyController> hitDuringSlide = new HashSet<EnemyController>();
        hitDuringSlide.Add(enemy);

        float timer = 0f;

        while (timer < safeDuration)
        {
            if (enemy == null || target == null)
                yield break;

            float t = timer / safeDuration;
            float speed = Mathf.Lerp(startSpeed, 0f, t);

            Vector3 move = dir * speed * Time.deltaTime;
            Vector3 nextPos = target.position + move;
            nextPos.y = fixedY;
            target.position = nextPos;

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
    }

    private void ResolveSlidingEnemyHits(
        EnemyController slidingEnemy,
        Vector3 slideDir,
        int collisionDamage,
        float chainDistance,
        float chainDuration,
        HashSet<EnemyController> hitDuringSlide)
    {
        if (slidingEnemy == null)
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
            if (other == null)
                continue;

            if (other == slidingEnemy)
                continue;

            if (!hitDuringSlide.Add(other))
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

    private void CleanupDeadEnemyEntries()
    {
        List<EnemyController> removeHpKeys = null;
        foreach (var pair in enemyHpTable)
        {
            if (pair.Key == null)
            {
                if (removeHpKeys == null) removeHpKeys = new List<EnemyController>();
                removeHpKeys.Add(pair.Key);
            }
        }

        if (removeHpKeys != null)
        {
            for (int i = 0; i < removeHpKeys.Count; i++)
                enemyHpTable.Remove(removeHpKeys[i]);
        }

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

    private Key SanitizeKey(Key current, Key fallback)
    {
        if (!Enum.IsDefined(typeof(Key), current))
            return fallback;

        if (current == Key.None)
            return fallback;

        return current;
    }

    private bool GetKeySafe(Key key)
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
            return false;

        if (!Enum.IsDefined(typeof(Key), key) || key == Key.None)
            return false;

        return kb[key].isPressed;
    }

    private bool GetKeyDownSafe(Key key)
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
            return false;

        if (!Enum.IsDefined(typeof(Key), key) || key == Key.None)
            return false;

        return kb[key].wasPressedThisFrame;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 attackDir = Application.isPlaying ? GetAttackDirection() : transform.forward;

        Vector3 weakCenter = transform.position
                           + Vector3.up * weakAttackHeightOffset
                           + attackDir.normalized * weakAttackForwardOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(weakCenter, weakAttackRadius);

        Vector3 strongCenter = transform.position
                             + Vector3.up * strongAttackHeightOffset
                             + attackDir.normalized * strongAttackForwardOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(strongCenter, strongAttackRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * slideHitHeightOffset, slideHitRadius);
    }
}