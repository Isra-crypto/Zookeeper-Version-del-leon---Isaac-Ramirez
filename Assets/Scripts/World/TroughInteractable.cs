using UnityEngine;
using ZooGame.Core;

namespace ZooGame.World
{
    public enum TroughType { Food, Water }

    public class TroughInteractable : MonoBehaviour, IInteractable
    {
        [Header("Trough Settings")]
        [SerializeField] private TroughType type = TroughType.Food;
        [SerializeField] private bool isFull = false;

        [Header("Sprites")]
        [SerializeField] private Sprite emptySprite;
        [SerializeField] private Sprite fullSprite;

        private SpriteRenderer spriteRenderer;

        public bool IsFull => isFull;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateVisual();
        }

        public string GetInteractionPrompt()
        {
            string typeStr = type == TroughType.Food ? "comedero" : "abrevadero";
            if (!isFull)
            {
                return $"[E] Llenar {typeStr}";
            }
            return $"{char.ToUpper(typeStr[0]) + typeStr.Substring(1)} (Lleno)";
        }

        public void Interact(GameObject interactor)
        {
            if (!isFull)
            {
                isFull = true;
                UpdateVisual();
                string typeStr = type == TroughType.Food ? "comida fresca" : "agua limpia";
                Debug.Log($"🌾 ¡Has llenado el recipiente con {typeStr}!");
            }
            else
            {
                Debug.Log("El recipiente ya está lleno y listo para los animales.");
            }
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.sprite = isFull ? fullSprite : emptySprite;
        }
    }
}
