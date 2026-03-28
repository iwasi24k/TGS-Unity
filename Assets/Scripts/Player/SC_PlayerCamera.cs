using UnityEngine;

public class SC_PlayerCamera : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Camera goMainCamera;
    [Tooltip("ターゲット情報"), SerializeField] SC_PlayerTarget scTarget;

    [Header("Setting")]
    [SerializeField] private Vector3 CameraOffset = new Vector3(0, 10, -10);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(goMainCamera == null) goMainCamera = Camera.main;
        if(scTarget == null) scTarget = this.GetComponent<SC_PlayerTarget>();
    }

    // Update is called once per frame
    void Update()
    {
        goMainCamera.transform.position = transform.position + CameraOffset;
        goMainCamera.transform.LookAt(transform.position);
    }
}
