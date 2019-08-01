using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bell : MonoBehaviour
{
    [SerializeField]
    private BellSynthesizer bellSynthesizer = null;

    [Header("Manual dinging support for testing")]
    [SerializeField]
    private bool clickToRing = false;
    [SerializeField]
    private float clickForce = 0.1f;

    private Collider _collider;
    private Rigidbody _body;


    public Collider Collider { get { return _collider; } }
    public Rigidbody Rigidbody { get { return _body; } }
    public BellSynthesizer BellSynthesizer { get { return bellSynthesizer; }}


    virtual protected void Awake()
    {
        _collider = GetComponent<Collider>();
        _body = GetComponent<Rigidbody>();
    }

    virtual protected void Update()
    {
        if (clickToRing && Input.GetMouseButtonDown(0))
        {
            Ray mousePickRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mousePickRay, out RaycastHit hit))
            {
                BellSynthesizer bell = hit.collider.gameObject.GetComponentInChildren<BellSynthesizer>();
                if (bell == bellSynthesizer && bell != null)
                {
                    Ring(hit.point, clickForce);
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 1e-5f)
        {
            Vector3 p = collision.GetContact(0).point;
            Ring(p, collision.relativeVelocity.magnitude);
        }
    }

    /// <summary>
    /// The WindSource needs *good* wind particle emission points for bells. 
    /// Bell implementations should "cast a shadow" on the provided plane, and generate
    /// an emission point at random in that shadow silhouette region which, if 
    /// extended from the plane along its normal would likely hit the bell.
    /// </summary>
    /// <param name="plane">The plane onto which to cast a shadow</param>
    /// <returns>A point on the plane which would be a good candidate to emit a wind particle along the plane normal which will likely hit the bell</returns>
    virtual public Vector3 GenerateRandomEmissionPointOnPlane(Plane plane)
    {
        throw new System.NotImplementedException();
    }


    /// <summary>
    /// Generate and play an appropriate ringing tone for an impact on this bell
    /// </summary>
    /// <param name="impactPoint">location of impact on bell in world coordinates</param>
    /// <param name="force">force of impact</param>
    virtual public void Ring(Vector3 impactPoint, float force)
    {
        throw new System.NotImplementedException();
    }
}