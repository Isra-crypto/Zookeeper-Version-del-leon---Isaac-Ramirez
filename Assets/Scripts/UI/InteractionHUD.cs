using UnityEngine;
using UnityEngine.UI;
using ZooGame.Player;

namespace ZooGame.UI
{
    public class InteractionHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private GameObject promptContainer;
        [SerializeField] private Text promptText;

        private void Start()
        {
            if (playerInteraction == null)
            {
                playerInteraction = FindFirstObjectByType<PlayerInteraction>();
            }

            if (promptContainer != null)
            {
                promptContainer.SetActive(false);
            }
        }

        private void Update()
        {
            if (playerInteraction == null) return;

            var target = playerInteraction.CurrentTarget;
            if (target != null)
            {
                if (promptContainer != null) promptContainer.SetActive(true);
                if (promptText != null) promptText.text = target.GetInteractionPrompt();
            }
            else
            {
                if (promptContainer != null) promptContainer.SetActive(false);
            }
        }
    }
}
