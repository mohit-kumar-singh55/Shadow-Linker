using System.Collections.Generic;
using UnityEngine;

public class ShadowZoneManager : MonoBehaviour
{
    private static List<ShadowZone> zones = new();

    public static void Register(ShadowZone zone)
    {
        if (!zones.Contains(zone)) zones.Add(zone);
    }

    public static void Unregister(ShadowZone zone)
    {
        if (zones.Contains(zone)) zones.Remove(zone);
    }

    /// <summary>
    /// Finds the closest ShadowZone that is visible from the given position and direction.
    /// A zone is considered visible if it is within the given max distance and there is no obstacle in the way.
    /// The direction is used to filter out zones that are outside of a 45° cone in front.
    /// </summary>
    /// <returns>The closest visible zone, or null if none are found</returns>
    public static ShadowZone GetClosestVisibleShadow(Vector3 from, Vector3 direction, float maxDistance, LayerMask obstacleMask, ShadowZone excludeZone = null)
    {
        ShadowZone best = null;
        float closest = maxDistance;
        float maxAngle = 45f; // Only consider zones within 45° cone in front

        foreach (var zone in zones)
        {
            if (zone == null || zone == excludeZone) continue;

            Vector3 to = zone.GetCenter();
            float dist = Vector3.Distance(from, to);
            if (dist > maxDistance) continue;

            Vector3 dirToZone = (to - from).normalized;
            float angle = Vector3.Angle(direction, dirToZone);
            if (angle > maxAngle) continue; // ignore zones outside forward cone

            if (dist < closest)
            {
                closest = dist;
                best = zone;
            }
        }

        return best;
    }
}