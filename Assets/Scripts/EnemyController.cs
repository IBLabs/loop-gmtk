using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseHeight = 2f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Movement Boundaries")]
    [SerializeField] private Transform minXBoundary; // Left boundary (optional - assign a transform)
    [SerializeField] private Transform maxXBoundary; // Right boundary (optional - assign a transform)
    [SerializeField] private float playerWidth = 1f; // Width of the player to prevent wall clipping
    [SerializeField] private Vector3 axis;

    [Header("Horizontal Movement")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float accelerationTime = 0.5f;
    [SerializeField] private float decelerationTime = 0.3f;
    [SerializeField] private float airAccelerationTime = 0.2f; // Faster air acceleration
    [SerializeField] private float airDecelerationTime = 0.2f; // Faster air deceleration
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve decelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve airAccelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve airDecelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool useAcceleration = true;
    [SerializeField] private float angularSpeed = 0.5f;

    [Header("Air Control")]
    [SerializeField][Range(0f, 1f)] private float airControlFactor = 0.8f; // How much control you have in air
    [SerializeField] private bool allowAirDirectionChange = true; // Allow changing direction mid-air
    [SerializeField] private float airMaxSpeedMultiplier = 0.9f; // Reduce max speed in air slightly

    [Header("Input")]
    [SerializeField] private GameObject assasinationTarget;
    [SerializeField] private InputActionProperty jumpAction;
    [SerializeField] private InputActionProperty moveAction;

    [Header("Effects")]
    [SerializeField] private Transform jumpEffect;

    private bool isJumping = false;
    private float jumpStartTime;
    private float startHeight;
    private Vector2 velocity;
    private Vector2 inputVector;

    // Movement state tracking
    [SerializeField] private float angle = 0;
    private float accelerationStartTime;
    private float lastInputDirection;
    private bool isAccelerating = false;
    private bool isDecelerating = false;
    private float velocityAtAccelerationStart;

    private void OnEnable()
    {
        jumpAction.action?.Enable();
        moveAction.action?.Enable();
    }

    private void OnDisable()
    {
        jumpAction.action?.Disable();
        moveAction.action?.Disable();
    }

    private void Start()
    {
        // Set initial position to base height
        Vector3 startPos = transform.position;
        startPos.y = baseHeight;
        transform.position = startPos;
    }

    private void Update()
    {
        HandleInput();
        UpdateMovement();
        UpdatePosition();
    }

    private void HandleInput()
    {
        // Get horizontal input
        inputVector.x = Mathf.Clamp(assasinationTarget.transform.position.x - transform.position.x, -1, 1);

        var height = Vector2.Distance(new Vector2(6.16f + transform.position.z, transform.position.y), Vector2.zero);
        var targetAngle = Mathf.Atan(assasinationTarget.transform.position.y / (assasinationTarget.transform.position.z - 6.16f)) * 180 / Mathf.PI;
        var angleChangeFactor = Mathf.Abs((Mathf.Abs(targetAngle - angle) % 360) - 180) / 180;
        //inputVector.y = Mathf.Clamp(angleChange, -1, 1);
        inputVector.y = angularSpeed + angleChangeFactor;

        // Handle jump input
        //if (jumpAction.action?.WasPressedThisFrame() == true && !isJumping)
        //{
        //    StartJump();
        //}
    }

    private void StartJump()
    {
        isJumping = true;
        jumpStartTime = Time.time;
        startHeight = Vector2.Distance(new Vector2(6.16f + transform.position.z, transform.position.y), Vector2.zero);

        // Play jump effect if assigned
        if (jumpEffect != null)
        {
            Transform newJumpEffect = Instantiate(jumpEffect, transform.position, Quaternion.identity);
            Destroy(newJumpEffect.gameObject, 1f);
        }
    }

    private void UpdateMovement()
    {
        float deltaTime = Time.deltaTime;
        float currentMaxSpeed = isJumping ? maxSpeed * airMaxSpeedMultiplier : maxSpeed;
        float targetVelocityX = inputVector.x * currentMaxSpeed;
        float targetVelocityY = inputVector.y * currentMaxSpeed;

        if (useAcceleration)
        {
            // Determine current movement curves and times based on air/ground state
            AnimationCurve currentAccelCurve = isJumping ? airAccelerationCurve : accelerationCurve;
            AnimationCurve currentDecelCurve = isJumping ? airDecelerationCurve : decelerationCurve;
            float currentAccelTime = isJumping ? airAccelerationTime : accelerationTime;
            float currentDecelTime = isJumping ? airDecelerationTime : decelerationTime;

            bool hasInput = Mathf.Abs(inputVector.x) > 0.1f || Mathf.Abs(inputVector.y) > 0.1f;
            bool directionChanged = hasInput && (Mathf.Sign(inputVector.x) != Mathf.Sign(lastInputDirection)) && Mathf.Abs(lastInputDirection) > 0.1f;

            // Special air control handling
            if (isJumping && hasInput)
            {
                if (!allowAirDirectionChange && directionChanged)
                {
                    // Don't allow direction changes in air if disabled
                    targetVelocityX = Mathf.Sign(lastInputDirection) * currentMaxSpeed;
                }
                else if (directionChanged && allowAirDirectionChange)
                {
                    // Apply air control factor for direction changes
                    float airControlledTarget = velocity.x + (targetVelocityX - velocity.x) * airControlFactor;
                    targetVelocityX = airControlledTarget;
                }
            }

            if (hasInput)
            {
                // Check if we need to start a new acceleration phase
                if (!isAccelerating || directionChanged)
                {
                    StartAcceleration(targetVelocityX, directionChanged);
                }

                // Calculate acceleration progress
                float accelerationProgress = (Time.time - accelerationStartTime) / currentAccelTime;
                accelerationProgress = Mathf.Clamp01(accelerationProgress);

                // Use curve to determine velocity
                float curveValue = currentAccelCurve.Evaluate(accelerationProgress);
                velocity.x = Mathf.Lerp(velocityAtAccelerationStart, targetVelocityX, curveValue);

                // Mark acceleration as complete if we've reached the target
                if (accelerationProgress >= 1f)
                {
                    isAccelerating = false;
                    velocity.x = targetVelocityX;
                }

                lastInputDirection = inputVector.x;
                isDecelerating = false;
            }
            else
            {
                if (!isDecelerating)
                {
                    StartDeceleration();
                }

                float decelerationProgress = (Time.time - accelerationStartTime) / currentDecelTime;
                decelerationProgress = Mathf.Clamp01(decelerationProgress);

                float curveValue = currentDecelCurve.Evaluate(decelerationProgress);
                velocity.x = Mathf.Lerp(velocityAtAccelerationStart, 0f, curveValue);

                if (decelerationProgress >= 1f)
                {
                    isDecelerating = false;
                    velocity.x = 0f;
                }

                isAccelerating = false;
            }
        }
        else
        {
            velocity.x = targetVelocityX;
            lastInputDirection = inputVector.x;
        }

        velocity.x = Mathf.Clamp(velocity.x, -currentMaxSpeed, currentMaxSpeed);
        velocity.y = Mathf.Clamp(targetVelocityY, -currentMaxSpeed, currentMaxSpeed);
    }

    private void StartAcceleration(float targetVelocity, bool directionChanged)
    {
        isAccelerating = true;
        isDecelerating = false;
        accelerationStartTime = Time.time;
        velocityAtAccelerationStart = velocity.x;

        if (directionChanged && Mathf.Abs(velocity.x) > 0.1f)
        {
            StartDeceleration();
        }
    }

    private void StartDeceleration()
    {
        isDecelerating = true;
        isAccelerating = false;
        accelerationStartTime = Time.time;
        velocityAtAccelerationStart = velocity.x;
    }

    private void UpdatePosition()
    {
        Vector3 newPosition = transform.position;

        newPosition.x += velocity.x * Time.deltaTime;

        // Apply movement boundaries
        float halfPlayerWidth = playerWidth * 0.5f;
        if (minXBoundary != null)
        {
            float minX = minXBoundary.position.x + halfPlayerWidth;
            newPosition.x = Mathf.Max(newPosition.x, minX);
        }
        if (maxXBoundary != null)
        {
            float maxX = maxXBoundary.position.x - halfPlayerWidth;
            newPosition.x = Mathf.Min(newPosition.x, maxX);
        }

        float height;
        if (isJumping)
        {
            float elapsed = Time.time - jumpStartTime;
            float normalizedTime = elapsed / jumpDuration;

            if (normalizedTime >= 1f)
            {
                isJumping = false;
                normalizedTime = 1f;
            }
            float curveValue = jumpCurve.Evaluate(normalizedTime);
            float newYValue = Mathf.Lerp(startHeight, startHeight + jumpHeight, curveValue);
            height = newYValue;
        }
        else
        {
            height = baseHeight;
        }
        var arch_length = velocity.y * Time.deltaTime;
        angle += (360 * arch_length) / (2 * Mathf.PI * height);
        var angleRadians = (angle * Mathf.PI) / 180;
        newPosition.y = Mathf.Cos(angleRadians) * height + axis.y;
        newPosition.z = Mathf.Sin(angleRadians) * height + axis.z;

        transform.rotation = Quaternion.Euler(angle, 0, 0);

        transform.position = newPosition;
    }
}
