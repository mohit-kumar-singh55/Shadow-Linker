using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyGuard), typeof(EnemyPatrol))]
public class EnemyAIManager : MonoBehaviour
{
    private EnemyGuard guard;
    private EnemyPatrol patrol;
    private Animator animator;
    private NavMeshAgent agent;

    const string ANIM_WALKING_SPEED = "speed";

    private void Awake()
    {
        guard = GetComponent<EnemyGuard>();
        patrol = GetComponent<EnemyPatrol>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        animator.SetFloat(ANIM_WALKING_SPEED, agent.velocity.magnitude);

        switch (guard.currentState)
        {
            case EnemyState.Idle:
                patrol.enabled = false;
                break;
            case EnemyState.Patrol:
                patrol.enabled = true;
                break;
            case EnemyState.Chasing:
                patrol.enabled = false;
                break;
        }
    }
}
