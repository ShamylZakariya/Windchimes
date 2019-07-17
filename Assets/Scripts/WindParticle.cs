using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WindParticle {
    public int id;
    public Vector3 position;
    public Vector3 dir;
    public float velocity;
    public float mass;
    public bool alive;
    public Color color;
    public bool hasEnteredTargetBounds;
}
