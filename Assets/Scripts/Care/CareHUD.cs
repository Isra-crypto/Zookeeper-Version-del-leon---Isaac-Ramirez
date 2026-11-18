using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZooGame.Care
{
    public class CareHUD : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text feedbackText;
        [SerializeField] private Text happinessText;
        [SerializeField] private Button returnButton;
        [SerializeField] private List<Image> slotBorders = new List<Image>();

        private readonly Color activeSlotColor = new Color(1f, 0.85f, 0.2f, 1f);
        private readonly Color inactiveSlotColor = new Color(0.35f, 0.25f, 0.15f, 0.8f);

        private List<RectTransform> slotRects = new List<RectTransform>();
        private List<CareToolType> slotTools = new List<CareToolType>();
        private RectTransform returnBtnRect;

        private void Awake()
        {
            EnsureEventSystemExists();
            AutoSetupSlotsAndButtons();
        }

        private void EnsureEventSystemExists()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private void AutoSetupSlotsAndButtons()
        {
            slotRects.Clear();
            slotTools.Clear();
            slotBorders.Clear();

            // Find Return Button
            if (returnButton == null)
            {
                var btnObj = GameObject.Find("Btn_Return");
                if (btnObj != null) returnButton = btnObj.GetComponent<Button>();
            }

            if (returnButton != null)
            {
                returnBtnRect = returnButton.GetComponent<RectTransform>();
                returnButton.onClick.RemoveAllListeners();
                returnButton.onClick.AddListener(() =>
                {
                    if (ElephantCareManager.Instance != null)
                    {
                        ElephantCareManager.Instance.ReturnToMainZoo();
                    }
                });
            }

            // Find Hotbar and slots
            Transform hotbar = transform.Find("Care_Hotbar") ?? GameObject.Find("Care_Hotbar")?.transform;
            if (hotbar != null)
            {
                CareToolType[] toolTypes = { CareToolType.Comida, CareToolType.Agua, CareToolType.Escoba, CareToolType.Guante, CareToolType.Medicina };

                for (int i = 0; i < hotbar.childCount && i < toolTypes.Length; i++)
                {
                    Transform slotTr = hotbar.GetChild(i);
                    CareToolType tType = toolTypes[i];
                    RectTransform rt = slotTr.GetComponent<RectTransform>();
                    Image bg = slotTr.GetComponent<Image>();

                    if (bg != null)
                    {
                        bg.raycastTarget = true;
                        slotBorders.Add(bg);
                    }

                    if (rt != null)
                    {
                        slotRects.Add(rt);
                        slotTools.Add(tType);
                    }

                    // Add / configure Button and ToolSlotButton
                    Button btn = slotTr.GetComponent<Button>();
                    if (btn == null) btn = slotTr.gameObject.AddComponent<Button>();

                    ToolSlotButton tsb = slotTr.GetComponent<ToolSlotButton>();
                    if (tsb == null) tsb = slotTr.gameObject.AddComponent<ToolSlotButton>();
                    tsb.ToolType = tType;
                }
            }
        }

        public bool TryHandleUIScreenClick(Vector2 screenPos)
        {
            // 1. Check Return Button
            if (returnBtnRect != null && RectTransformUtility.RectangleContainsScreenPoint(returnBtnRect, screenPos, null))
            {
                if (ElephantCareManager.Instance != null)
                {
                    ElephantCareManager.Instance.ReturnToMainZoo();
                }
                return true;
            }

            // 2. Check Tool Slots
            for (int i = 0; i < slotRects.Count; i++)
            {
                if (slotRects[i] != null && RectTransformUtility.RectangleContainsScreenPoint(slotRects[i], screenPos, null))
                {
                    if (ElephantCareManager.Instance != null)
                    {
                        ElephantCareManager.Instance.SelectTool(slotTools[i]);
                    }
                    return true;
                }
            }

            return false;
        }

        public void SetFeedbackText(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
            }
        }

        public void SetAverageHappiness(int avg)
        {
            if (happinessText != null)
            {
                happinessText.text = $"❤️ Bienestar Familiar: <b>{avg}%</b>";
            }
        }

        public void UpdateSelectedToolVisual(CareToolType tool)
        {
            int selectedIndex = (int)tool - 1; // Tool 1 -> index 0, Tool 5 -> index 4

            for (int i = 0; i < slotBorders.Count; i++)
            {
                if (slotBorders[i] != null)
                {
                    bool isCurrent = (i == selectedIndex);
                    slotBorders[i].color = isCurrent ? activeSlotColor : inactiveSlotColor;
                    slotBorders[i].transform.localScale = isCurrent ? Vector3.one * 1.15f : Vector3.one;
                }
            }
        }
    }
}
