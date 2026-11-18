using UnityEngine;
using UnityEngine.InputSystem;

namespace ZooGame.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float runSpeed = 7.0f;
        
        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform targetCursor;

        private Rigidbody2D rb;
        private Vector2 movementInput;
        private Vector2 lastFacingDirection = Vector2.down;
        private bool isRunning;

        public Vector2 FacingDirection => lastFacingDirection;
        public Vector2 MovementInput => movementInput;
        public bool IsMoving => movementInput.sqrMagnitude > 0.01f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Update()
        {
            float moveX = 0f;
            float moveY = 0f;
            isRunning = false;

            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) moveY += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) moveY -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveX -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveX += 1f;

                isRunning = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.05f)
                {
                    moveX += stick.x;
                    moveY += stick.y;
                }
                if (gamepad.dpad.up.isPressed) moveY += 1f;
                if (gamepad.dpad.down.isPressed) moveY -= 1f;
                if (gamepad.dpad.left.isPressed) moveX -= 1f;
                if (gamepad.dpad.right.isPressed) moveX += 1f;

                if (gamepad.buttonSouth.isPressed || gamepad.rightTrigger.isPressed)
                {
                    isRunning = true;
                }
            }

            movementInput = new Vector2(moveX, moveY);
            if (movementInput.sqrMagnitude > 1f)
            {
                movementInput.Normalize();
            }

            // Update facing direction (4-directional Stardew-style priority)
            if (movementInput.sqrMagnitude > 0.01f)
            {
                if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
                {
                    lastFacingDirection = new Vector2(Mathf.Sign(moveX), 0);
                }
                else
                {
                    lastFacingDirection = new Vector2(0, Mathf.Sign(moveY));
                }

                // Sprite flip for left/right
                if (spriteRenderer != null && Mathf.Abs(moveX) > 0.01f)
                {
                    spriteRenderer.flipX = moveX < 0;
                }
            }

            // Update cursor / selector position in front of player
            if (targetCursor != null)
            {
                Vector3 targetTilePos = transform.position + (Vector3)lastFacingDirection * 1.0f;
                targetCursor.position = new Vector3(
                    Mathf.Round(targetTilePos.x),
                    Mathf.Round(targetTilePos.y),
                    0f
                );
            }
        }

        private void FixedUpdate()
        {
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            rb.linearVelocity = movementInput * currentSpeed;
        }
    }
}
