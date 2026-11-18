using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZooGame.Care
{
    [RequireComponent(typeof(Button))]
    public class ToolSlotButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CareToolType toolType = CareToolType.Comida;

        public CareToolType ToolType
        {
            get => toolType;
            set => toolType = value;
        }

        private void Awake()
        {
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnSlotClicked);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked();
        }

        public void OnSlotClicked()
        {
            if (ElephantCareManager.Instance != null)
            {
                ElephantCareManager.Instance.SelectTool(toolType);
            }
        }
    }
}
