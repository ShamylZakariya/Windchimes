using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ChimeBell : MonoBehaviour {
    private Collider _collider;
    private Rigidbody _body;

    void Awake() {
        _collider = GetComponent<Collider>();
        _body = GetComponent<Rigidbody>();
    }

    public Collider Collider { get { return _collider; }}
    public Rigidbody Rigidbody { get { return _body; }}
}