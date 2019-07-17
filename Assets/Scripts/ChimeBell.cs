using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ChimeBell : MonoBehaviour
{
    private CapsuleCollider _collider;
    private Rigidbody _body;

    private Vector3 _a;
    private Vector3 _b;
    private float _radius;
    private float _length;

    void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
        _body = GetComponent<Rigidbody>();

        _a = new Vector3(0, _collider.height / 2, 0);
        _b = new Vector3(0, -_collider.height / 2, 0);
        _radius = transform.TransformVector(new Vector3(_collider.radius, 0, 0)).magnitude;
        _length = Vector3.Distance(Top, Bottom);
    }

    public Collider Collider { get { return _collider; } }
    public Rigidbody Rigidbody { get { return _body; } }

    /// <returns>Get the top of the bell in world space</returns>
    public Vector3 Top { get { return transform.TransformPoint(_a); } }

    /// <returns>Get the bottom of the bell in world space</returns>
    public Vector3 Bottom { get { return transform.TransformPoint(_b); } }

    /// <returns>Get the radius of the bell in world space</returns>
    public float Radius { get { return _radius; } }

    /// <returns>Get the length of the bell in world space</returns>
    public float Length { get { return _length; } }

}