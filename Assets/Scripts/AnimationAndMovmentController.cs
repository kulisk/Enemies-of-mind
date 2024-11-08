using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AnimationAndMovmentController : MonoBehaviour
{
    PlayerInput PlayerInput;
    CharacterController characterController;
    Animator animator;

    public Slider HealthBar;

    int isWalkingHash;
    int isRunningHash;
    int isDeadHash;

    Vector2 currentMovementInput;
    Vector2 currentLookInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 currentLook;
    bool isMovementPressed;
    bool isRunPressed;
    bool isDead;
    
    float rotationFactorPerFrame = 1.0f;
    float runMultiplier = 5.0f;
    float mouseSensitivity = 3.0f;

    float cameraPitch = 0.0f;
    public Transform cameraTransform;
    public float maxPitchAngle = 40.0f;
    public float minPitchAngle = -40.0f;
    public GameObject bulletPrefab; // Prefab για το βλήμα
    public Transform shootPoint; // Σημείο από όπου θα πυροβολεί ο παίκτης
    public float bulletSpeed = 30f; // Ταχύτητα του βλήματος

    // Νέες μεταβλητές για τη ζωή του παίκτη
    public int maxHealth = 1000; // Η μέγιστη ζωή του παίκτη
    private int currentHealth;  // Η τρέχουσα ζωή του παίκτη

    // Νέες μεταβλητές για ήχους
    public AudioClip walkSound;  // Ήχος περπατήματος
    public AudioClip runSound;   // Ήχος τρεξίματος
    public AudioClip shootSound; // Ήχος πυροβολισμού
    public AudioClip deathSound; // Ήχος θανάτου
    private AudioSource audioSource; // Ηχητική πηγή

    void Awake()
    {
        PlayerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // Βρίσκουμε την ηχητική πηγή

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isDeadHash = Animator.StringToHash("isDead");

        PlayerInput.CharacterControls.Move.started += onMovementInput;
        PlayerInput.CharacterControls.Move.canceled += onMovementInput;
        PlayerInput.CharacterControls.Move.performed += onMovementInput;
        PlayerInput.CharacterControls.Run.started += onRun;
        PlayerInput.CharacterControls.Run.canceled += onRun;

        PlayerInput.CharacterControls.Look.started += onLookInput;
        PlayerInput.CharacterControls.Look.performed += onLookInput;
        PlayerInput.CharacterControls.Look.canceled += onLookInput;

        PlayerInput.CharacterControls.Shoot.started += onShoot; // Σύνδεση για το Shoot input

        Cursor.lockState = CursorLockMode.Locked;

        // Αρχικοποίηση της ζωής
        currentHealth = maxHealth;
        HealthBar.value = maxHealth;
        HealthBar.maxValue = maxHealth;
        isDead = false;
    }

    // Μέθοδος για να δέχεται ζημιά ο παίκτης
    public void TakeDamage(int damage)
    {
        if (isDead==false)
        {
            currentHealth -= damage; // Μείωση της ζωής
            Debug.Log("Player Health: " + currentHealth); // Μήνυμα στο console για έλεγχο

            if (currentHealth <= 0)
            {
                Die(); // Κλήση της μεθόδου Die αν η ζωή φτάσει στο 0
            }
        }
    }

    // Μέθοδος για να πεθαίνει ο παίκτης
    void Die()
    {
        if (isDead == false)
        {
            Debug.Log("Player is Dead");
            animator.SetBool(isDeadHash, true);
            audioSource.PlayOneShot(deathSound); 
            characterController.enabled = false;
            isDead = true;
        }
    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void onLookInput(InputAction.CallbackContext context)
    {
        currentLookInput = context.ReadValue<Vector2>();
    }

    void onShoot(InputAction.CallbackContext context)
    {
        if (isDead == false)
        {
            Shoot(); // Κάθε φορά που γίνεται Shoot, καλούμε τη μέθοδο Shoot
        }
    }

    void handleRotation()
    {
        // Οριζόντια περιστροφή του παίκτη (Y άξονας)
        float mouseX = currentLookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX * rotationFactorPerFrame);

        // Κατακόρυφη περιστροφή της κάμερας (X άξονας)
        float mouseY = currentLookInput.y * mouseSensitivity * Time.deltaTime;
        cameraPitch -= mouseY; // Μείωση της γωνίας για πάνω/κάτω κίνηση

        // Περιορισμός της περιστροφής μεταξύ των ορίων
        cameraPitch = Mathf.Clamp(cameraPitch, minPitchAngle, maxPitchAngle);

        // Εφαρμογή της κίνησης στην κάμερα
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0.0f, 0.0f);
    }

    void handleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
            audioSource.PlayOneShot(walkSound); // Παίζει ήχο περπατήματος
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
            audioSource.PlayOneShot(runSound); // Παίζει ήχο τρεξίματος
        }
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
    }

    void handleGravity()
    {
        if (characterController.isGrounded)
        {
            float groundedGravity = -.05f;
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        }
        else
        {
            float gravity = -9.8f;
            currentMovement.y = gravity;
            currentRunMovement.y = gravity;
        }
    }

    void handleMovement()
    {
        Vector3 moveDirection = (transform.forward * currentMovementInput.y) + (transform.right * currentMovementInput.x);

        if (isRunPressed)
        {
            characterController.Move(moveDirection * runMultiplier * Time.deltaTime);
        }
        else
        {
            characterController.Move(moveDirection * Time.deltaTime);
        }
    }

    void Shoot()
    {
        // Δημιουργία βλήματος
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        
        // Πρόσθεση ταχύτητας στο βλήμα
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.velocity = shootPoint.forward * bulletSpeed;

        // Παίζει ήχο πυροβολισμού
        audioSource.PlayOneShot(shootSound);

        // Καταστροφή του βλήματος μετά από κάποιο χρονικό διάστημα
        Destroy(bullet, 5f);
    }

    void Update()
    {
        handleRotation();
        handleAnimation();
        handleGravity();
        handleMovement();

        if (isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }

        HealthBar.value = currentHealth;
    }

    void OnEnable()
    {
        PlayerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        PlayerInput.CharacterControls.Disable();
    }
}
