using UnityEngine;

public class SC_PlayerCamera : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private GameObject goMainCamera;

    [Header("Setting")]
    [SerializeField] private Vector3 v3CameraOffset = new Vector3(0, 10, -10);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(goMainCamera == null) goMainCamera = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        goMainCamera.transform.position = transform.position + v3CameraOffset;
        goMainCamera.transform.LookAt(transform.position);
    }
}
