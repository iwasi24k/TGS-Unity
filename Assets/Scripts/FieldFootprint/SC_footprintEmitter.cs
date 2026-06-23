using UnityEngine;

public class SC_footprintEmitter : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 0.2f;
    [SerializeField] private SC_footprintSetting footprintSetting;
    [SerializeField] private bool useAnimationEvent = false;

    private bool wasGrounded = false;


    void Update()
    {
        if (useAnimationEvent) return;

        CheckGroundContact();
    }

    private void CheckGroundContact()
    {
        bool grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            raycastDistance,
            groundLayer
            );

        if (grounded && !wasGrounded)
        {

        }

        wasGrounded = grounded;
    }

    public void CreateFootprint()
    {
        if(!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            return;
        }

        FootprintStampData data = new FootprintStampData{};

        data.uv = hit.textureCoord;
        data.radius = footprintSetting.radius;
        data.depth = footprintSetting.depth;

        data.rotation = transform.eulerAngles.y;

        SC_footprintManager.instance.EnqueueStamp(data);

    }
}
