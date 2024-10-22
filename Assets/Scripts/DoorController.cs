using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float openDistance = 2.0f;  // Η απόσταση από την πόρτα που θα ανοίγει
    public bool Lock = false;  // Αν η πόρτα είναι κλειδωμένη
    public float doorOpenHeight = 3.0f;  // Το ύψος που θα ανεβαίνει η πόρτα όταν ανοίγει
    public float openSpeed = 2.0f;  // Η ταχύτητα με την οποία θα ανοίγει/κλείνει η πόρτα
    public AudioClip openSound;  // Ο ήχος που παίζει όταν ανοίγει η πόρτα
    public AudioClip closeSound;  // Ο ήχος που παίζει όταν κλείνει η πόρτα

    private Vector3 initialPosition;  // Η αρχική θέση της πόρτας
    private Vector3 openPosition;  // Η τελική θέση όταν ανοίγει η πόρτα
    private AudioSource audioSource;  // Αναπαραγωγή του ήχου
    private bool isOpen = false;  // Έλεγχος αν η πόρτα είναι ανοιχτή

    void Start()
    {
        // Αποθήκευση της αρχικής θέσης της πόρτας
        initialPosition = transform.position;

        // Ορισμός της θέσης που θα πάει η πόρτα όταν ανοίξει
        openPosition = new Vector3(initialPosition.x, initialPosition.y + doorOpenHeight, initialPosition.z);

        // Πρόσθεση του AudioSource component για αναπαραγωγή ήχου
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Έλεγχος αν η πόρτα είναι κλειδωμένη
        if (Lock)
            return;

        // Έλεγχος για όλους τους παίκτες που έχουν το tag "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        bool playerIsNear = false;

        foreach (GameObject player in players)
        {
            // Υπολογισμός της απόστασης ανάμεσα στον παίκτη και την πόρτα
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

            // Αν υπάρχει παίκτης εντός της απόστασης, ανοίγει η πόρτα
            if (distanceToPlayer <= openDistance)
            {
                playerIsNear = true;
                break;
            }
        }

        // Έλεγχος αν ο παίκτης πλησίασε και αν η πόρτα δεν είναι ήδη ανοιχτή
        if (playerIsNear && !isOpen)
        {
            StopAllCoroutines();  // Σταμάτημα τυχόν άλλης κίνησης της πόρτας
            StartCoroutine(OpenDoor());  // Ξεκινάει η διαδικασία ανοίγματος
        }
        // Έλεγχος αν ο παίκτης απομακρύνθηκε και αν η πόρτα είναι ανοιχτή
        else if (!playerIsNear && isOpen)
        {
            StopAllCoroutines();
            StartCoroutine(CloseDoor());  // Ξεκινάει η διαδικασία κλεισίματος
        }
    }

    // Μέθοδος που ανοίγει την πόρτα
    IEnumerator OpenDoor()
    {
        isOpen = true;

        // Παίζει ο ήχος ανοίγματος
        audioSource.clip = openSound;
        audioSource.Play();

        // Κίνηση της πόρτας προς τα πάνω
        while (Vector3.Distance(transform.position, openPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, openPosition, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.position = openPosition;  // Εξασφάλιση ότι η πόρτα θα είναι ακριβώς στη θέση ανοίγματος
    }

    // Μέθοδος που κλείνει την πόρτα
    IEnumerator CloseDoor()
    {
        isOpen = false;

        // Παίζει ο ήχος κλεισίματος
        audioSource.clip = closeSound;
        audioSource.Play();

        // Κίνηση της πόρτας προς την αρχική θέση
        while (Vector3.Distance(transform.position, initialPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.position = initialPosition;  // Εξασφάλιση ότι η πόρτα θα επιστρέψει ακριβώς στην αρχική θέση
    }
}
