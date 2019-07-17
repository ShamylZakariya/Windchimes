using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ChimeBell : MonoBehaviour
{
    [SerializeField] Transform _top;
    [SerializeField] Transform _bottom;
    [SerializeField] Transform _radiusMarker;
    [SerializeField] bool doItSmart = true;

    private CapsuleCollider _collider;
    private Rigidbody _body;

    private Vector3 _a;
    private Vector3 _b;
    private float _radius;

    void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
        _body = GetComponent<Rigidbody>();

        if (doItSmart)
        {
            _a = new Vector3(0, _collider.height / 2, 0);
            _b = new Vector3(0, -_collider.height / 2, 0);
            _radius = transform.TransformVector(new Vector3(_collider.radius, 0, 0)).magnitude;
        }
        else
        {
            _radius = Vector3.Distance(_radiusMarker.position, _bottom.position);
        }
    }

    public Collider Collider { get { return _collider; } }
    public Rigidbody Rigidbody { get { return _body; } }

    public Vector3 Top { get { return doItSmart ? transform.TransformPoint(_a) : _top.position; } }
    public Vector3 Bottom { get { return doItSmart ? transform.TransformPoint(_b) : _bottom.position; } }
    public float Radius { get { return _radius; } }
}