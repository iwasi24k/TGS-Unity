using UnityEngine;

public class SC_PartFollowTarget : MonoBehaviour
{
    [Header("Follow Target")]
    [Tooltip("追従先")]
    [SerializeField] private Transform target;

    [Header("Follow Setting")]
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;

    [Tooltip("位置補正。ターゲット基準のローカル座標")]
    [SerializeField] private Vector3 localPositionOffset = Vector3.zero;

    [Tooltip("回転補正")]
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    [Tooltip("trueなら常に追従。falseなら外部から有効化された時だけ追従")]
    [SerializeField] private bool alwaysFollow = false;

    [Tooltip("追従をなめらかにする")]
    [SerializeField] private bool useBlend = false;

    [SerializeField] private float positionBlendSpeed = 20f;
    [SerializeField] private float rotationBlendSpeed = 20f;

    private bool followEnabled;

    public void SetFollowEnabled(bool enabled)
    {
        followEnabled = enabled;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        if (!alwaysFollow && !followEnabled) return;

        Vector3 targetPosition =
            target.TransformPoint(localPositionOffset);

        Quaternion targetRotation =
            target.rotation * Quaternion.Euler(rotationOffsetEuler);

        if (followPosition)
        {
            if (useBlend)
            {
                float t = 1f - Mathf.Exp(-positionBlendSpeed * Time.deltaTime);

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    t
                );
            }
            else
            {
                transform.position = targetPosition;
            }
        }

        if (followRotation)
        {
            if (useBlend)
            {
                float t = 1f - Mathf.Exp(-rotationBlendSpeed * Time.deltaTime);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    t
                );
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }
}
