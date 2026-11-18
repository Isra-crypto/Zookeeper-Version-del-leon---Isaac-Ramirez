using UnityEngine;

namespace ZooGame.Core
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class YSortRenderer : MonoBehaviour
    {
        [SerializeField] private int baseSortingOrder = 100;
        [SerializeField] private float yOffset = 0f;
        [SerializeField] private bool isStatic = false;

        private SpriteRenderer spriteRenderer;

        public float YOffset
        {
            get => yOffset;
            set { yOffset = value; UpdateSorting(); }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSorting();
        }

        private void OnEnable()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSorting();
        }

        private void OnValidate()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSorting();
        }

        private void LateUpdate()
        {
            if (!isStatic || !Application.isPlaying)
            {
                UpdateSorting();
            }
        }

        public void UpdateSorting()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = baseSortingOrder - Mathf.RoundToInt((transform.position.y + yOffset) * 10f);
            }
        }
    }
}
