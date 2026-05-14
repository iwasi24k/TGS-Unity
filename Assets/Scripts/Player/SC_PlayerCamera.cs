using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PlayerCamera : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Camera goMainCamera;
    [Tooltip("ターゲット情報"), SerializeField] SC_PlayerTarget scTarget;
    [Tooltip("移動入力"), SerializeField] InputActionReference iaMove;

    [Header("Setting")]
    [Tooltip("非ターゲット時のカメラ位置"),SerializeField] private Vector3 NonTargetCameraOffset = new Vector3(0, 3, -5);
    [Tooltip("ターゲット時のカメラ位置"), SerializeField] private Vector3 TargetCameraOffset = new Vector3(0, 2, -2);
    [Tooltip("カメラ移動速度"),SerializeField] private float CameraMoveSpeed = 5f;
    [Tooltip("カメラ移動速度"), SerializeField] private float TargetingCameraMoveSpeed = 10f;
    [Tooltip("カメラ回転速度"), SerializeField] private float CameraRotateSpeed = 8f;
    [Tooltip("横移動時のカメラ位置補正"),SerializeField] private float CameraHorizontalOffset = 0.5f;

    bool isTargeting = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(goMainCamera == null) goMainCamera = Camera.main;
        if(scTarget == null) scTarget = this.GetComponent<SC_PlayerTarget>();

        if(iaMove == null)
        {
            Debug.LogError("移動用のInputActionReferenceがアタッチされていません。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        var inputVal = iaMove.action.ReadValue<Vector2>();
        var target = scTarget.GetCurrentTarget();

        if(target == null)
        {
            // 目標位置（プレイヤー + オフセット）
            Vector3 desiredPos = transform.position + NonTargetCameraOffset;
            // 閾値以内に到達したら Lerp ではなく直に追従する
            const float snapThreshold = 0.4f;
            if (Mathf.Abs(goMainCamera.transform.position.y - desiredPos.y) < snapThreshold || !isTargeting)
            {
                isTargeting = false;
                goMainCamera.transform.position = Vector3.Lerp(goMainCamera.transform.position, desiredPos, 50f);
            }
            else
            {
                goMainCamera.transform.position = Vector3.Lerp(goMainCamera.transform.position, desiredPos, Time.deltaTime * CameraMoveSpeed);
            }
            goMainCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
        }
        else
        {
            if(!isTargeting)
            {
                isTargeting = true;
            }

            // ターゲット時：入力による横オフセットを含めた目標位置を算出
            var moveoffset = TargetCameraOffset + new Vector3(inputVal.x * CameraHorizontalOffset, 0.0f, 0.0f);

            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            Vector3 flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up);
            if (flatForward.sqrMagnitude < 0.0001f) flatForward = Vector3.forward;
            if (flatRight.sqrMagnitude < 0.0001f) flatRight = Vector3.right;
            flatForward.Normalize();
            flatRight.Normalize();

            Vector3 camTargetPos = this.transform.position + flatRight * moveoffset.x + Vector3.up * moveoffset.y + flatForward * moveoffset.z;
            goMainCamera.transform.position = Vector3.Lerp(goMainCamera.transform.position, camTargetPos, Time.deltaTime * TargetingCameraMoveSpeed);

            Quaternion targetRot = Quaternion.LookRotation(target.transform.position + new Vector3(0.0f,1.5f,0.0f) - goMainCamera.transform.position);
            goMainCamera.transform.rotation = Quaternion.Slerp(goMainCamera.transform.rotation, targetRot, Time.deltaTime * CameraRotateSpeed);
        }
    }
}
