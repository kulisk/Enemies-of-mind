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
    float mouseSensitivity = 5.0f;




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
       float mouseX = currentLookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX * rotationFactorPerFrame);
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
            float groundedGravety = -.05f;
            currentMovement.y = groundedGravety;
            currentRunMovement.y = groundedGravety;
        }
        else{
            float gravety = -9.8f;
            currentMovement.y = gravety;
            currentRunMovement.y = gravety;
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
            characterController.Move( currentRunMovement * Time.deltaTime);
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







