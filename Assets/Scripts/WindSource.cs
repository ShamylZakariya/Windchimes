using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class WindSource : MonoBehaviour
{
    [SerializeField] Transform target = null;
    [SerializeField] ChimeBell[] bells = null;

    [Header("Wind Properties")]
    [SerializeField] float windVelocity = 1;
    [SerializeField] float windParticleMass = 0.001f;

    private List<WindParticle> _particles = new List<WindParticle>();
    private Dictionary<int, List<Vector3>> _particlePaths = new Dictionary<int, List<Vector3>>();
    private float _secondsUntilNextParticlePruning;
    private const float _particlePruningPeriod = 1;
    private Vector3 _particlePruningBoundsOrigin;
    private float _particlePruningBoundsRadius2;

    void Start()
    {
        _secondsUntilNextParticlePruning = _particlePruningPeriod;

        Bounds b = target.CalculateBounds();
        b.size *= 2;
        float radius = Mathf.Max(new float[] { b.size.x, b.size.y, b.size.z });
        _particlePruningBoundsOrigin = b.center;
        _particlePruningBoundsRadius2 = radius * radius;
    }

    void Update()
    {
        // make the "fan" face the chime - this is just for appearance
        if (target != null)
        {
            Bounds targetBounds = target.CalculateBounds();
            transform.LookAt(targetBounds.center);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // for now send a single pulse to the center of the first bell
            ChimeBell bell = bells[0];
            float wiggle = 0.02f;
            Vector3 start = transform.position + Random.onUnitSphere * wiggle;
            Vector3 dir = ((bell.transform.position + Random.onUnitSphere * wiggle) - transform.position).normalized;
            EmitWindParticle(start, dir);
        }

        UpdateWindParticles();

        _secondsUntilNextParticlePruning -= Time.deltaTime;
        if (_secondsUntilNextParticlePruning <= 0)
        {
            _secondsUntilNextParticlePruning = _particlePruningPeriod;
            PruneWindParticles();
        }
    }

    void UpdateWindParticles()
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            WindParticle particle = _particles[i];
            if (!particle.alive) { continue; }

            List<Vector3> path = _particlePaths[particle.id];

            // raycast against bell the short ray
            Vector3 position = particle.position;
            Vector3 nextPosition = position + (particle.dir * particle.velocity * Time.deltaTime);
            float distanceTraveled = Vector3.Distance(nextPosition, position);

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
                    particle.alive = false;
                    _particles[i] = particle;
                    continue;
                }
            }

            ChimeBell bell = bells[0];

            if (CylinderIntersection.Ray_Backtracking(position, particle.dir, bell.Top, bell.Bottom, bell.Radius,
                out Vector3 intersection, out Vector3 normal, out float distance) && distance < distanceTraveled)
            {
                // first add intersection point to the path so we can render it
                path.Add(intersection);

                // we have a collision - apply impulse and deflect and reduce power of the particle
                // compute the reflection (this is the new particle direction) and the incidence
                // incidence goeas from -1 to 1 where -1 means we hit square on, and 1 means we 
                // perfectly grazed the surface imparting no energy

                Vector3 reflection = Vector3.Reflect(particle.dir, normal);
                float incidence = Vector3.Dot(particle.dir, reflection);

                // remap the incidence such that a value of 1 means full energy transfer and 0 means none
                float energyTransfer = 1 - ((incidence + 1f) / 2f);
                Vector3 force = particle.dir * particle.velocity * particle.mass * energyTransfer;

                bell.Rigidbody.AddForceAtPosition(force, intersection, ForceMode.Impulse);


                // reduce particle velocity
                particle.velocity *= 1 - energyTransfer;
                particle.dir = reflection;
                particle.alive = particle.velocity > 1e-5f;

                // update the particle position along the path up to collision and then the 
                // remaining distance along the reflection
                nextPosition = intersection + (reflection * Mathf.Max(distanceTraveled - distance, 0.01f));
            }

            // update the particle
            particle.position = nextPosition;
            path.Add(nextPosition);
            _particles[i] = particle;

            // now draw this particle's path
            Vector3 a = path[0];
            for (int j = 1, N = path.Count; j < N; j++)
            {
                Vector3 b = path[j];
                RuntimeDebugDraw.Draw.DrawLine(a, b, particle.color);
                a = b;
            }
        }
    }

    void PruneWindParticles()
    {
        foreach (WindParticle p in _particles)
        {
            if (!p.alive)
            {
                _particlePaths.Remove(p.id);
            }
        }
        _particles = _particles.Where((p) => { return p.alive; }).ToList();
    }

    private int _particleId = 0;

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
            // color = Random.ColorHSV(0, 1, 0.9f, 1f, 1f, 1f),
            hasEnteredTargetBounds = false,
        };

        _particlePaths.Add(p.id, (new Vector3[] { startPosition }).ToList());
        _particles.Add(p);
    }
}
