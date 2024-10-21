using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiLocomotion : MonoBehaviour
{
    public Transform playerTransform;
    public float detectionRange = 10.0f; // Απόσταση στην οποία το ζόμπι εντοπίζει τον παίκτη
    public float maxTime = 1.0f;
    public float maxDistance = 1.0f;
    public float attackDistance = 2.5f; // Απόσταση στην οποία μπορεί να κάνει attack ο εχθρός
    public int damage = 10;
    private float attackCooldown = 0.9f;
    private float attackTimer = 0.0f;

    int isDeadHash;
    int isAttackingHash;
    int SpeedHash;

    NavMeshAgent agent;
    Animator animator;
    AnimationAndMovmentController playerHealth;

    public Transform[] patrolPoints; // Προκαθορισμένα σημεία περιπολίας
    private int currentPatrolIndex = 0; // Δείκτης για το τρέχον σημείο περιπολίας
    private bool isChasingPlayer = false; // Έλεγχος αν το ζόμπι κυνηγάει τον παίκτη
    private bool isDead = false;

    public int maxHits = 5;
    private int currentHits = 0;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        isDeadHash = Animator.StringToHash("isDead");
        isAttackingHash = Animator.StringToHash("isAttacking");
        SpeedHash = Animator.StringToHash("Speed");

        playerHealth = playerTransform.GetComponent<AnimationAndMovmentController>();
    }

    void Update()
    {
        if (isDead)
        {
            return; // Αν είναι νεκρός, δεν κάνει τίποτα
        }

        attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Αν ο παίκτης είναι κοντά, κυνηγάει τον παίκτη
            isChasingPlayer = true;
            agent.destination = playerTransform.position;
        }
        else if (isChasingPlayer && distanceToPlayer > detectionRange)
        {
            // Αν ο παίκτης βγήκε από την εμβέλεια, επιστρέφει στην περιπολία
            isChasingPlayer = false;
            GoToNextPatrolPoint();
        }

        // Αν δεν κυνηγάει τον παίκτη, συνεχίζει την περιπολία
        if (!isChasingPlayer && !agent.pathPending && agent.remainingDistance < 3.0f)
        {
            GoToNextPatrolPoint();
        }

        // Έλεγχος για επίθεση
        if (isChasingPlayer && distanceToPlayer <= attackDistance && attackTimer <= 0)
        {
            AttackPlayer(true); // Κλήση της μεθόδου επίθεσης
            attackTimer = attackCooldown; // Επαναφορά του χρονόμετρου για την επόμενη επίθεση
        }
        else if (distanceToPlayer > attackDistance)
        {
            AttackPlayer(false);
        }

        // Ενημέρωση του animation με βάση την ταχύτητα του agent
        animator.SetFloat(SpeedHash, agent.velocity.magnitude);
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void AttackPlayer(bool itis)
    {
        if (itis == true)
        {
            // Εκτέλεση του animation επίθεσης
            animator.SetBool(isAttackingHash, true);

            // Κλήση της μεθόδου που προκαλεί ζημιά στον παίκτη
            playerHealth.TakeDamage(damage);
        }
        else
        {
            animator.SetBool(isAttackingHash, false);
        }
    }

    // Όταν χτυπήσει ο εχθρός από σφαίρα
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeHit();  // Καλεί τη μέθοδο TakeHit στον εχθρό
        }
    }


    // Μέθοδος που καλείται όταν ο εχθρός δέχεται χτύπημα (π.χ. από σφαίρα)
    public void TakeHit()
    {
        if (isDead) return;  // Αν είναι ήδη νεκρός, δεν γίνεται τίποτα

        currentHits++;  // Αυξάνουμε τον μετρητή των χτυπημάτων

        if (currentHits >= maxHits)
        {
            Die();  // Αν τα χτυπήματα φτάσουν το όριο, ο εχθρός πεθαίνει
        }
    }

    // Μέθοδος που εκτελείται όταν ο εχθρός πεθαίνει
    void Die()
    {
        isDead = true;

        // Σταματάει η κίνηση και η αναζήτηση του παίκτη
        agent.isStopped = true;

        // Εκκίνηση του animation θανάτου
        animator.SetBool(isDeadHash, true);

        Destroy(gameObject, 5f); // Διαγράφει τον εχθρό 5 δευτερόλεπτα μετά το θάνατό του
    }
}
