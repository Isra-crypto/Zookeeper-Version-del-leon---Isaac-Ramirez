using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ZooGame.Care
{
    public class ElephantCareManager : MonoBehaviour
    {
        public static ElephantCareManager Instance { get; private set; }

        [Header("Active Tool")]
        [SerializeField] private CareToolType selectedTool = CareToolType.Ninguna;

        [Header("Scene Navigation")]
        [SerializeField] private string mainZooSceneName = "SampleScene";

        [Header("Floating Emote Prefab / Template")]
        [SerializeField] private GameObject floatingEmoteTemplate;

        private CareHUD careHUD;
        private List<InteractiveCareElephant> elephants = new List<InteractiveCareElephant>();

        public CareToolType SelectedTool => selectedTool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            careHUD = FindFirstObjectByType<CareHUD>();
            elephants.AddRange(FindObjectsByType<InteractiveCareElephant>(FindObjectsSortMode.None));
        }

        private void Start()
        {
            SelectTool(CareToolType.Comida); // Start with food selected by default
            ShowActionFeedback("🌿 <b>¡Bienvenido a la Sabana de Elefantes!</b> Haz clic con tus herramientas para cuidarlos.");
        }

        private void Update()
        {
            HandlePointerInput();
        }

        private void HandlePointerInput()
        {
            Vector2 screenPos = Vector2.zero;
            bool pointerTriggered = false;

            // 1. Touchscreen support (Mobile)
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
                pointerTriggered = true;
            }
            // 2. Mouse / Pointer click support
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPos = Mouse.current.position.ReadValue();
                pointerTriggered = true;
            }
            // 3. Generic pointer device fallback
            else if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                screenPos = Pointer.current.position.ReadValue();
                pointerTriggered = true;
            }

            if (!pointerTriggered) return;

            // 1. Direct UI RectTransform click handling (100% reliable for tools & return button)
            if (careHUD != null && careHUD.TryHandleUIScreenClick(screenPos))
            {
                return;
            }

            // 2. Ignore world tap if tapping on other UI elements
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                {
                    int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touchId))
                    {
                        return;
                    }
                }
            }

            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

            // Raycast in 2D world
            Collider2D hit = Physics2D.OverlapPoint(worldPos2D);
            if (hit != null)
            {
                InteractiveCareElephant elephant = hit.GetComponent<InteractiveCareElephant>();
                if (elephant == null) elephant = hit.GetComponentInParent<InteractiveCareElephant>();

                if (elephant != null)
                {
                    OnElephantClicked(elephant);
                    return;
                }

                CareTrough trough = hit.GetComponent<CareTrough>();
                if (trough == null) trough = hit.GetComponentInParent<CareTrough>();

                if (trough != null)
                {
                    OnTroughClicked(trough);
                    return;
                }
            }
        }

        public void SelectTool(CareToolType tool)
        {
            selectedTool = tool;
            if (careHUD != null)
            {
                careHUD.UpdateSelectedToolVisual(selectedTool);
            }

            string toolDesc = tool switch
            {
                CareToolType.Comida => "🌾 <b>Comida seleccionada</b>: Haz clic en un elefante o en el comedero.",
                CareToolType.Agua => "💧 <b>Cubeta de Agua seleccionada</b>: Haz clic en un elefante o en el bebedero.",
                CareToolType.Escoba => "🧹 <b>Escoba seleccionada</b>: Haz clic en un elefante sucio para bañarlo.",
                CareToolType.Guante => "🧤 <b>Guante seleccionado</b>: Haz clic para acariciar y cepillar.",
                CareToolType.Medicina => "💊 <b>Medicina seleccionada</b>: Haz clic para curar y dar vitaminas.",
                _ => "✋ <b>Manos libres</b>: Haz clic en cualquier elefante para ver sus detalles."
            };

            ShowActionFeedback(toolDesc);
        }

        public void OnElephantClicked(InteractiveCareElephant elephant)
        {
            if (elephant == null) return;
            elephant.ApplyCare(selectedTool);
            UpdateGlobalStats();
        }

        public void OnTroughClicked(CareTrough trough)
        {
            if (trough == null) return;
            trough.TryRefill(selectedTool);
        }

        public void ShowActionFeedback(string message)
        {
            if (careHUD != null)
            {
                careHUD.SetFeedbackText(message);
            }
        }

        public void SpawnFloatingEmote(Vector3 worldPos, Sprite emoteSprite)
        {
            if (emoteSprite == null) return;
            StartCoroutine(FloatingEmoteRoutine(worldPos, emoteSprite));
        }

        private IEnumerator FloatingEmoteRoutine(Vector3 startPos, Sprite emoteSprite)
        {
            GameObject obj = new GameObject("FloatingEmote");
            obj.transform.position = startPos;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = emoteSprite;
            sr.sortingOrder = 9500;

            float maxDim = Mathf.Max(emoteSprite.bounds.size.x, emoteSprite.bounds.size.y);
            float targetSize = 0.6f;
            float scale = (maxDim > 0.001f) ? (targetSize / maxDim) : 1f;
            obj.transform.localScale = Vector3.one * scale;

            float elapsed = 0f;
            float duration = 1.0f;
            Vector3 initialPos = startPos;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                obj.transform.position = initialPos + new Vector3(Mathf.Sin(progress * 4f) * 0.2f, progress * 1.2f, 0);
                sr.color = new Color(1f, 1f, 1f, 1f - progress);

                yield return null;
            }

            Destroy(obj);
        }

        public void UpdateGlobalStats()
        {
            if (careHUD == null) return;
            int totalHappiness = 0;
            foreach (var e in elephants)
            {
                if (e != null) totalHappiness += e.Happiness;
            }
            int avgHappiness = elephants.Count > 0 ? totalHappiness / elephants.Count : 100;
            careHUD.SetAverageHappiness(avgHappiness);
        }

        public void ReturnToMainZoo()
        {
            Debug.Log($"<b>[Zoo Game]</b> Regresando al mapa general del zoo ({mainZooSceneName})...");
            SceneManager.LoadScene(mainZooSceneName);
        }
    }
}
