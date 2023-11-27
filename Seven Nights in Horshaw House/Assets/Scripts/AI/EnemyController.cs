using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

// https://www.youtube.com/watch?v=xppompv1DBg&ab_channel=Brackeys
// https://www.youtube.com/watch?v=NK1TssMD5mE&t=1s&ab_channel=TableFlipGames
public class EnemyController : MonoBehaviour, IEntityController
{
    [Header("Stats")]
    private PlayerStats playerStats = null;
    [SerializeField] private Transform target = null;
    [SerializeField] private Collider meleeCollider = null;
    [SerializeField] private float lookRadius = 8f;
    [SerializeField] private int chaseSpeed = 4;
    [SerializeField] private int patrolSpeed = 2;
    private NavMeshAgent navMeshAgent = null;
    private Animator animator = null;

    [Header("Navigation")]
    [SerializeField] private float totalWaitTime = 3f;
    [SerializeField] private bool patrolWaiting = false;
    private ConnectedWaypoint currWaypoint = null, prevWaypoint = null;
    public float waitTimer = 0f;
    public int waypointsVisited = 0;
    public bool searching = false;
    public bool foundTarget = false;
    public float distance = 0f;

    public enum EnemyState
    {
        Idle,
        Walking,
        Chasing,
        Attacking
    }

    public EnemyState currentState = EnemyState.Idle;

    [Header("Character Sound")]
    [SerializeField] private AudioMixerGroup audioMixerGroup = null;
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private List<AudioClip> footstepSounds = new List<AudioClip>();
    [SerializeField] private AudioClip jumpSound = null;
    [SerializeField] private AudioClip landSound = null;
    private FootstepSwapper footstepSwapper = null;
    private float lastFootstepTime = 0f;
    private Queue<int> lastSoundsQueue = new Queue<int>();

