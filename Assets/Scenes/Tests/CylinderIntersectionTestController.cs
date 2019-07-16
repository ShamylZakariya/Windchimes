using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CylinderIntersectionTestController : MonoBehaviour
{
    [SerializeField] GameObject cylinder;
    [SerializeField] Transform cylinderEndpointA;
    [SerializeField] Transform cylinderEndpointB;
    [SerializeField] Transform cylinderRadius;

    [SerializeField] Transform ray_a;
    [SerializeField] Transform ray_b;

    private CapsuleCollider _collider;
    private float _spinRate = 5f;

    void Start()
    {
        _collider = cylinder.GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        cylinder.transform.Rotate(Vector3.up, _spinRate * Time.deltaTime);
        cylinder.transform.Rotate(Vector3.right, _spinRate * Time.deltaTime);
        cylinder.transform.Rotate(Vector3.forward, _spinRate * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestCylinderCollision(ray_a.position, ray_b.position);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            TestStructure();
        }
    }

    void TestStructure() 
    {
        Vector3 a = cylinder.transform.TransformPoint(new Vector3(0,_collider.height / 2,0));
        Vector3 b = cylinder.transform.TransformPoint(new Vector3(0,-_collider.height / 2,0));
        float radius = Vector3.Distance(cylinder.transform.position, cylinder.transform.TransformPoint(new Vector3(_collider.radius,0,0)));

        float distA = Vector3.Distance(a, cylinderEndpointA.position);
        float distB = Vector3.Distance(b, cylinderEndpointB.position);
        float distR = Mathf.Abs(radius - Vector3.Distance(cylinderRadius.position, cylinderEndpointB.position));

        Debug.LogFormat("[TestStructure] - distA: {0} distB: {1} distR: {2}", distA, distB, distR);
        Debug.LogFormat("a: {0} b: {1} radius: {2} -- ref a: {3} ref b: {4} ref radius: {5}",
            a, b, radius,
            cylinderEndpointA.position, cylinderEndpointB.position, Vector3.Distance(cylinderRadius.position, cylinderEndpointB.position)
        );
    }

    void TestCylinderCollision(Vector3 a, Vector3 b)
    {
        Vector3 unityIntersection, unityNormal;
        float unityDistance;
        bool unityDidHit = RayCylinder(a, b, out unityIntersection, out unityNormal, out unityDistance);

        Vector3 testIntersection, testNormal;
        float testDistance;
        bool testDidHit = TestRayCylinder(a, b, out testIntersection, out testNormal, out testDistance);

        bool displayDetails = false;

        if (unityDidHit && testDidHit)
        {
            float intersectionError = Vector3.Distance(unityIntersection, testIntersection);
            float normalError = Vector3.Angle(unityNormal, testNormal);
            float distanceError = Mathf.Abs(unityDistance - testDistance);
            Debug.LogFormat("BOTH HIT! intersection error: {0} angle error: {1} distance error: {2}",
                intersectionError, normalError, distanceError);

            displayDetails = intersectionError > 1e-3 || normalError > 1e-3 || distanceError > 1e-3;
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

    bool RayCylinder(Vector3 a, Vector3 b, out Vector3 intersection, out Vector3 normal, out float distance)
    {
        float length = (b - a).magnitude;
        Ray ray = new Ray(a, (b - a).normalized);
        RaycastHit hitInfo;

        bool didHit = _collider.Raycast(ray, out hitInfo, length);
        intersection = hitInfo.point;
        normal = hitInfo.normal;
        distance = hitInfo.distance;

        return didHit;
    }

    bool TestRayCylinder(Vector3 a, Vector3 b, out Vector3 intersection, out Vector3 normal, out float distance)
    {
        // Vector3 ca = cylinderEndpointA.position;
        // Vector3 cb = cylinderEndpointB.position;
        // float cr = Vector3.Distance(cylinderRadius.position, cylinderEndpointB.position);

        Vector3 ca = cylinder.transform.TransformPoint(new Vector3(0,_collider.height / 2,0));
        Vector3 cb = cylinder.transform.TransformPoint(new Vector3(0,-_collider.height / 2,0));
        float cr = Vector3.Distance(cylinder.transform.position, cylinder.transform.TransformPoint(new Vector3(_collider.radius,0,0)));

        return CylinderIntersection.Line(a, b, ca, cb, cr, out intersection, out normal, out distance);
    }

}