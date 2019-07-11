using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSource : MonoBehaviour
{
    [SerializeField] Transform target = null;
    [SerializeField] ChimeBell[] bells = null;

    [Header("Wind Properties")]
    [SerializeField] float windVelocity = 1;
    [SerializeField] float windParticleMass = 0.001f;

    void Start()
    {
        Debug.LogFormat("sleepThreshold: {0}", Physics.sleepThreshold);
    }


    void Update()
    {
        if (target != null)
        {
            Bounds targetBounds = target.CalculateBounds();
            transform.LookAt(targetBounds.center);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            // for now send a single pulse to the center of the first bell
            ChimeBell bell = bells[0];
            Bounds b = bell.transform.CalculateBounds();

            if (bell.Collider.Raycast(new Ray(transform.position, (b.center - transform.position).normalized), out var hitInfo, float.MaxValue)) {        
                Vector3 dir = (hitInfo.point - transform.position).normalized;
                Vector3 force = dir * windVelocity * windParticleMass;
                bell.Rigidbody.AddForceAtPosition(force, hitInfo.point, ForceMode.Impulse);
            }
            else 
            {
                Debug.LogFormat("[WindSource] - raycast missed bell");
            }
        }
    }
}