    [Header("VFX")]
    [SerializeField] private ParticleSystem smokeParticle = null;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseEnemy();
        GetWaypoints();
        EnemyReset();
        //StartCoroutine(StartMovingAfterDelay());
    }

    private void InitialiseEnemy()
    {
        // Get components
        playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        footstepSwapper = GetComponent<FootstepSwapper>();

        // Set the speed
        navMeshAgent.speed = 2;
    }

    private void GetWaypoints()
    {
        if (currWaypoint == null)
        {
            GameObject[] allWaypointsInScene = GameObject.FindGameObjectsWithTag("Waypoint");
            GetWaypoints(allWaypointsInScene);
        }
    }

    public void SetRandomPositionAndRotation()
    {
        Vector3 enemySpawnPosition;
        Vector3 enemySpawnRotation;
        GameManager.gMan.GetSpawnPoint(GameManager.gMan.enemySpawnPointSO, out enemySpawnPosition, out enemySpawnRotation);
        navMeshAgent.Warp(enemySpawnPosition);
        transform.rotation = Quaternion.Euler(enemySpawnRotation.x, enemySpawnRotation.y, enemySpawnRotation.z);
    }

    public IEnumerator StartMovingAfterDelay()
    {
        // Play the smoke particle effect
        smokeParticle.Play();

        // Wait for the specified delay
        yield return new WaitForSeconds(totalWaitTime);
        searching = true;
    }

    void Update()
    {
        distance = Vector3.Distance(target.position, transform.position);

        HandleLookRadius();
        Footsteps();
        if (!playerStats.spiritRealm && searching)
            Searching();

        meleeCollider.enabled = currentState == EnemyState.Attacking;

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                animator.SetBool("isWalking", false);
                animator.SetBool("isChasing", false);
                animator.SetBool("isAttacking", false);
                break;
            case EnemyState.Walking:
                HandleWalkingState();
                animator.SetBool("isWalking", true);
                animator.SetBool("isChasing", false);
                animator.SetBool("isAttacking", false);
                break;
            case EnemyState.Chasing:
                HandleChasingState();
                FaceTarget();
                animator.SetBool("isChasing", true);
                animator.SetBool("isAttacking", false);
                animator.SetBool("isWalking", false);
                break;
            case EnemyState.Attacking:
                HandleAttackingState();
                FaceTarget();
                animator.SetBool("isAttacking", true);
                animator.SetBool("isChasing", false);
                animator.SetBool("isWalking", false);
                break;
        }
    }

    void HandleIdleState()
    {
        if (patrolWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= totalWaitTime)
            {
                SetDestination();
                waitTimer = 0f;
            }
        }
        else
        {
            SetDestination();
        }
    }

    void HandleWalkingState()
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
            navMeshAgent.velocity.sqrMagnitude == 0f)
        {
            waypointsVisited++;
            Debug.Log(gameObject.name + " has reached: " + currWaypoint.name);

            if (patrolWaiting)
            {
                waitTimer = 0f;
            }
            currentState = EnemyState.Idle;
        }
    }

    private void Searching()
    {
        if (distance <= lookRadius && !foundTarget)
        {
            Debug.Log("I've found the player!");
            foundTarget = true;
            currentState = EnemyState.Chasing;
        }
        else if (distance >= lookRadius && foundTarget)
        {
            Debug.Log("I've lost the player!");
            foundTarget = false;
            navMeshAgent.ResetPath();
            currentState = EnemyState.Idle;
        }
    }

    private void HandleChasingState()
    {
        if (patrolWaiting)
        {
            waitTimer = 0f;
        }

        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.SetDestination(target.position);

        // Attack the target if it is close
        if (distance <= navMeshAgent.stoppingDistance)
        {
            currentState = EnemyState.Attacking;
        }
    }

    private void HandleAttackingState()
    {
        // Confirmed kill
        if (playerStats.spiritRealm)
        {
            foundTarget = false;
            currentState = EnemyState.Idle;
            return;
        }

        // Go back to chasing if the target has evaded
        if (distance >= navMeshAgent.stoppingDistance)
        {
            currentState = EnemyState.Chasing;
        }
    }

    #region Navigation

    private void SetDestination()
    {
        if (waypointsVisited > 0)
            UpdateWaypoints();

        Vector3 target = currWaypoint.transform.position;
        navMeshAgent.SetDestination(target);
        navMeshAgent.speed = patrolSpeed;

        Debug.Log(gameObject.name + " is going to " + currWaypoint.name);

        currentState = EnemyState.Walking;
    }

    private void UpdateWaypoints()
    {
        ConnectedWaypoint nextWaypoint = currWaypoint.NextWaypoint(prevWaypoint);
        prevWaypoint = currWaypoint;
        currWaypoint = nextWaypoint;
    }

    private void GetWaypoints(GameObject[] allWaypoints)
    {
        if (allWaypoints.Length > 0)
        {
            while (currWaypoint == null)
            {
                int random = Random.Range(0, allWaypoints.Length);
                ConnectedWaypoint startingWaypoint = allWaypoints[random].GetComponent<ConnectedWaypoint>();

                if (startingWaypoint != null)
                {
                    currWaypoint = startingWaypoint;
                }
            }
        }
        else
        {
            Debug.LogError("Failed to find any waypoints for use in the scene!");
        }
    }

    #endregion

    #region Actions

    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 5f);
    }

    private void HandleLookRadius()
    {
        lookRadius = GameManager.gMan.kingOfTheHill.enemyInside ? 5 : 8;
    }

    #endregion

    #region Audio

    private void Footsteps()
    {
        // Set footstep delay based on whether the enemy is chasing or not
        float currentFootstepDelay = currentState == EnemyState.Chasing ? 0.4f : 0.6f;

        if (navMeshAgent.velocity.sqrMagnitude > 0 && currentState == EnemyState.Walking || navMeshAgent.velocity.sqrMagnitude > 0 && currentState == EnemyState.Chasing)
        {
            // Check if enough time has passed since the last footstep
            if (Time.time - lastFootstepTime > currentFootstepDelay)
            {
                PlayFootstepAudio();
                lastFootstepTime = Time.time;
            }
        }
    }

    private void PlayFootstepAudio()
    {
        footstepSwapper.CheckLayers();

        // Check if there are any footstep sounds available
        if (footstepSounds.Count == 0)
        {
            Debug.LogWarning("No footstep sounds available.");
            return;
        }

        int ranNo;
        do
        {
            ranNo = Random.Range(0, footstepSounds.Count);
        } while (lastSoundsQueue.Contains(ranNo));

        audioSource.clip = footstepSounds[ranNo];

        // Set a random pitch between 1 and 2 with increments of 0.1
        float randomPitch = Mathf.Round(Random.Range(10f, 20f)) * 0.1f;
        audioSource.pitch = randomPitch;

        audioSource.PlayOneShot(audioSource.clip);
        audioSource.outputAudioMixerGroup = audioMixerGroup;

        // Enqueue the recently played sound
        lastSoundsQueue.Enqueue(ranNo);

        // Keep the queue size at 2, removing the oldest sound
        if (lastSoundsQueue.Count > 2)
        {
            lastSoundsQueue.Dequeue();
        }
    }

    public void SwapFootsteps(FootstepCollection collection)
    {
        footstepSounds.Clear();
        for (int i = 0; i < collection.footstepSpunds.Count; i++)
        {
            footstepSounds.Add(collection.footstepSpunds[i]);
        }
        jumpSound = collection.jumpSound;
        landSound = collection.landSound;
    }

    #endregion

    public void EnemyReset()
    {
        navMeshAgent.ResetPath();
        SetRandomPositionAndRotation();
        searching = false;
        GameManager.gMan.kingOfTheHill.enemyInside = false;
        waypointsVisited = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}