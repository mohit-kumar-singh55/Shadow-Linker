using UnityEngine;
using UnityEngine.AI;

// this script is just for traveling between predifined waypoints
public class EnemyPatrol : MonoBehaviour
{
    public Transform waypointGroup;
    public float waitTimeAtWaypoint = 2f;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private NavMeshAgent agent;
    private float waitTimer = 0f;
    private bool waiting = false;

    /// <summary>
    /// Initializes the patrol path by collecting waypoints and starting movement.
    /// </summary>
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // waypoints will not be assigned for a standing duty guard
        if (waypointGroup == null)
        {
            // Debug.LogError("Waypoint group not assigned!", this);
            enabled = false;
            return;
        }

        // Collect child transforms as waypoints
        waypoints = new Transform[waypointGroup.childCount];

        for (int i = 0; i < waypoints.Length; i++)
            waypoints[i] = waypointGroup.GetChild(i);

        // go to first waypoint
        GoToNextWaypoint();
    }

    /// <summary>
    /// Updates patrol path by moving to the next waypoint after waiting at each waypoint.
    /// </summary>
    void Update()
    {
        if (agent.pathPending || waypoints.Length == 0) return;

        if (!waiting && agent.remainingDistance <= agent.stoppingDistance)
        {
            waiting = true;
            waitTimer = waitTimeAtWaypoint;
        }

        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                GoToNextWaypoint();
            }
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    // for visual debugging purpose only
    void OnDrawGizmosSelected()
    {
        if (waypointGroup == null) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < waypointGroup.childCount; i++)
        {
            Vector3 wp = waypointGroup.GetChild(i).position;
            Gizmos.DrawSphere(wp, 0.2f);

            if (i < waypointGroup.childCount - 1)
                Gizmos.DrawLine(wp, waypointGroup.GetChild(i + 1).position);
        }
    }
}
