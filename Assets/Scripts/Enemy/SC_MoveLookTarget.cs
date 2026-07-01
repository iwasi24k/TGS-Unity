using UnityEngine;

public class SC_MoveLookTarget : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("進行方向へ向けたいパーツ。空ならこのGameObject自身を回転")]
    [SerializeField] private Transform[] rotateTargets;

    [Header("Setting")]
    [Tooltip("向きの補正。モデルの正面がズレる場合に使う")]
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    [Tooltip("進行方向へ向く時の回転速度")]
    [SerializeField] private float rotateSpeed = 720f;

    [Tooltip("元の向きへ戻るまでの時間")]
    [SerializeField] private float restoreDuration = 0.3f;

    private float restoreTimer;

    private Quaternion selfRestoreStartLocalRotation;
    private Quaternion[] targetRestoreStartLocalRotations;

    [Tooltip("trueなら即回転、falseならブレンド回転")]
    [SerializeField] private bool instantRotation = false;

    [Tooltip("ClearLookDirection時に元の回転へ戻す")]
    [SerializeField] private bool restoreRotationOnClear = true;

    [Tooltip("trueなら元の向きへ即戻す。falseならブレンドで戻す")]
    [SerializeField] private bool instantRestore = false;

    private Vector3 lookDirection;
    private bool hasLookDirection;
    private bool isRestoring;

    private Quaternion currentWorldRotation;
    private bool initializedRotation;

    private Quaternion selfDefaultLocalRotation;
    private Quaternion[] targetDefaultLocalRotations;
    private bool savedDefaultRotation;

    private void Awake()
    {
        SaveDefaultLocalRotations();
    }

    private void OnEnable()
    {
        if (!savedDefaultRotation)
        {
            SaveDefaultLocalRotations();
        }

        currentWorldRotation = GetCurrentWorldRotation();
        initializedRotation = true;

        hasLookDirection = false;
        isRestoring = false;
    }

    private void SaveDefaultLocalRotations()
    {
        selfDefaultLocalRotation = transform.localRotation;

        if (rotateTargets != null && rotateTargets.Length > 0)
        {
            targetDefaultLocalRotations = new Quaternion[rotateTargets.Length];

            for (int i = 0; i < rotateTargets.Length; i++)
            {
                if (rotateTargets[i] != null)
                {
                    targetDefaultLocalRotations[i] = rotateTargets[i].localRotation;
                }
                else
                {
                    targetDefaultLocalRotations[i] = Quaternion.identity;
                }
            }
        }

        savedDefaultRotation = true;
    }

    public void SetLookDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        lookDirection = direction.normalized;
        hasLookDirection = true;
        isRestoring = false;

        if (!initializedRotation)
        {
            currentWorldRotation = GetCurrentWorldRotation();
            initializedRotation = true;
        }
    }

    public void ClearLookDirection()
    {
        hasLookDirection = false;

        if (!restoreRotationOnClear)
        {
            isRestoring = false;
            return;
        }

        if (instantRestore)
        {
            RestoreDefaultLocalRotationsImmediate();
            isRestoring = false;
            return;
        }

        SaveRestoreStartLocalRotations();

        restoreTimer = 0f;
        isRestoring = true;
    }

    private void SaveRestoreStartLocalRotations()
    {
        selfRestoreStartLocalRotation = transform.localRotation;

        if (rotateTargets != null && rotateTargets.Length > 0)
        {
            targetRestoreStartLocalRotations = new Quaternion[rotateTargets.Length];

            for (int i = 0; i < rotateTargets.Length; i++)
            {
                if (rotateTargets[i] != null)
                {
                    targetRestoreStartLocalRotations[i] =
                        rotateTargets[i].localRotation;
                }
                else
                {
                    targetRestoreStartLocalRotations[i] =
                        Quaternion.identity;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (hasLookDirection)
        {
            RotateToMoveDirection();
            return;
        }

        if (isRestoring)
        {
            RestoreDefaultLocalRotationsBlend();
        }
    }

    private void RotateToMoveDirection()
    {
        Quaternion targetWorldRotation =
            Quaternion.LookRotation(lookDirection) *
            Quaternion.Euler(rotationOffsetEuler);

        if (instantRotation)
        {
            currentWorldRotation = targetWorldRotation;
        }
        else
        {
            currentWorldRotation = Quaternion.RotateTowards(
                currentWorldRotation,
                targetWorldRotation,
                Mathf.Max(rotateSpeed, 1f) * Time.deltaTime
            );
        }

        ApplyWorldRotation(currentWorldRotation);
    }

    private void RestoreDefaultLocalRotationsBlend()
    {
        if (!savedDefaultRotation)
        {
            isRestoring = false;
            return;
        }

        restoreTimer += Time.deltaTime;

        float duration = Mathf.Max(restoreDuration, 0.01f);
        float t = Mathf.Clamp01(restoreTimer / duration);

        // 滑らかに戻したい場合
        t = Mathf.SmoothStep(0f, 1f, t);

        if (rotateTargets == null || rotateTargets.Length == 0)
        {
            transform.localRotation = Quaternion.Slerp(
                selfRestoreStartLocalRotation,
                selfDefaultLocalRotation,
                t
            );
        }
        else
        {
            for (int i = 0; i < rotateTargets.Length; i++)
            {
                Transform target = rotateTargets[i];

                if (target == null) continue;
                if (targetDefaultLocalRotations == null) continue;
                if (targetRestoreStartLocalRotations == null) continue;
                if (i >= targetDefaultLocalRotations.Length) continue;
                if (i >= targetRestoreStartLocalRotations.Length) continue;

                target.localRotation = Quaternion.Slerp(
                    targetRestoreStartLocalRotations[i],
                    targetDefaultLocalRotations[i],
                    t
                );
            }
        }

        if (t >= 1f)
        {
            RestoreDefaultLocalRotationsImmediate();
            isRestoring = false;
            initializedRotation = false;
        }
    }

    private void RestoreDefaultLocalRotationsImmediate()
    {
        if (!savedDefaultRotation) return;

        if (rotateTargets == null || rotateTargets.Length == 0)
        {
            transform.localRotation = selfDefaultLocalRotation;
            return;
        }

        for (int i = 0; i < rotateTargets.Length; i++)
        {
            Transform target = rotateTargets[i];

            if (target == null) continue;
            if (targetDefaultLocalRotations == null) continue;
            if (i >= targetDefaultLocalRotations.Length) continue;

            target.localRotation = targetDefaultLocalRotations[i];
        }
    }

    private Quaternion GetCurrentWorldRotation()
    {
        if (rotateTargets != null && rotateTargets.Length > 0)
        {
            for (int i = 0; i < rotateTargets.Length; i++)
            {
                if (rotateTargets[i] != null)
                {
                    return rotateTargets[i].rotation;
                }
            }
        }

        return transform.rotation;
    }

    private void ApplyWorldRotation(Quaternion worldRotation)
    {
        if (rotateTargets == null || rotateTargets.Length == 0)
        {
            transform.rotation = worldRotation;
            return;
        }

        for (int i = 0; i < rotateTargets.Length; i++)
        {
            if (rotateTargets[i] == null) continue;

            rotateTargets[i].rotation = worldRotation;
        }
    }
}