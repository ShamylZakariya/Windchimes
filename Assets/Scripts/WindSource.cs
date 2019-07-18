﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class WindSource : MonoBehaviour
{
    [SerializeField] GameObject windChime = null;

    [Header("Fan Properties")]
    [SerializeField] GameObject windSourceVisual;

    [Header("Wind Properties")]
    [SerializeField] float windVelocity = 1;
    [SerializeField] float windParticleMass = 0.001f;
    [SerializeField] float particlesPerSecond = 20;
    [SerializeField] bool renderParticlePaths = false;
    [SerializeField] LayerMask windTargetLayer;


    private List<WindParticle> _particles = new List<WindParticle>();
    private Dictionary<int, List<Vector3>> _particlePaths = new Dictionary<int, List<Vector3>>();
    private float _secondsUntilNextParticlePruning;
    private const float _particlePruningPeriod = 1;
    private Vector3 _particlePruningBoundsOrigin;
    private float _particlePruningBoundsRadius;
    private float _particlePruningBoundsRadius2;
    private float _secondsUntilNextParticle = 0;
    private RaycastHit[] _raycastHits;
    private ChimeBell[] _bells;
    private int _particleId = 0;
    private Plane _fanSurfacePlane;

    void Start()
    {
        //
        // collect our bells
        //

        _bells = windChime.GetComponentsInChildren<ChimeBell>().ToArray();
        _raycastHits = new RaycastHit[_bells.Length];

        _secondsUntilNextParticlePruning = _particlePruningPeriod;
        if (particlesPerSecond == 0)
        {
            Debug.Log("[WindSource::Start] - particles per second == 0, so turning on particle path rendering");
            renderParticlePaths = true;
        }
    }

    void Update()
    {
        //
        // update the bounding region to fit the bells
        //
        Bounds b = TransformExtensions.CalculateBounds(_bells);
        _particlePruningBoundsRadius = b.size.magnitude;
        _particlePruningBoundsOrigin = b.center;
        _particlePruningBoundsRadius2 = _particlePruningBoundsRadius * _particlePruningBoundsRadius;

        // point the fan at the bounding region and update the plane
        transform.LookAt(_particlePruningBoundsOrigin);
        transform.position = _particlePruningBoundsOrigin - transform.forward * _particlePruningBoundsRadius;
        _fanSurfacePlane.SetNormalAndPosition(transform.forward, transform.position);


        EmitWindParticles();
        UpdateWindParticles();
        PruneWindParticles();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_particlePruningBoundsOrigin, _particlePruningBoundsRadius);
    }

    void UpdateWindParticles()
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            WindParticle particle = _particles[i];
            if (!particle.alive) { continue; }

            List<Vector3> path = renderParticlePaths ? _particlePaths[particle.id] : null;

            //
            //  Particles are allowd to move into the target bounds, but when they exit it
            //  they are done with simulation. If a particle velocity drops to zero it is
            //  also all done. Pruning will periodically clear out dead particles.
            //

            if (!particle.hasEnteredTargetBounds)
            {
                if ((particle.position - _particlePruningBoundsOrigin).sqrMagnitude <= _particlePruningBoundsRadius2)
                {
                    particle.hasEnteredTargetBounds = true;
                }
            }
            else
            {
                if ((particle.position - _particlePruningBoundsOrigin).sqrMagnitude > _particlePruningBoundsRadius2)
                {
                    // we're done here
                    particle.alive = false;
                    _particles[i] = particle;

                    continue;
                }
            }

            //
            //  compute the travel of the particle this step. Run raycasts.
            //  If there's a collision, update particle direction and velocity
            //

            Vector3 nextPosition = particle.position + (particle.dir * particle.velocity * Time.deltaTime);
            float distanceTraveled = Vector3.Distance(nextPosition, particle.position);
            int hitCount = Physics.RaycastNonAlloc(new Ray(particle.position, particle.dir), _raycastHits, distanceTraveled, windTargetLayer, QueryTriggerInteraction.Ignore);

            for (int hit = 0; hit < hitCount; hit++)
            {
                //
                // we have a collision - apply impulse and deflect and reduce power of the particle
                // compute the reflection (this is the new particle direction) and the incidence
                // incidence goeas from -1 to 1 where -1 means we hit square on, and 1 means we 
                // perfectly grazed the surface imparting no energy
                //

                RaycastHit hitInfo = _raycastHits[hit];

                // first add intersection point to the path so we can render it
                if (renderParticlePaths) { path.Add(hitInfo.point); }

                Vector3 reflection = Vector3.Reflect(particle.dir, hitInfo.normal);
                float incidence = Vector3.Dot(particle.dir, reflection);

                // remap the incidence such that a value of 1 means full energy transfer and 0 means none
                float energyTransfer = 1 - ((incidence + 1f) / 2f);
                Vector3 force = particle.dir * particle.velocity * particle.mass * energyTransfer;

                hitInfo.rigidbody.AddForceAtPosition(force, hitInfo.point, ForceMode.Impulse);

                // reduce particle velocity
                particle.velocity *= 1 - energyTransfer;
                particle.dir = reflection;
                particle.alive = particle.velocity > 1e-5f;

                // update the particle position along the path up to collision and then the 
                // remaining distance along the reflection
                nextPosition = hitInfo.point + (reflection * Mathf.Max(distanceTraveled - hitInfo.distance, 0.01f));
            }

            //
            // update the particle and store
            //

            particle.position = nextPosition;
            if (renderParticlePaths) { path.Add(nextPosition); }
            _particles[i] = particle;

            //
            // now draw this particle's path
            //

            if (renderParticlePaths)
            {
                Vector3 a = path[0];
                for (int j = 1, N = path.Count; j < N; j++)
                {
                    Vector3 b = path[j];
                    RuntimeDebugDraw.Draw.DrawLine(a, b, particle.color);
                    a = b;
                }
            }
        }
    }

    void PruneWindParticles()
    {
        _secondsUntilNextParticlePruning -= Time.deltaTime;
        if (_secondsUntilNextParticlePruning <= 0)
        {
            _secondsUntilNextParticlePruning = _particlePruningPeriod;

            foreach (WindParticle p in _particles)
            {
                if (!p.alive)
                {
                    _particlePaths.Remove(p.id);
                }
            }
            _particles = _particles.Where((p) => { return p.alive; }).ToList();
        }
    }

    void EmitWindParticles()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EmitWindParticle(EmissionPointForChimeBell(_bells[0]), transform.forward);
        }

        _secondsUntilNextParticle -= Time.deltaTime;
        if ((particlesPerSecond > 0 && _secondsUntilNextParticle < 0))
        {
            _secondsUntilNextParticle = 1 / particlesPerSecond;

            foreach (ChimeBell bell in _bells)
            {
                EmitWindParticle(EmissionPointForChimeBell(bell), transform.forward);
            }
        }
    }

    Vector3 EmissionPointForChimeBell(ChimeBell bell)
    {
        //
        // project bell's A and B points to our emission plane,
        // generate a point in a circle of radius == chime bell radius, 
        // a random distance along the line from a to b
        //

        Vector3 a = _fanSurfacePlane.ClosestPointOnPlane(bell.Top);
        Vector3 b = _fanSurfacePlane.ClosestPointOnPlane(bell.Bottom);
        Vector3 o = Vector3.Lerp(a, b, Random.Range(0f, 1f));
        Vector2 c = Random.insideUnitCircle * bell.Radius;
        Vector3 e = o + transform.TransformDirection(new Vector3(c.x, c.y, 0));
        return e;
    }

    void EmitWindParticle(Vector3 startPosition, Vector3 startDirection)
    {
        WindParticle p = new WindParticle()
        {
            id = _particleId++,
            position = startPosition,
            dir = startDirection,
            velocity = windVelocity,
            mass = windParticleMass,
            alive = true,
            color = Color.black,
            hasEnteredTargetBounds = false,
        };

        _particlePaths.Add(p.id, (new Vector3[] { startPosition }).ToList());
        _particles.Add(p);
    }
}
