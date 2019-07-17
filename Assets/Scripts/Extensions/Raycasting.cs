using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raycasting
{
    /// <summary>
    /// Helpers for Unity's builtin raycasting
    /// </summary>
    namespace Helpers {

    }

    /// <summary>
    /// Custom raycasting implementations - these bypass Unity's builtins.
    /// </summary>
    namespace Custom
    {
        public static class CappedCylinder
        {
            /// <summary>
            /// Compute intersection of an infinite ray with a capped cylinder.
            /// NOTE/WARNING/TODO, this implementation doesn't generate cap intersections! Only intersections with the finite cylinderical shell.
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
            /// <returns>True if the ray intersects the cylinder. NOTE: If </returns>
            public static bool Ray(Vector3 start, Vector3 dir, Vector3 A, Vector3 B, float r, out Vector3 intersection, out Vector3 normal, out float distance)
            {
                const float Epsilon = 1e-5f;
                distance = 0;
                normal = Vector3.zero;
                intersection = Vector3.zero;

                Vector3 AB = B - A;
                Vector3 AO = start - A;
                Vector3 AOxAB = Vector3.Cross(AO, AB);
                Vector3 VxAB = Vector3.Cross(dir, AB);
                float ab2 = Vector3.Dot(AB, AB);
                float a = Vector3.Dot(VxAB, VxAB);
                float b = 2 * Vector3.Dot(VxAB, AOxAB);
                float c = Vector3.Dot(AOxAB, AOxAB) - (r * r * ab2);
                float d = b * b - 4 * a * c;
                if (d < -Epsilon)
                {
                    return false;
                }

                distance = (-b - Mathf.Sqrt(d)) / (2 * a);
                if (distance < -Epsilon)
                {
                    return false;
                }

                intersection = start + dir * distance; // intersection point
                Vector3 projection = A + (Vector3.Dot(AB, (intersection - A)) / ab2) * AB; // intersection projected onto cylinder axis
                if ((projection - A).magnitude + (B - projection).magnitude > AB.magnitude + Epsilon)
                {
                    Debug.LogFormat("hi");
                    return false;
                }

                normal = (intersection - projection).normalized;
                return true;
            }

            /// <summary>
            /// Compute intersection of an infinite ray with a capped cylinder - handling scenarios where the ray origin is inside the cylinder
            /// by writing a negative distance into distance.
            /// NOTE/WARNING/TODO, this implementation doesn't generate cap intersections! Only intersections with the finite cylinderical shell.
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
            /// <returns>True if the ray intersects the cylinder. NOTE: If </returns>
            public static bool Ray_Backtracking(Vector3 start, Vector3 dir, Vector3 A, Vector3 B, float r, out Vector3 intersection, out Vector3 normal, out float distance)
            {
                const float Epsilon = 1e-5f;
                distance = 0;
                normal = Vector3.zero;
                intersection = Vector3.zero;

                Vector3 AB = B - A;
                Vector3 AO = start - A;
                Vector3 AOxAB = Vector3.Cross(AO, AB);
                Vector3 VxAB = Vector3.Cross(dir, AB);
                float ab2 = Vector3.Dot(AB, AB);
                float a = Vector3.Dot(VxAB, VxAB);
                float b = 2 * Vector3.Dot(VxAB, AOxAB);
                float c = Vector3.Dot(AOxAB, AOxAB) - (r * r * ab2);
                float d = b * b - 4 * a * c;
                if (d < -Epsilon)
                {
                    return false;
                }

                distance = (-b - Mathf.Sqrt(d)) / (2 * a);

                // only reject if the point is actually outside the cylinder
                if (distance < -Epsilon && !Inside(start, A, B, r))
                {
                    return false;
                }

                intersection = start + dir * distance; // intersection point
                Vector3 projection = A + (Vector3.Dot(AB, (intersection - A)) / ab2) * AB; // intersection projected onto cylinder axis
                if ((projection - A).magnitude + (B - projection).magnitude > AB.magnitude + Epsilon)
                {
                    return false;
                }

                normal = (intersection - projection).normalized;
                return true;
            }


            public static bool Line(Vector3 start, Vector3 end, Vector3 A, Vector3 B, float r, out Vector3 intersection, out Vector3 normal, out float distance)
            {
                Vector3 dir = end - start;
                float length = dir.magnitude + 1e-5f;
                dir /= length;

                return Ray(start, dir, A, B, r, out intersection, out normal, out distance) && distance <= length;
            }

            /// <summary>
            /// Test if a point is inside a capped cylinder.
            /// Adapted from https://www.flipcode.com/archives/Fast_Point-In-Cylinder_Test.shtml
            /// </summary>
            /// <param name="p">The point to test</param>
            /// <param name="A">One end of cylinder</param>
            /// <param name="B">Other end of cylinder</param>
            /// <param name="radius">Radius of cylinder</param>
            /// <param name="distanceFromCyclinderAxis">If the point is inside, the distance of the point from the cylinderical axis</param>
            /// <returns>true iff the point is inside the cylinder</returns>
            public static bool Inside(Vector3 p, Vector3 A, Vector3 B, float radius, out float distanceFromCyclinderAxis)
            {
                float length2 = (A - B).sqrMagnitude;
                float radius2 = radius * radius;

                // translate so pt1 is origin.  Make vector from pt1 to pt2.  
                float dx = B.x - A.x;
                float dy = B.y - A.y;
                float dz = B.z - A.z;

                // vector from pt1 to test point.
                float pdx = p.x - A.x;
                float pdy = p.y - A.y;
                float pdz = p.z - A.z;

                // Dot the d and pd vectors to see if point lies behind the 
                // cylinder cap at pt1.x, pt1.y, pt1.z

                float dot = pdx * dx + pdy * dy + pdz * dz;

                // If dot is less than zero the point is behind the pt1 cap.
                // If greater than the cylinder axis line segment length squared
                // then the point is outside the other end cap at pt2.

                if (dot < 0.0f || dot > length2)
                {
                    distanceFromCyclinderAxis = 0;
                    return false;
                }

                // Point lies within the parallel caps, so find
                // distance squared from point to line, using the fact that sin^2 + cos^2 = 1
                // the dot = cos() * |d||pd|, and cross*cross = sin^2 * |d|^2 * |pd|^2
                // Carefull: '*' means mult for scalars and dotproduct for vectors
                // In short, where dist is pt distance to cyl axis: 
                // dist = sin( pd to d ) * |pd|
                // distsq = dsq = (1 - cos^2( pd to d)) * |pd|^2
                // dsq = ( 1 - (pd * d)^2 / (|pd|^2 * |d|^2) ) * |pd|^2
                // dsq = pd * pd - dot * dot / lengthsq
                //  where lengthsq is d*d or |d|^2 that is passed into this function 

                // distance squared to the cylinder axis:
                float dsq = (pdx * pdx + pdy * pdy + pdz * pdz) - dot * dot / length2;
                distanceFromCyclinderAxis = Mathf.Sqrt(dsq);
                return dsq < radius2;
            }

            public static bool Inside(Vector3 p, Vector3 A, Vector3 B, float radius)
            {
                return Inside(p, A, B, radius, out float _);
            }

        }
    }

}
