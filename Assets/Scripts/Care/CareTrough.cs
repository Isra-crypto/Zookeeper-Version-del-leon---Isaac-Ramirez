using UnityEngine;
using ZooGame.World;

namespace ZooGame.Care
{
    [RequireComponent(typeof(Collider2D))]
    public class CareTrough : MonoBehaviour
    {
        [Header("Trough Configuration")]
        [SerializeField] private TroughType troughType = TroughType.Food;
        [SerializeField] private bool isFull = false;
        [SerializeField] private Sprite emptySprite;
        [SerializeField] private Sprite fullSprite;

        private SpriteRenderer spriteRenderer;

        public TroughType Type => troughType;
        public bool IsFull => isFull;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = isFull ? fullSprite : emptySprite;
            }
        }

        public bool TryRefill(CareToolType tool)
        {
            bool match = (troughType == TroughType.Food && tool == CareToolType.Comida) ||
                         (troughType == TroughType.Water && tool == CareToolType.Agua);

            if (match)
            {
                if (!isFull)
                {
                    isFull = true;
                    UpdateVisual();
                    string typeName = troughType == TroughType.Food ? "Comedero de Heno 🌾" : "Bebedero de Agua 💧";
                    if (ElephantCareManager.Instance != null)
                    {
                        ElephantCareManager.Instance.ShowActionFeedback($"✨ ¡Has llenado el {typeName}!");
                    }
                    return true;
                }
                else
                {
                    if (ElephantCareManager.Instance != null)
                    {
                        ElephantCareManager.Instance.ShowActionFeedback("ℹ️ Este puesto ya está lleno al máximo.");
                    }
                }
            }
            else
            {
                string neededTool = troughType == TroughType.Food ? "Comida (1)" : "Cubeta de Agua (2)";
                if (ElephantCareManager.Instance != null)
                {
                    ElephantCareManager.Instance.ShowActionFeedback($"⚠️ Selecciona [{neededTool}] para rellenar este puesto.");
                }
            }
            return false;
        }

        private void OnMouseDown()
        {
            if (ElephantCareManager.Instance != null)
            {
                ElephantCareManager.Instance.OnTroughClicked(this);
            }
        }
    }
}
