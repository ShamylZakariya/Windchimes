using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class CylinderIntersection
{
    /// <summary>
    /// Compute intersection of an infinite ray with a capped cylinder
    /// adapted from https://stackoverflow.com/questions/4078401/trying-to-optimize-line-vs-cylinder-intersection
    /// </summary>
    /// <param name="start">Ray origin</param>
    /// <param name="dir">Ray direction (normalized)</param>
    /// <param name="A">one end of the cylinder</param>
    /// <param name="B">other end of the cylinder</param>
    /// <param name="r">radius of cylinder</param>
    /// <param name="intersection">If the ray intesects, location of intersection</param>
    /// <param name="normal">If the ray intesects, normal of cylinder at intersection</param>
    /// <param name="distance">If the ray intesects, distance of intersection from ro</param>
    /// /// <returns>True if the ray intersects the cylinder</returns>
    public static bool Ray(Vector3 start, Vector3 dir, Vector3 A, Vector3 B, float r, out Vector3 intersection, out Vector3 normal, out float distance)
    {
        distance = 0;
        normal = Vector3.zero;
        intersection = Vector3.zero;

        Vector3 AB = B - A;
        Vector3 AO = start - A;
        Vector3 AOxAB = Vector3.Cross(AO, AB); //AO.crossProduct(AB);
        Vector3 VxAB = Vector3.Cross(dir, AB); //dir.crossProduct(AB);
        float ab2 = Vector3.Dot(AB, AB); //AB.dotProduct(AB);
        float a = Vector3.Dot(VxAB, VxAB); //VxAB.dotProduct(VxAB);
        float b = 2 * Vector3.Dot(VxAB, AOxAB); //2 * VxAB.dotProduct(AOxAB);
        float c = Vector3.Dot(AOxAB, AOxAB) - (r * r * ab2); //AOxAB.dotProduct(AOxAB) - (r * r * ab2);
        float d = b * b - 4 * a * c;
        if (d < 0) return false;
        float time = (-b - Mathf.Sqrt(d)) / (2 * a);
        if (time < 0) return false;

        intersection = start + dir * time;        /// intersection point
        Vector3 projection = A + (Vector3.Dot(AB, (intersection - A)) / ab2) * AB; //A + (AB.dotProduct(intersection - A) / ab2) * AB; /// intersection projected onto cylinder axis
        if ((projection - A).magnitude + (B - projection).magnitude > AB.magnitude) return false; /// THIS IS THE SLOW SAFE WAY

        normal = (intersection - projection).normalized;
        distance = time; /// at last
        return true;
    }

    public static bool Line(Vector3 start, Vector3 end, Vector3 A, Vector3 B, float r, out Vector3 intersection, out Vector3 normal, out float distance)
    {
        Vector3 dir = end - start;
        float length = dir.magnitude + 1e-5f;
        dir /= length;

        return Ray(start, dir, A, B, r, out intersection, out normal, out distance) && distance <= length;
    }


}