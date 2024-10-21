using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiLocomotion : MonoBehaviour
{
    public Transform playerTransform;
    public float maxTime = 1.0f;
    public float maxDistance = 1.0f;
    public float attackDistance = 2.5f; // Απόσταση στην οποία μπορεί να κάνει attack ο εχθρός
    public int damage = 10; // Ποσότητα ζημιάς που κάνει ο εχθρός στον παίκτη
    private float attackCooldown = 0.9f; // Χρονικό διάστημα μεταξύ των επιθέσεων
    private float attackTimer = 0.0f; // Χρονόμετρο για επιθέσεις

    int isDeadHash;
    int isAttackingHash;

    NavMeshAgent agent;
    Animator animator;
    AnimationAndMovmentController playerHealth; // Αναφορά στον παίκτη

    float timer = 0.0f;

    // Μεταβλητές για τη λογική της ζωής και των χτυπημάτων
    public int maxHits = 5;   // Πόσα χτυπήματα δέχεται μέχρι να πεθάνει
    private int currentHits = 0; // Πόσα χτυπήματα έχει δεχτεί
    bool isDead = false; // Έλεγχος αν ο εχθρός είναι νεκρός
    bool isAttacking = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        isDeadHash = Animator.StringToHash("isDead");
        isAttackingHash = Animator.StringToHash("isAttacking");

        // Αναζήτηση του script του παίκτη που χειρίζεται τη ζωή
        playerHealth = playerTransform.GetComponent<AnimationAndMovmentController>();
    }

    void Update()
    {
        if (isDead)
        {
            return; // Αν είναι νεκρός, δεν κάνει τίποτα
        }

        timer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

        if (timer < 0.0f)
        {
            float sqDistance = (playerTransform.position - agent.destination).sqrMagnitude;
            if (sqDistance > maxDistance * maxDistance)
            {
                agent.destination = playerTransform.position;
            }
            timer = maxTime;
        }

        // Ενημέρωση του animation με βάση την ταχύτητα του agent
        animator.SetFloat("Speed", agent.velocity.magnitude);

        // Έλεγχος αν ο εχθρός είναι σε απόσταση επίθεσης
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= attackDistance && attackTimer <= 0)
        {
            AttackPlayer(true); // Κλήση της μεθόδου επίθεσης
            attackTimer = attackCooldown; // Επαναφορά του χρονόμετρου για την επόμενη επίθεση
        }
        else if (distanceToPlayer > attackDistance)
        {
            AttackPlayer(false);
        }
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
