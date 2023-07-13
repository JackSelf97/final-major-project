using UnityEngine;
using UnityEngine.AI;

// https://www.youtube.com/watch?v=xppompv1DBg&ab_channel=Brackeys
public class EnemyController : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Transform target = null;
    [SerializeField] private float lookRadius = 8f;
    [SerializeField] private int chaseSpeed = 4;
    private NavMeshAgent navMeshAgent = null;
    private EnemyStats enemyStats = null;

    [Header("States")]
    [SerializeField] private bool patrolWaiting = false; 
    [SerializeField] private float totalWaitTime = 3f; 
    [SerializeField] private float switchProbability = 0.2f;
    public bool chasing = false;
    [SerializeField] private bool travelling = false;
    [SerializeField] private bool waiting = false;
    public Spawner parentSpawner = null;
    private ConnectedWaypoint currWaypoint = null, prevWaypoint = null;
    private float waitTimer = 0f;
    private int waypointsVisited = 0;

    //[Header("Ragdoll Physics")]
    //private Animator animator = null;
    //private Collider[] colliders = null;
    //private Rigidbody[] rigidbodies = null;
    //public BoxCollider mainCollider = null;
    //public GameObject hips = null;
    //public GameObject hitBox = null;

    //[Header("Projectile Data")]
    //public int impactCount = 0;
    //public int maxWeightOfImpact = 5;

    // Start is called before the first frame update
    void Start()
    {
        //animator = GetComponent<Animator>();
        //SetRagdollParts();
        //TurnOffRagdoll();

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
                    //animator.SetBool("IsAttacking", true);
                    FaceTarget();
                }
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
                //animator.SetBool("IsWalking", false);
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

    void ChaseTarget()
    {
        // Set the bool
        chasing = true;

        // Set the animation
        //animator.SetBool("IsWalking", false);
        //animator.SetBool("IsAttacking", false);
        //animator.SetBool("IsChasing", true);

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

    #region Ragdoll Physics

    //private void SetRagdollParts()
    //{
    //    colliders = hips.GetComponentsInChildren<Collider>(); // get all the colliders
    //    rigidbodies = hips.GetComponentsInChildren<Rigidbody>();
    //}

    //void TurnOnRagdoll()
    //{
    //    float suckCannonForce = PlayerManager.pMan.player.GetComponent<SuckCannon>().force;
    //    Transform playerCam = Camera.main.transform;

    //    animator.enabled = false;
    //    foreach (Collider col in colliders)
    //    {
    //        col.enabled = true;
    //    }

    //    foreach (Rigidbody rb in rigidbodies)
    //    {
    //        rb.isKinematic = false;
    //        rb.AddForce(playerCam.transform.forward * suckCannonForce, ForceMode.Impulse);
    //    }

    //    mainCollider.enabled = false;
    //    GetComponent<Rigidbody>().isKinematic = false;
    //}

    //void TurnOffRagdoll()
    //{
    //    foreach (Collider col in colliders)
    //    {
    //        col.enabled = false;
    //    }

    //    foreach (Rigidbody rb in rigidbodies)
    //    {
    //        rb.isKinematic = true;
    //    }

    //    animator.enabled = true;
    //    mainCollider.enabled = true;
    //    GetComponent<Rigidbody>().isKinematic = true;
    //    hitBox.GetComponent<Collider>().enabled = true; // turn 'hitBox' collider back on
    //}

    #endregion

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
        //animator.SetBool("IsWalking", true);
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

    private void OnCollisionEnter(Collision collision) 
    {
        //if (collision.gameObject.layer == 6)
        //{
        //    if (collision.gameObject.GetComponent<Junk>().shot)
        //    {
        //        int junkProjectileWeight = collision.gameObject.GetComponent<Junk>().junkItemSO.weight;
        //        impactCount += junkProjectileWeight;

        //        if (!isChasing && enemyStats.isAlive)
        //            ChaseTarget();

        //        if (impactCount >= maxWeightOfImpact) // different junk items will hold different weight values
        //        {
        //            isChasing = false;
        //            navMeshAgent.enabled = false; // makes them grounded and also solves the issue of them 'zapping'
        //            enemyStats.isAlive = false;

        //            TurnOnRagdoll();
        //            Destroy(gameObject, 5); // could make into shrink death
        //        }
        //    }
        //}
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}