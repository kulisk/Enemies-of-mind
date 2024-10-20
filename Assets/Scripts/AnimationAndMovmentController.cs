using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovmentController : MonoBehaviour
{
    PlayerInput PlayerInput;
    CharacterController characterController;
    Animator animator;

    int isWalkingHash;
    int isRunningHash;

    Vector2 currentMovementInput;
    Vector2 currentLookInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    Vector3 currentLook;
    bool isMovementPressed;
    bool isRunPressed;
    
    float rotationFactorPerFrame = 1.0f;
    float runMultiplier = 4.0f;
    float mouseSensitivity = 3.0f;

    float cameraPitch = 0.0f; // Για τον έλεγχο της κατακόρυφης περιστροφής
    public Transform cameraTransform; // Σύνδεση με την κάμερα του παίκτη
    public float maxPitchAngle = 40.0f; // Όριο προς τα πάνω
    public float minPitchAngle = -40.0f; // Όριο προς τα κάτω

    void Awake()
    {
        PlayerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        PlayerInput.CharacterControls.Move.started += onMovementInput;
        PlayerInput.CharacterControls.Move.canceled += onMovementInput;
        PlayerInput.CharacterControls.Move.performed += onMovementInput;
        PlayerInput.CharacterControls.Run.started += onRun;
        PlayerInput.CharacterControls.Run.canceled += onRun;

        PlayerInput.CharacterControls.Look.started += onLookInput;
        PlayerInput.CharacterControls.Look.performed += onLookInput;
        PlayerInput.CharacterControls.Look.canceled += onLookInput;

        Cursor.lockState = CursorLockMode.Locked;
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
            animator.SetBool("isWalking", true);
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool("isWalking", false);
        }

        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
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
        // Κατεύθυνση που κοιτάει ο χαρακτήρας με βάση το transform.forward
        Vector3 moveDirection = (transform.forward * currentMovementInput.y) + (transform.right * currentMovementInput.x);

        // Κίνηση τρεξίματος ή περπατήματος
        if (isRunPressed)
        {
            characterController.Move(moveDirection * runMultiplier * Time.deltaTime);
        }
        else
        {
            characterController.Move(moveDirection * Time.deltaTime);
        }
    }

    // Update is called once per frame
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
