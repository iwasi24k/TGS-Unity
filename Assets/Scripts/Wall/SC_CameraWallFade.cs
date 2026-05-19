using System.Collections.Generic;
using UnityEngine;

public class CameraWallFade : MonoBehaviour
{
    private Transform target;

    private List<SC_FadeObject> fadedObjects = new List<SC_FadeObject>();

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void LateUpdate()
    {
        // ëOÉtÉåÅ[ÉÄÇÃìßñæâªâèú
        foreach (SC_FadeObject obj in fadedObjects)
        {
            obj.FadeIn();
        }

        fadedObjects.Clear();

        Vector3 dir = target.position - transform.position;
        float distance = dir.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(
            transform.position,
            dir.normalized,
            distance
        );

        foreach (RaycastHit hit in hits)
        {
            SC_FadeObject fadeObj = hit.collider.GetComponent<SC_FadeObject>();

            if (fadeObj != null)
            {
                fadeObj.FadeOut();
                fadedObjects.Add(fadeObj);
            }
        }
    }
}