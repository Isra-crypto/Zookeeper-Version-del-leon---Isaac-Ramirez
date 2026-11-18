using System.Collections;
using UnityEngine;
using ZooGame.Core;

namespace ZooGame.World
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ZooAnimal : MonoBehaviour, IInteractable
    {
        [Header("Animal Info")]
        [SerializeField] private string speciesName = "Capibara";
        [SerializeField] private bool isHungry = true;
        [SerializeField] private int happiness = 50;

        [Header("Wander Settings")]
        [SerializeField] private float moveSpeed = 1.2f;
        [SerializeField] private float wanderRadius = 3f;
        [SerializeField] private float minWaitTime = 2f;
        [SerializeField] private float maxWaitTime = 5f;

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
            targetPosition = spawnPosition;
        }

        private void Start()
        {
            StartCoroutine(WanderRoutine());
        }

        private IEnumerator WanderRoutine()
        {
            while (true)
            {
                float waitTime = Random.Range(minWaitTime, maxWaitTime);
                isMoving = false;
                rb.linearVelocity = Vector2.zero;
                yield return new WaitForSeconds(waitTime);

                // Pick random point inside enclosure wander radius
                Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
                targetPosition = spawnPosition + randomOffset;
                isMoving = true;

                float moveDuration = Random.Range(1.5f, 3f);
                float timer = 0f;

                while (timer < moveDuration && Vector2.Distance(transform.position, targetPosition) > 0.2f)
                {
                    Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
                    rb.linearVelocity = dir * moveSpeed;

                    if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.05f)
                    {
                        spriteRenderer.flipX = dir.x < 0;
                    }

                    timer += Time.deltaTime;
                    yield return null;
                }

                rb.linearVelocity = Vector2.zero;
                isMoving = false;
            }
        }

        public string GetInteractionPrompt()
        {
            if (isHungry)
            {
                return $"[E] Alimentar a {speciesName}";
            }
            return $"[E] Acariciar a {speciesName} (Felicidad: {happiness}%)";
        }

        public void Interact(GameObject interactor)
        {
            if (isHungry)
            {
                isHungry = false;
                happiness = Mathf.Min(100, happiness + 25);
                Debug.Log($"🍎 ¡Has alimentado a {speciesName}! Ahora está feliz y satisfecho (+25 Felicidad).");
            }
            else
            {
                happiness = Mathf.Min(100, happiness + 10);
                Debug.Log($"❤️ Acariciaste a {speciesName}. ¡Le agradas mucho! Felicidad actual: {happiness}%.");
            }

            // Play cute hop
            StartCoroutine(SmallHopEffect());
        }

        private IEnumerator SmallHopEffect()
        {
            Vector3 originalScale = transform.localScale;
            transform.localScale = originalScale * 1.25f;
            yield return new WaitForSeconds(0.15f);
            transform.localScale = originalScale;
        }
    }
}
