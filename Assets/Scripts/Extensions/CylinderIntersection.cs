using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class CylinderIntersection
{
    /// <summary>
    /// Compute intersection of an infinite ray with a capped cylinder
    /// adapted from http://iquilezles.org/www/articles/intersectors/intersectors.htm
    /// </summary>
    /// <param name="ro">Ray origin</param>
    /// <param name="rd">Ray direction (normalized)</param>
    /// <param name="pa">one end of the cylinder</param>
    /// <param name="pb">other end of the cylinder</param>
    /// <param name="ra">radius of cylinder</param>
    /// <param name="intersection">If the ray intesects, location of intersection</param>
    /// <param name="normal">If the ray intesects, normal of cylinder at intersection</param>
    /// <returns>True if the ray intersects the cylinder</returns>
    public static bool InfiniteRay(in Vector3 ro, in Vector3 rd, in Vector3 pa, in Vector3 pb, float ra, out Vector3 intersection, out Vector3 normal)
    {
        Vector3 ca = pb - pa;
        Vector3 oc = ro - pa;
        float caca = Vector3.Dot(ca, ca);
        float card = Vector3.Dot(ca, rd);
        float caoc = Vector3.Dot(ca, oc);
        float a = caca - card * card;
        float b = caca * Vector3.Dot(oc, rd) - caoc * card;
        float c = caca * Vector3.Dot(oc, oc) - caoc * caoc - ra * ra * caca;
        float h = b * b - a * c;
        if (h < 0.0)
        {
            intersection = Vector3.zero;
            normal = Vector3.zero;
            return false;
        }

        h = Mathf.Sqrt(h);
        float t = (-b - h) / a;

        // body
        float y = caoc + t * card;
        Vector3 v = (oc + t * rd - ca * y / caca) / ra;
        if (y > 0.0 && y < caca)
        {
            intersection = ro + rd * t;
            normal = new Vector3(v.x, v.y, v.z);
            return true;
        }

        // caps
        t = (((y < 0.0f) ? 0.0f : caca) - caoc) / card;
        v = ca * Mathf.Sign(y) / caca;
        if (Mathf.Abs(b + a * t) < h)
        {
            intersection = ro + rd * t;
            normal = new Vector3(v.x, v.y, v.z);
            return true;
        }

        intersection = Vector3.zero;
        normal = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Compute intersection of a finite ray with a capped cylinder
    /// adapted from http://iquilezles.org/www/articles/intersectors/intersectors.htm
    /// </summary>
    /// <param name="ro">Ray origin</param>
    /// <param name="re">Ray end</param>
    /// <param name="pa">one end of the cylinder</param>
    /// <param name="pb">other end of the cylinder</param>
    /// <param name="ra">radius of cylinder</param>
    /// <param name="intersection">If the ray intesects, location of intersection</param>
    /// <param name="normal">If the ray intesects, normal of cylinder at intersection</param>
    /// <returns>True if the ray intersects the cylinder</returns>
    public static bool FiniteRay(in Vector3 ro, in Vector3 re, in Vector3 pa, in Vector3 pb, float ra, out Vector3 intersection, out Vector3 normal)
    {
        float length = (re - ro).magnitude;
        Vector3 rd = (re - ro) / length;
        Vector3 ca = pb - pa;
        Vector3 oc = ro - pa;
        float caca = Vector3.Dot(ca, ca);
        float card = Vector3.Dot(ca, rd);
        float caoc = Vector3.Dot(ca, oc);
        float a = caca - card * card;
        float b = caca * Vector3.Dot(oc, rd) - caoc * card;
        float c = caca * Vector3.Dot(oc, oc) - caoc * caoc - ra * ra * caca;
        float h = b * b - a * c;

        if (h < 0.0)
        {
            intersection = Vector3.zero;
            normal = Vector3.zero;
            return false;
        }

        h = Mathf.Sqrt(h);
        float t = (-b - h) / a;

        // body
        float y = caoc + t * card;
        Vector3 n = (oc + t * rd - ca * y / caca) / ra;
        if ((y > 0.0) && (y < caca) && (t <= length))
        {
            intersection = ro + rd * t;
            normal = n;
            return true;
        }

        // caps
        t = (((y < 0.0f) ? 0.0f : caca) - caoc) / card;
        n = ca * Mathf.Sign(y) / caca;
        if ((Mathf.Abs(b + a * t) < h) && (t <= length))
        {
            intersection = ro + rd * t;
            normal = n;
            return true;
        }

        intersection = Vector3.zero;
        normal = Vector3.zero;
        return false;
    }

}