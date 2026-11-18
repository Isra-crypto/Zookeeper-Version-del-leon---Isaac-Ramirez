using System.Collections;
using UnityEngine;
using ZooGame.Core;

namespace ZooGame.World
{
    public enum ElephantStage { Bebe, Hembra, Macho }
    public enum ElephantNeed { Ninguna, Hambre, Sed, Sucio, Cepillado, Medicina }

    [RequireComponent(typeof(Rigidbody2D))]
    public class ElephantAnimal : MonoBehaviour, IInteractable
    {
        [Header("Elephant Info")]
        [SerializeField] private string elephantName = "Tantor";
        [SerializeField] private ElephantStage stage = ElephantStage.Macho;
        [SerializeField] private ElephantNeed currentNeed = ElephantNeed.Hambre;
        [Range(0, 100)]
        [SerializeField] private int happiness = 70;

        [Header("Sprites")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite walk1Sprite;
        [SerializeField] private Sprite walk2Sprite;

        [Header("Wander Settings")]
        [SerializeField] private float moveSpeed = 1.0f;
        [SerializeField] private float wanderRadius = 3.5f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Vector2 spawnPosition;
        private Vector2 targetPosition;
        private bool isMoving;
        public bool IsMoving => isMoving;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            spawnPosition = transform.position;
        }

        private void Start()
        {
            StartCoroutine(WanderAndAnimateRoutine());
        }

        private IEnumerator WanderAndAnimateRoutine()
        {
            while (true)
            {
                // Idle state
                isMoving = false;
                rb.linearVelocity = Vector2.zero;
                if (idleSprite != null && spriteRenderer != null) spriteRenderer.sprite = idleSprite;

                yield return new WaitForSeconds(Random.Range(3f, 6f));

                // Move state
                Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
                targetPosition = spawnPosition + randomOffset;
                isMoving = true;

                float moveDuration = Random.Range(2f, 4f);
                float timer = 0f;
                int animFrame = 0;
                float animTimer = 0f;

                while (timer < moveDuration && Vector2.Distance(transform.position, targetPosition) > 0.2f)
                {
                    Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
                    rb.linearVelocity = dir * moveSpeed;

                    if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.05f)
                    {
                        spriteRenderer.flipX = dir.x < 0;
                    }

                    // Animate walk frames
                    animTimer += Time.deltaTime;
                    if (animTimer >= 0.22f)
                    {
                        animTimer = 0f;
                        animFrame = (animFrame + 1) % 4;
                        if (spriteRenderer != null)
                        {
                            switch (animFrame)
                            {
                                case 0: spriteRenderer.sprite = idleSprite; break;
                                case 1: spriteRenderer.sprite = walk1Sprite; break;
                                case 2: spriteRenderer.sprite = idleSprite; break;
                                case 3: spriteRenderer.sprite = walk2Sprite; break;
                            }
                        }
                    }

                    timer += Time.deltaTime;
                    yield return null;
                }

                rb.linearVelocity = Vector2.zero;
                isMoving = false;
                if (idleSprite != null && spriteRenderer != null) spriteRenderer.sprite = idleSprite;
            }
        }

        public string GetInteractionPrompt()
        {
            string stageStr = stage switch
            {
                ElephantStage.Bebe => "Elefantito",
                ElephantStage.Hembra => "Elefanta",
                _ => "Elefante"
            };

            return currentNeed switch
            {
                ElephantNeed.Hambre => $"[E] Alimentar a {elephantName} ({stageStr})",
                ElephantNeed.Sed => $"[E] Dar agua a {elephantName} ({stageStr})",
                ElephantNeed.Sucio => $"[E] Limpiar y bañar a {elephantName} ({stageStr})",
                ElephantNeed.Medicina => $"[E] Administrar medicina a {elephantName} ({stageStr})",
                _ => $"[E] Acariciar a {elephantName} ({stageStr}) - Felicidad: {happiness}%"
            };
        }

        public void Interact(GameObject interactor)
        {
            if (currentNeed != ElephantNeed.Ninguna)
            {
                happiness = Mathf.Min(100, happiness + 20);
                Debug.Log($"✨ ¡Atendiste a {elephantName}! Felicidad: {happiness}%");
                currentNeed = ElephantNeed.Ninguna;
            }
            else
            {
                happiness = Mathf.Min(100, happiness + 5);
                Debug.Log($"❤️ Acariciaste con cariño a {elephantName}.");
            }

            StartCoroutine(HopEffect());
        }

        private IEnumerator HopEffect()
        {
            Vector3 orig = transform.localScale;
            transform.localScale = orig * 1.18f;
            yield return new WaitForSeconds(0.15f);
            transform.localScale = orig;
        }
    }
}
