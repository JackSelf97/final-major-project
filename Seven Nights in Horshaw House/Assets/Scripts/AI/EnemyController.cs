using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

// https://www.youtube.com/watch?v=xppompv1DBg&ab_channel=Brackeys
// https://www.youtube.com/watch?v=NK1TssMD5mE&t=1s&ab_channel=TableFlipGames
public class EnemyController : MonoBehaviour, IEntityController
{
    [Header("Stats")]
    [SerializeField] private Transform target = null;
    [SerializeField] private float lookRadius = 8f;
    [SerializeField] private int chaseSpeed = 7;
    [SerializeField] private int patrolSpeed = 2;
    private NavMeshAgent navMeshAgent = null;
    private EnemyStats enemyStats = null;
    private Animator animator = null;
    private Vector3 originalPos = Vector3.zero;
    private Quaternion originalRot = Quaternion.identity;
    private PlayerStats playerStats = null;
    
    [Header("Navigation")]
    [SerializeField] private float totalWaitTime = 3f;
    [SerializeField] private bool patrolWaiting = false;
    [SerializeField] private bool travelling = false;
    [SerializeField] private bool waiting = false;
    private ConnectedWaypoint currWaypoint = null, prevWaypoint = null;
    private float waitTimer = 0f;
    private int waypointsVisited = 0;
    public bool chasing = false;
    public Spawner parentSpawner = null;

    [Header("Character Sound")]
    [SerializeField] private AudioMixerGroup audioMixerGroup = null;
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private List<AudioClip> footstepSounds = new List<AudioClip>();
    [SerializeField] private AudioClip jumpSound = null;
    [SerializeField] private AudioClip landSound = null;
    private FootstepSwapper footstepSwapper = null;
    private float lastFootstepTime = 0f;
    private float footstepDelay = 0.6f;
    private Queue<int> lastSoundsQueue = new Queue<int>();

    // Start is called before the first frame update
    void Start()
    {
        InitialiseEnemy();
        SetWaypoints();
        SetDestination();
    }

    void InitialiseEnemy()
    {
        // Original position and rotation
        originalPos = transform.position;
        originalRot = transform.rotation;

        // Get components
        playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyStats = GetComponent<EnemyStats>();
        audioSource = GetComponent<AudioSource>();
        footstepSwapper = GetComponent<FootstepSwapper>();

        // Set the speed
        navMeshAgent.speed = 2;
    }

    void SetWaypoints()
    {
        if (currWaypoint == null)
        {
            if (parentSpawner == null)
            {
                GameObject[] allWaypointsInScene = GameObject.FindGameObjectsWithTag("Waypoint");
                GetWaypoints(allWaypointsInScene);
            }
            else
            {
                GetWaypoints(parentSpawner.allWaypoints);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (enemyStats.isDead) { return; }

        float distance = Vector3.Distance(target.position, transform.position);
        if (distance <= lookRadius && !playerStats.spiritRealm)
        {
            ChaseTarget();

            if (distance <= navMeshAgent.stoppingDistance)
            {
                // Attack and face the target
                animator.SetBool("isAttacking", true);
                FaceTarget();
            }
        }
        else
        {
            chasing = false;
        }
    }

    private void Update()
    {
        Footsteps();
        if (chasing) { return; }
        
        if (travelling && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            HandleDestinationReached();

        if (waiting)
            HandleWaiting();
    }

    private void HandleDestinationReached()
    {
        travelling = false;
        waypointsVisited++;

        if (patrolWaiting)
        {
            waiting = true;
            animator.SetBool("isWalking", false);
            waitTimer = 0f;
        }
        else
        {
            SetDestination();
        }
    }

    private void HandleWaiting()
    {
        waitTimer += Time.fixedDeltaTime;
        if (waitTimer >= totalWaitTime)
        {
            waiting = false;
            SetDestination();
        }
    }

    #region Navigation

    private void SetDestination()
    {
        if (waypointsVisited > 0)
            UpdateWaypoints();

        Vector3 target = currWaypoint.transform.position;
        navMeshAgent.SetDestination(target);
        travelling = true;
        navMeshAgent.speed = patrolSpeed;
        animator.SetBool("isWalking", true);
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

                // We found a waypoint
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

    void ChaseTarget()
    {
        // Set the bool
        chasing = true;

        // Set the animation
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isChasing", true);

        // Set the scripts
        navMeshAgent.speed = chaseSpeed;

        // Set the target
        navMeshAgent.SetDestination(target.position);
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 5f);
    }

    #endregion

    #region Audio

    private void Footsteps()
    {
        // Set footstep delay based on whether the enemy is chasing or not
        float currentFootstepDelay = chasing ? 0.35f : 0.6f;

        if (navMeshAgent.velocity.sqrMagnitude > 0 && travelling)
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

        int ranNo;
        do
        {
            ranNo = Random.Range(0, footstepSounds.Count);
        } while (lastSoundsQueue.Contains(ranNo));

        audioSource.clip = footstepSounds[ranNo];

        // Set a random pitch between 1 and 2
        float randomPitch = Random.Range(1f, 2f);
        audioSource.pitch = randomPitch;

        audioSource.PlayOneShot(audioSource.clip);
        Debug.Log(audioSource.clip);
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
        // Reset the position and rotation
        transform.position = originalPos;
        transform.rotation = originalRot;

        // Stop chasing
        if (chasing)
            chasing = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}