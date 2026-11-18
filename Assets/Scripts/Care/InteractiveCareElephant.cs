using System.Collections;
using UnityEngine;
using ZooGame.World;

namespace ZooGame.Care
{
    [RequireComponent(typeof(Collider2D))]
    public class InteractiveCareElephant : MonoBehaviour
    {
        [Header("Elephant Info")]
        [SerializeField] private string elephantName = "Tantor";
        [SerializeField] private ElephantStage stage = ElephantStage.Macho;
        [SerializeField] private ElephantNeed currentNeed = ElephantNeed.Hambre;
        [Range(0, 100)]
        [SerializeField] private int happiness = 65;

        [Header("Animation Frames")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite walk1Sprite;
        [SerializeField] private Sprite walk2Sprite;

        [Header("Emotes")]
        [SerializeField] private SpriteRenderer emoteRenderer;
        [SerializeField] private Sprite heartEmote;
        [SerializeField] private Sprite foodEmote;
        [SerializeField] private Sprite waterEmote;
        [SerializeField] private Sprite dirtyEmote;
        [SerializeField] private Sprite medicineEmote;

        [Header("Wander Settings")]
        [SerializeField] private float moveSpeed = 0.8f;
        [SerializeField] private Vector2 roamAreaMin = new Vector2(-6f, -3f);
        [SerializeField] private Vector2 roamAreaMax = new Vector2(6f, 3f);

        private SpriteRenderer spriteRenderer;
        private Vector2 targetPosition;
        private bool isInteracting;

        public string ElephantName => elephantName;
        public ElephantStage Stage => stage;
        public ElephantNeed CurrentNeed => currentNeed;
        public int Happiness => happiness;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            targetPosition = transform.position;
        }

        private void Start()
        {
            UpdateEmoteIcon();
            StartCoroutine(RoamAndAnimateRoutine());
        }

        private void UpdateEmoteIcon()
        {
            if (emoteRenderer == null) return;

            Sprite needSprite = currentNeed switch
            {
                ElephantNeed.Hambre => foodEmote,
                ElephantNeed.Sed => waterEmote,
                ElephantNeed.Sucio => dirtyEmote,
                ElephantNeed.Medicina => medicineEmote,
                _ => heartEmote
            };

            emoteRenderer.sprite = needSprite;
            emoteRenderer.enabled = (currentNeed != ElephantNeed.Ninguna);

            if (needSprite != null && emoteRenderer.enabled)
            {
                float maxDim = Mathf.Max(needSprite.bounds.size.x, needSprite.bounds.size.y);
                float targetSize = 0.55f;
                float scale = (maxDim > 0.001f) ? (targetSize / maxDim) : 1f;
                emoteRenderer.transform.localScale = Vector3.one * scale;
            }
        }

        private IEnumerator RoamAndAnimateRoutine()
        {
            while (true)
            {
                if (!isInteracting)
                {
                    if (idleSprite != null && spriteRenderer != null) spriteRenderer.sprite = idleSprite;
                    yield return new WaitForSeconds(Random.Range(2.5f, 6f));

                    targetPosition = new Vector2(
                        Random.Range(roamAreaMin.x, roamAreaMax.x),
                        Random.Range(roamAreaMin.y, roamAreaMax.y)
                    );

                    float timer = 0f;
                    float maxDuration = Random.Range(2f, 4f);
                    int animStep = 0;
                    float stepTimer = 0f;

                    while (timer < maxDuration && Vector2.Distance(transform.position, targetPosition) > 0.3f && !isInteracting)
                    {
                        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
                        transform.position += (Vector3)(dir * (moveSpeed * Time.deltaTime));

                        if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.05f)
                        {
                            spriteRenderer.flipX = dir.x < 0;
                        }

                        stepTimer += Time.deltaTime;
                        if (stepTimer >= 0.22f)
                        {
                            stepTimer = 0f;
                            animStep = (animStep + 1) % 4;
                            if (spriteRenderer != null)
                            {
                                spriteRenderer.sprite = (animStep % 2 == 0) ? idleSprite : (animStep == 1 ? walk1Sprite : walk2Sprite);
                            }
                        }

                        timer += Time.deltaTime;
                        yield return null;
                    }
                }
                yield return null;
            }
        }

        public void ApplyCare(CareToolType tool)
        {
            isInteracting = true;
            string feedbackMsg = "";

            switch (tool)
            {
                case CareToolType.Comida:
                    happiness = Mathf.Min(100, happiness + 30);
                    if (currentNeed == ElephantNeed.Hambre) currentNeed = ElephantNeed.Ninguna;
                    feedbackMsg = $"🌾 ¡Alimentaste a {elephantName}! (+30 Felicidad)";
                    break;

                case CareToolType.Agua:
                    happiness = Mathf.Min(100, happiness + 30);
                    if (currentNeed == ElephantNeed.Sed) currentNeed = ElephantNeed.Ninguna;
                    feedbackMsg = $"💧 ¡Le diste agua fresca a {elephantName}! (+30 Felicidad)";
                    break;

                case CareToolType.Escoba:
                    happiness = Mathf.Min(100, happiness + 25);
                    if (currentNeed == ElephantNeed.Sucio) currentNeed = ElephantNeed.Ninguna;
                    feedbackMsg = $"🧹 ¡Bañaste y limpiaste a {elephantName}! (+25 Felicidad)";
                    break;

                case CareToolType.Guante:
                    happiness = Mathf.Min(100, happiness + 20);
                    if (currentNeed == ElephantNeed.Cepillado) currentNeed = ElephantNeed.Ninguna;
                    feedbackMsg = $"🧤 ¡Acariciaste y cepillaste con cariño a {elephantName}! (+20 Felicidad)";
                    break;

                case CareToolType.Medicina:
                    happiness = Mathf.Min(100, happiness + 35);
                    if (currentNeed == ElephantNeed.Medicina) currentNeed = ElephantNeed.Ninguna;
                    feedbackMsg = $"💊 ¡Administraste medicina y vitaminas a {elephantName}! (+35 Felicidad)";
                    break;

                case CareToolType.Ninguna:
                default:
                    happiness = Mathf.Min(100, happiness + 10);
                    feedbackMsg = $"❤️ Acariciaste a {elephantName}. (Felicidad: {happiness}%)";
                    break;
            }

            UpdateEmoteIcon();
            StartCoroutine(CareReactionRoutine(feedbackMsg));
        }

        private IEnumerator CareReactionRoutine(string message)
        {
            if (ElephantCareManager.Instance != null)
            {
                ElephantCareManager.Instance.ShowActionFeedback(message);
                ElephantCareManager.Instance.SpawnFloatingEmote(transform.position + Vector3.up * 1.2f, heartEmote);
            }

            // Happy bounce
            Vector3 originalScale = transform.localScale;
            transform.localScale = originalScale * 1.22f;
            yield return new WaitForSeconds(0.18f);
            transform.localScale = originalScale;

            yield return new WaitForSeconds(0.3f);
            isInteracting = false;
        }

        private void OnMouseDown()
        {
            if (ElephantCareManager.Instance != null)
            {
                ElephantCareManager.Instance.OnElephantClicked(this);
            }
        }
    }
}
