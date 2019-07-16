using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CylinderIntersectionTestController : MonoBehaviour
{
    [SerializeField] Collider cylinderCollider;
    [SerializeField] Transform cylinderEndpointA;
    [SerializeField] Transform cylinderEndpointB;
    [SerializeField] Transform cylinderRadius;

    [SerializeField] Transform ray_a;
    [SerializeField] Transform ray_b;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestCylinderCollision(ray_a.position, ray_b.position);
        }
    }

    void TestCylinderCollision(Vector3 a, Vector3 b)
    {
        Vector3 unityIntersection, unityNormal;
        bool unityDidHit = RayCylinder(a, b, out unityIntersection, out unityNormal);

        Vector3 testIntersection, testNormal;
        bool testDidHit = TestRayCylinder(a, b, out testIntersection, out testNormal);

        bool displayDetails = false;

        if (unityDidHit && testDidHit)
        {
            float distance = Vector3.Distance(unityIntersection, testIntersection);
            float angleDistance = Vector3.Angle(unityNormal, testNormal);
            Debug.LogFormat("BOTH HIT! distance between intersection points: {0} angle between normals: {1}",
                distance, angleDistance);

            displayDetails = distance > 1e-3 || angleDistance > 1e-3;
        }
        else if (!unityDidHit && !testDidHit)
        {
            Debug.LogFormat("BOTH MISSED!");
        }
        else
        {
            Debug.LogFormat("DISAGREEMENT");
            displayDetails = true;
        }

        if (displayDetails)
        {
            Debug.LogFormat("DETAILS a {0} b {1} UNITY {2} @ {3} {4} -- TEST {5} {6} {7}",
                a, b, unityDidHit, unityIntersection, unityNormal,
                testDidHit, testIntersection, testNormal
            );
        }
    }

    bool RayCylinder(Vector3 a, Vector3 b, out Vector3 intersection, out Vector3 normal)
    {
        float length = (b - a).magnitude;
        Ray ray = new Ray(a, (b - a).normalized);
        RaycastHit hitInfo;

        bool didHit = cylinderCollider.Raycast(ray, out hitInfo, length);
        intersection = hitInfo.point;
        normal = hitInfo.normal;

        return didHit;
    }

    bool TestRayCylinder(Vector3 a, Vector3 b, out Vector3 intersection, out Vector3 normal)
    {
        Vector3 ca = cylinderEndpointA.position;
        Vector3 cb = cylinderEndpointB.position;
        float cr = Vector3.Distance(cylinderRadius.position, cylinderEndpointB.position);
        return CylinderIntersection.FiniteRay(a, b, ca, cb, cr, out intersection, out normal);
    }

}