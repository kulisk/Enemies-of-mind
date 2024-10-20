using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiLocomotion : MonoBehaviour
{
    public Transform playerTransform;
    public float maxTime = 1.0f;
    public float maxDistance = 1.0f;


    NavMeshAgent agent;
    Animator animator;
    float timer = 0.0f;

    // Μεταβλητές για τη λογική της ζωής και των χτυπημάτων
    public int maxHits = 5;   // Πόσα χτυπήματα δέχεται μέχρι να πεθάνει
    private int currentHits = 0; // Πόσα χτυπήματα έχει δεχτεί

    private bool isDead = false; // Έλεγχος αν ο εχθρός είναι νεκρός

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            // Αν ο εχθρός είναι νεκρός, δεν κυνηγάει πλέον τον παίκτη
            return;
        }

        timer -= Time.deltaTime;
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
    }

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
        animator.SetTrigger("Die");

        // Μπορείς να βάλεις εδώ επιπλέον λογική, π.χ. να διαγραφεί ο εχθρός μετά από λίγο.
        Destroy(gameObject, 5f); // Διαγράφει τον εχθρό 5 δευτερόλεπτα μετά το θάνατό του
    }
}
