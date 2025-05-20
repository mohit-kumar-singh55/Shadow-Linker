using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { StandingDuty, Patrollable };
public enum EnemyState { Idle, Patrol, Chasing };

/*
* 1. Standing Duty Enemy Guard State Machine
* Idle -> for a standing guard only -> Chasing -> Back to starting position -> Idle
* 2. Petrollable Enemy Guard State Machine
* Patrol -> Player Detected -> 
* Chasing -> Player sight lost (after a few seconds) -> 
* Suspicious (inspecting) for a few seconds -> Return to patrol
*/
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyGuard : MonoBehaviour
{
    [Header("General Settings")]
    public EnemyType enemyType = EnemyType.Patrollable;
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Vision Settings")]
    public float viewRadius = 10f;
    [Range(0f, 360f)] public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Detection Settings")]
    public float detectionTime = 2f;
    private float currentDetectTimer = 0f;

    [Header("References")]
    public Transform player;
    public Transform eyePosition; // place it on guard’s head
    public AudioSource walkAudioSource;
    public AudioSource runAudioSource;
    public AudioSource chasingAudioSource;
    public AudioSource attackAudioSource;

    private NavMeshAgent agent;
    private Animator animator;

    public float walkSpeed = 3f;       // enemy walk speed  -  this will override navmash agent's default speed
    public float chaseSpeed = 5f;       // speed to chase player
    public float killDistance = 6.5f;   // distance to kill player

    // time to lose player if player is not in sight
    public float losePlayerTime = 3f;
    private float losePlayerTimer = 0f;

    // suspicious timer -> timer to search for player
    public float inspectionTime = 3f;
    private float inspectionTimer = 0f;

    // for standing duty guard, to go back to its original position after chasing the player
    private Vector3 startingPosition;

    const string ANIM_RUNNING = "isRunning";
    const string ANIM_ATTACKING = "isAttacking";
    const string ANIM_INSPECTING = "isInspecting";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        startingPosition = transform.position;

        // apply difficulty settings
        // this will override some variables as per difficulty, irrespective of what is set in inspector
        ApplyDifficultySettings();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleBehaviour();
                break;
            case EnemyState.Patrol:
                PatrolBehaviour();
                break;
            case EnemyState.Chasing:
                ChasingBehaviour();
                break;
        }
    }

    void IdleBehaviour()
    {
        // idle -> stop all audio
        if (walkAudioSource != null && runAudioSource != null)
        {
            walkAudioSource.Stop();
            runAudioSource.Stop();
        }

        // go back to starting position
        if (transform.position != startingPosition) agent.SetDestination(startingPosition);
        PatrolBehaviour();
    }

    /// <summary>
    /// Patrol behaviour for the guard enemy.
    /// This function makes the guard patrol and set to chasing state if it sees the player.
    /// </summary>
    void PatrolBehaviour()
    {
        if (player == null || PlayerShadowState.Instance == null) return;

        agent.speed = walkSpeed;

        // walking audio
        if (walkAudioSource != null && runAudioSource != null && !walkAudioSource.isPlaying)
        {
            runAudioSource.Stop();
            walkAudioSource.Play();
        }

        if (IsPlayerInSight())
        {
            currentDetectTimer += Time.deltaTime;

            if (currentDetectTimer >= detectionTime)
            {
                currentState = EnemyState.Chasing;
                agent.SetDestination(player.position);

                Debug.Log("❗ PLAYER DETECTED! CHASING...");
            }
        }
        else
        {
            currentDetectTimer -= Time.deltaTime;
            currentDetectTimer = Mathf.Clamp(currentDetectTimer, 0f, detectionTime);
        }
    }

    /// <summary>
    /// Chasing behaviour for the guard enemy.
    /// This function makes the guard chase the player and play the running animation.
    /// If the player is in sight, the guard will slash the player if it is close enough.
    /// If the player is not in sight, the guard will enter a suspicious (inspecting) state after a cooldown timer.
    /// </summary>
    void ChasingBehaviour()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);      // follow player
        animator.SetBool(ANIM_RUNNING, true);        // set running animation

        // stopping bgm audios
        AudioManager.Instance.StopBGM();

        // chasing audio
        if (chasingAudioSource != null && !chasingAudioSource.isPlaying) chasingAudioSource.Play();

        // running audio
        if (walkAudioSource != null && runAudioSource != null && !runAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
            runAudioSource.Play();
        }

        if (IsPlayerInSight())
        {
            // slash the player if close enough
            float distToPlayer = Vector3.Distance(transform.position, player.position);

            if (distToPlayer <= killDistance)
            {
                // stop audio
                if (runAudioSource != null) runAudioSource.Stop();
                if (chasingAudioSource != null) chasingAudioSource.Stop();

                // attack audio
                if (attackAudioSource != null) attackAudioSource.Play();

                // stopping agent and playing animation
                agent.isStopped = true;
                animator.SetTrigger(ANIM_ATTACKING);

                // trigger lose - game over
                Invoke("TriggerLose", .8f);
                Debug.Log("🗡️ Attacking player");
            }
            else agent.isStopped = false;

            // reset timer
            losePlayerTimer = losePlayerTime;
            inspectionTimer = inspectionTime;
        }
        else
        {
            // chasing cooldown timer
            losePlayerTimer -= Time.deltaTime;

            if (losePlayerTimer < 0)
            {
                // stop audio
                if (runAudioSource != null) runAudioSource.Stop();

                // stopping and playing suspicious (inspecting) animation
                agent.isStopped = true;
                animator.SetBool(ANIM_RUNNING, false);        // set running animation
                animator.SetBool(ANIM_INSPECTING, true);        // set inspecting animation
                Debug.Log("🔍 Inspecting the place");

                // suspicious (inspecting) cooldown timer
                inspectionTimer -= Time.deltaTime;

                // inspection finished and player lost, return to patrol...
                if (inspectionTimer <= 0)
                {
                    // playing bgm audios
                    AudioManager.Instance.PlayBGM();

                    agent.isStopped = false;
                    currentState = enemyType == EnemyType.StandingDuty ? EnemyState.Idle : EnemyState.Patrol;
                    animator.SetBool(ANIM_INSPECTING, false);        // set inspecting animation

                    Debug.Log("👁️ Lost player. Returning to patrol.");
                }
            }
        }
    }

    /// <summary>
    /// Checks whether the player is in the guard's sight.
    /// This does the following checks:
    /// 1. Is the player in the view radius?
    /// 2. Is the player in the view angle?
    /// 3. Is there an obstacle in the way (raycast check)?
    /// 4. Is the player in a shadow zone?
    /// 5. Is the player crouching?
    /// If any of these conditions are false, the player is not in sight.
    /// </summary>
    bool IsPlayerInSight()
    {
        Vector3 guardPosition = eyePosition ? eyePosition.position : transform.position + Vector3.up * 1.5f;
        Vector3 dirToPlayer = (player.position - guardPosition).normalized;
        float distToPlayer = Vector3.Distance(guardPosition, player.position);

        if (distToPlayer > viewRadius) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        // this obstacle mask is so that if player is hiding behind any obstacle this raycast should be blocked by the obstacle
        if (Physics.Raycast(guardPosition, dirToPlayer, distToPlayer, obstacleMask)) return false;

        // Shadow + crouch check
        bool isInShadow = PlayerShadowState.Instance.isInShadow;
        bool isCrouching = PlayerController.Instance.IsCrouching;

        return !isInShadow || !isCrouching;
    }

    /// <summary>
    /// Stops all audio sources and triggers the lose condition in the game.
    /// Disables the guard component after triggering the lose condition.
    /// </summary>
    void TriggerLose()
    {
        // stop all audios
        if (walkAudioSource != null && runAudioSource != null && chasingAudioSource != null)
        {
            walkAudioSource.Stop();
            runAudioSource.Stop();
            chasingAudioSource.Stop();
            // AudioManager.Instance.StopBGM();
        }

        GameManager.Instance.TriggerLose(); // Trigger lose

        enabled = false;
    }

    /// <summary>
    /// Applies difficulty settings from the current <see cref="DifficultySettings"/>,
    /// overriding the fields in this class with the values from the difficulty settings.
    /// </summary>
    private void ApplyDifficultySettings()
    {
        var settings = DifficultyManager.Instance.CurrentSettings;
        viewRadius = settings.viewRadius;
        detectionTime = settings.detectionTime;
        losePlayerTime = settings.losePlayerTime;
    }

    // for visual debugging purpose only
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
    }

    // for visual debugging purpose only
    public Vector3 DirFromAngle(float angle, bool global)
    {
        if (!global) angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
