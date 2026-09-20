using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehavior : MonoBehaviour
{
    public float detectionRange = 10f;
    public float attackRange = 1.8f;
    public float patrolRadius = 8f;
    public float patrolPauseTime = 2f;
    public float maxHealth = 100f;
    public float fleeThreshold = 35f;
    public float fleeChance = 0.5f;
    public float fleeDistance = 8f;
    public float touchDamage = 10f;
    public float damageCooldown = 1f;
    public float memoryDuration = 4f;
    public float memorySearchRange = 12f;
    public float ragdollForce = 4f;
    public float ragdollTorque = 2f;
    public float awarenessCooldown = 1.5f;
    public float aggressionGain = 0.35f;
    public float fearGain = 0.4f;
    public float calmDownRate = 0.15f;
    public float suspiciousLookDuration = 1.2f;
    public float angerThreshold = 0.6f;
    public float angerGainFromHit = 0.4f;
    public float angerGainFromDialogue = 0.5f;
    public bool canAttackOnlyWhenAngered = true;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform playerTarget;
    private Vector3 homePosition;
    private Vector3 patrolDestination;
    private float patrolTimer;
    private float currentHealth;
    private bool hasFled;
    private bool isDead;
    private float damageTimer;
    private bool remembersPlayer;
    private float memoryTimer;
    private Vector3 lastKnownPlayerPosition;
    private DialogueSystem dialogueSystem;
    private float awarenessTimer;
    private float aggression;
    private float fear;
    private float suspiciousTimer;
    private bool isSuspicious;
    private enum AIState { Patrol, Investigate, Chase, Attack, Flee }
    private AIState state = AIState.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        homePosition = transform.position;
        patrolDestination = homePosition;
        FindPlayer();
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        aggression = 0f;
        fear = 0f;
    }

    void Update()
    {
        if (isDead || agent == null)
            return;

        if (playerTarget == null)
        {
            FindPlayer();
            if (playerTarget == null)
                return;
        }

        if (awarenessTimer > 0f)
        {
            awarenessTimer -= Time.deltaTime;
        }

        if (suspiciousTimer > 0f)
        {
            suspiciousTimer -= Time.deltaTime;
            isSuspicious = true;
        }
        else
        {
            isSuspicious = false;
        }

        if (aggression > 0f)
        {
            aggression = Mathf.Max(0f, aggression - calmDownRate * Time.deltaTime);
        }

        if (fear > 0f)
        {
            fear = Mathf.Max(0f, fear - calmDownRate * Time.deltaTime);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        bool seesPlayer = distanceToPlayer <= detectionRange;

        if (seesPlayer)
        {
            remembersPlayer = true;
            memoryTimer = memoryDuration;
            lastKnownPlayerPosition = playerTarget.position;
            aggression = Mathf.Min(1f, aggression + aggressionGain);
            fear = Mathf.Max(0f, fear - 0.05f);
            awarenessTimer = awarenessCooldown;
        }
        else if (remembersPlayer)
        {
            memoryTimer -= Time.deltaTime;
            if (memoryTimer <= 0f)
            {
                remembersPlayer = false;
            }
        }

        if (currentHealth <= fleeThreshold && !hasFled && Random.value <= fleeChance)
        {
            EnterFleeState();
            return;
        }

        if (state == AIState.Flee)
        {
            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                state = AIState.Patrol;
                hasFled = true;
                agent.isStopped = false;
            }
            return;
        }

        bool isAngeredEnough = !canAttackOnlyWhenAngered || aggression >= angerThreshold;

        if (distanceToPlayer <= attackRange && isAngeredEnough)
        {
            state = AIState.Attack;
            agent.isStopped = true;
            transform.LookAt(playerTarget);

            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                damageTimer = damageCooldown;
                DealTouchDamage();
            }

            return;
        }

        if (seesPlayer)
        {
            if (aggression >= 0.7f || (fear <= 0.2f && aggression >= 0.4f))
            {
                state = AIState.Chase;
                agent.isStopped = false;
                agent.SetDestination(playerTarget.position);
            }
            else
            {
                state = AIState.Investigate;
                agent.isStopped = false;
                agent.SetDestination(playerTarget.position);
                suspiciousTimer = suspiciousLookDuration;
                fear = Mathf.Min(1f, fear + fearGain);
            }
            return;
        }

        if (remembersPlayer && Vector3.Distance(transform.position, lastKnownPlayerPosition) <= memorySearchRange)
        {
            state = AIState.Investigate;
            agent.isStopped = false;
            agent.SetDestination(lastKnownPlayerPosition);
            return;
        }

        state = AIState.Patrol;
        agent.isStopped = false;

        if (!agent.hasPath || agent.remainingDistance <= 1f)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0f)
            {
                SetNextPatrolPoint();
                patrolTimer = patrolPauseTime;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        fear = Mathf.Min(1f, fear + 0.25f);
        aggression = Mathf.Min(1f, aggression + 0.1f);

        aggression = Mathf.Min(1f, aggression + angerGainFromHit);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void ReactToHit(Vector3 hitPoint)
    {
        if (isDead || playerTarget == null)
            return;

        transform.LookAt(playerTarget);
        Vector3 knockback = (transform.position - hitPoint).normalized * 0.3f;
        transform.position += knockback;
        suspiciousTimer = suspiciousLookDuration;
        aggression = Mathf.Min(1f, aggression + angerGainFromHit);

        if (animator != null)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == "Hit")
                {
                    animator.SetTrigger("Hit");
                    break;
                }
            }
        }
    }

    private void EnterFleeState()
    {
        state = AIState.Flee;
        agent.isStopped = false;
        fear = 1f;
        aggression = 0f;
        Vector3 fleeDirection = transform.position - playerTarget.position;
        fleeDirection.y = 0f;
        fleeDirection.Normalize();
        Vector3 fleePoint = transform.position + fleeDirection * fleeDistance;

        if (NavMesh.SamplePosition(fleePoint, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (agent != null)
        {
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (TryGetComponent(out Collider rootCollider))
        {
            rootCollider.enabled = false;
        }

        Rigidbody mainRigidbody = null;
        if (TryGetComponent(out Rigidbody rootRigidbody))
        {
            mainRigidbody = rootRigidbody;
            mainRigidbody.isKinematic = false;
            mainRigidbody.useGravity = true;
            mainRigidbody.velocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            mainRigidbody = gameObject.AddComponent<Rigidbody>();
            mainRigidbody.mass = 1f;
            mainRigidbody.useGravity = true;
        }

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child == transform)
                continue;

            Rigidbody childRigidbody = child.GetComponent<Rigidbody>();
            if (childRigidbody == null)
            {
                childRigidbody = child.gameObject.AddComponent<Rigidbody>();
                childRigidbody.mass = 0.6f;
                childRigidbody.useGravity = true;
            }

            if (child.GetComponent<Collider>() == null)
            {
                BoxCollider boxCollider = child.gameObject.AddComponent<BoxCollider>();
                boxCollider.size = Vector3.one * 0.2f;
            }

            childRigidbody.isKinematic = false;
            childRigidbody.velocity = Vector3.zero;
            childRigidbody.angularVelocity = Vector3.zero;
            child.GetComponent<Collider>().enabled = true;

            if (childRigidbody != mainRigidbody)
            {
                Vector3 direction = (child.position - transform.position).normalized;
                childRigidbody.AddForce(direction * ragdollForce, ForceMode.Impulse);
                childRigidbody.AddTorque(Random.insideUnitSphere * ragdollTorque, ForceMode.Impulse);
            }
        }

        StartCoroutine(DestroyAfterDelay(4f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void TriggerDialogueAnger(int choice)
    {
        if (choice == 1)
        {
            aggression = Mathf.Min(1f, aggression + angerGainFromDialogue);
            fear = Mathf.Max(0f, fear - 0.1f);
            suspiciousTimer = suspiciousLookDuration;
            remembersPlayer = true;
            memoryTimer = memoryDuration;
            lastKnownPlayerPosition = playerTarget != null ? playerTarget.position : transform.position;
            if (dialogueSystem != null)
            {
                dialogueSystem.HideDialogue();
            }
        }
    }

    public void StartDialogue()
    {
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<DialogueSystem>();
        }

        if (dialogueSystem != null)
        {
            dialogueSystem.ShowDialogue(
                new[] { "The skeleton turns to face you...", "Its eyes narrow with contempt." },
                new[] { "Be polite", "Say something rude" },
                TriggerDialogueAnger);
        }
    }

    private void DealTouchDamage()
    {
        if (playerTarget == null)
            return;

        Player player = playerTarget.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(touchDamage);
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
            return;
        }

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }

    private void SetNextPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += homePosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            patrolDestination = hit.position;
            agent.SetDestination(patrolDestination);
        }
    }
}
