using UnityEngine;
using UnityEngine.AI;

// https://www.youtube.com/watch?v=xppompv1DBg&ab_channel=Brackeys
// https://www.youtube.com/watch?v=NK1TssMD5mE&t=1s&ab_channel=TableFlipGames
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private Transform target = null;
    [SerializeField] private float lookRadius = 8f;
    [SerializeField] private int chaseSpeed = 6;
    [SerializeField] private int patrolSpeed = 2;
    private NavMeshAgent navMeshAgent = null;
    private EnemyStats enemyStats = null;
    private Animator animator = null;
    private Vector3 originalPos = Vector3.zero;
    private Quaternion originalRot = Quaternion.identity;
    
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

    // Start is called before the first frame update
    void Start()
    {
        InitialiseComponents();
        SetWaypoints();
        SetDestination();
    }

    void InitialiseComponents()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyStats = GetComponent<EnemyStats>();
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
        if (distance <= lookRadius)
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
        if (chasing) { return; }

        if (travelling && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            HandleDestinationReached();
        }

        if (waiting)
        {
            HandleWaiting();
        }
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

    #region State Machine Logic

    private void SetDestination()
    {
        if (waypointsVisited > 0)
        {
            UpdateWaypoints();
        }

        Vector3 target = currWaypoint.transform.position;
        navMeshAgent.SetDestination(target);
        travelling = true;
        navMeshAgent.speed = patrolSpeed;
        animator.SetBool("isWalking", true);
        Debug.Log(currWaypoint.name);
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