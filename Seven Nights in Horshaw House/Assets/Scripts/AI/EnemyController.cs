using UnityEngine;
using UnityEngine.AI;

// https://www.youtube.com/watch?v=xppompv1DBg&ab_channel=Brackeys
// https://www.youtube.com/watch?v=NK1TssMD5mE&t=1s&ab_channel=TableFlipGames
public class EnemyController : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Transform target = null;
    [SerializeField] private float lookRadius = 8f;
    [SerializeField] private int chaseSpeed = 7;
    [SerializeField] private int patrolSpeed = 4;
    private NavMeshAgent navMeshAgent = null;
    private EnemyStats enemyStats = null;
    private Vector3 originalPos = Vector3.zero;
    private Quaternion originalRot = Quaternion.identity;

    [Header("States")]
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
        originalPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        originalRot = transform.rotation;
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<EnemyStats>();
        navMeshAgent.speed = 2;
        if (currWaypoint == null)
        {
            // Set it at random
            // Grab all the waypoint objects in the scene
            if (parentSpawner == null) // meaning if the enemy was NOT instantiated
            {
                GameObject[] allWaypointsInScene = GameObject.FindGameObjectsWithTag("Waypoint"); // then have the freedom to move with every waypoint
                GetWaypoints(allWaypointsInScene);
            }
            else
            {
                GetWaypoints(parentSpawner.allWaypoints);
            }
        }

        SetDestination();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!enemyStats.isDead)
        {
            float distance = Vector3.Distance(target.position, transform.position);
            if (distance <= lookRadius)
            {
                ChaseTarget();
                if (distance <= navMeshAgent.stoppingDistance)
                {
                    // attack and face the target
                    FaceTarget();
                }
            }
            else
            {
                chasing = false;
            }
        }
    }

    private void Update()
    {
        if (chasing) { return; }
        // Check if we're close to the destination
        if (travelling && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) // has to match the stopping distance
        {
            travelling = false;
            waypointsVisited++;

            // If we're going to wait, then wait
            if (patrolWaiting)
            {
                waiting = true;
                waitTimer = 0f;
            }
            else
            {
                SetDestination();
            }
        }

        // Instead if we're waiting
        if (waiting)
        {
            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= totalWaitTime)
            {
                waiting = false;
                SetDestination();
            }
        }
    }

    #region State Machine Logic

    void ChaseTarget()
    {
        // Set the bool
        chasing = true;

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

    private void SetDestination()
    {
        if (waypointsVisited > 0)
        {
            ConnectedWaypoint nextWaypoint = currWaypoint.NextWaypoint(prevWaypoint);
            prevWaypoint = currWaypoint;
            currWaypoint = nextWaypoint;
        }

        Vector3 target = currWaypoint.transform.position;
        navMeshAgent.SetDestination(target);
        travelling = true;
        navMeshAgent.speed = patrolSpeed;
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

    public void EnemyReset()
    {
        // Reset the enemy's position and rotation
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