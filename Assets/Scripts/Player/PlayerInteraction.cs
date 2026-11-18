using UnityEngine;
using UnityEngine.InputSystem;
using ZooGame.Core;

namespace ZooGame.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactRange = 1.2f;
        [SerializeField] private LayerMask interactableLayers = ~0; // all layers by default

        private PlayerController playerController;
        private IInteractable currentTarget;

        public IInteractable CurrentTarget => currentTarget;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            CheckForInteractable();

            bool interactPressed = false;
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
                {
                    interactPressed = true;
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.buttonWest.wasPressedThisFrame || gamepad.buttonSouth.wasPressedThisFrame)
                {
                    interactPressed = true;
                }
            }

            if (interactPressed && currentTarget != null)
            {
                currentTarget.Interact(gameObject);
            }
        }

        private void CheckForInteractable()
        {
            Vector2 checkPosition = (Vector2)transform.position + playerController.FacingDirection * 0.8f;
            Collider2D hit = Physics2D.OverlapCircle(checkPosition, interactRange * 0.5f, interactableLayers);

            if (hit != null && hit.gameObject != gameObject)
            {
                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    interactable = hit.GetComponentInParent<IInteractable>();
                }

                currentTarget = interactable;
            }
            else
            {
                currentTarget = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            PlayerController pc = GetComponent<PlayerController>();
            Vector2 dir = pc != null ? pc.FacingDirection : Vector2.down;
            Vector2 checkPos = (Vector2)transform.position + dir * 0.8f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(checkPos, interactRange * 0.5f);
        }
    }
}
