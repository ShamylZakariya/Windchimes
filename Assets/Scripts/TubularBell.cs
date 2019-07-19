using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class TubularBell : Bell
{
    private CapsuleCollider _capsuleCollider;

    private Vector3 _top;
    private Vector3 _bottom;
    private float _radius;
    private float _length;

    public CapsuleCollider CapsuleCollider { get { return _capsuleCollider; } }

    /// <returns>Get the top of the bell in world space</returns>
    public Vector3 Top { get { return transform.TransformPoint(_top); } }

    /// <returns>Get the bottom of the bell in world space</returns>
    public Vector3 Bottom { get { return transform.TransformPoint(_bottom); } }

    /// <returns>Get the radius of the bell in world space</returns>
    public float Radius { get { return _radius; } }

    /// <returns>Get the length of the bell in world space</returns>
    public float Length { get { return _length; } }


    override protected void Awake()
    {
        base.Awake();

        _capsuleCollider = GetComponent<CapsuleCollider>();
        _top = new Vector3(0, _capsuleCollider.height / 2, 0);
        _bottom = new Vector3(0, -_capsuleCollider.height / 2, 0);
        _radius = transform.TransformVector(new Vector3(_capsuleCollider.radius, 0, 0)).magnitude;
        _length = Vector3.Distance(Top, Bottom);
    }

    override public Vector3 GenerateRandomEmissionPointOnPlane(Plane plane)
    {
        //
        // project bell's A and B points to our emission plane,
        // generate a point in a circle of radius == chime bell radius, 
        // a random distance along the line from a to b
        //

        Vector3 a = plane.ClosestPointOnPlane(Top);
        Vector3 b = plane.ClosestPointOnPlane(Bottom);
        Vector3 o = Vector3.Lerp(a, b, UnityEngine.Random.Range(0f, 1f));
        Vector2 c = UnityEngine.Random.insideUnitCircle * Radius;
        Vector3 e = o + transform.TransformDirection(new Vector3(c.x, c.y, 0));
        return e;
    }


    override public void Ring(Vector3 impactPoint, float force)
    {
        float extent = Mathf.Clamp01((impactPoint - Top).magnitude / Length);
        Debug.LogFormat("[TubularBell({0})::Ring] - impactPoint:{1} extent: {2} force: {3}", gameObject.name, impactPoint, extent, force);
    }
}