using System.Collections;
using UnityEngine;
using ZooGame.Core;

namespace ZooGame.World
{
    public enum OstrichGender { Macho, Hembra }

    [RequireComponent(typeof(Rigidbody2D))]
    public class OstrichAnimal : MonoBehaviour, IInteractable
    {
        [Header("Info")]
        [SerializeField] private string ostrichName = "Avestruz";
        [SerializeField] private OstrichGender gender = OstrichGender.Macho;
        [SerializeField] private bool isHungry = true;
        [SerializeField] private int happiness = 60;

        [Header("Animation Frames")]
        [SerializeField] private Sprite[] animFrames;
        [SerializeField] private float frameRate = 0.12f;

        [Header("Wander Settings")]
        [SerializeField] private float moveSpeed = 1.4f;
        [SerializeField] private float wanderRadius = 3f;

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
            StartCoroutine(AnimationLoop());
            StartCoroutine(WanderLoop());
        }

        private IEnumerator AnimationLoop()
        {
            int currentFrame = 0;
            while (true)
            {
                if (animFrames != null && animFrames.Length > 0 && spriteRenderer != null)
                {
                    spriteRenderer.sprite = animFrames[currentFrame % animFrames.Length];
                    currentFrame++;
                }
                yield return new WaitForSeconds(frameRate);
            }
        }

        private IEnumerator WanderLoop()
        {
            while (true)
            {
                rb.linearVelocity = Vector2.zero;
                isMoving = false;
                yield return new WaitForSeconds(Random.Range(2.5f, 5.5f));

                Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
                targetPosition = spawnPosition + randomOffset;
                isMoving = true;

                float moveDuration = Random.Range(1.5f, 3.5f);
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
            string genderStr = gender == OstrichGender.Macho ? "Avestruz Macho" : "Avestruz Hembra";
            if (isHungry)
            {
                return $"[E] Alimentar a {ostrichName} ({genderStr})";
            }
            return $"[E] Acariciar a {ostrichName} (Felicidad: {happiness}%)";
        }

        public void Interact(GameObject interactor)
        {
            if (isHungry)
            {
                isHungry = false;
                happiness = Mathf.Min(100, happiness + 25);
                Debug.Log($"🌾 ¡Has alimentado a {ostrichName}! (+25 Felicidad)");
            }
            else
            {
                happiness = Mathf.Min(100, happiness + 10);
                Debug.Log($"❤️ Acariciaste a {ostrichName}. Felicidad: {happiness}%");
            }

            StartCoroutine(HopEffect());
        }

        private IEnumerator HopEffect()
        {
            Vector3 orig = transform.localScale;
            transform.localScale = orig * 1.25f;
            yield return new WaitForSeconds(0.12f);
            transform.localScale = orig;
        }
    }
}
