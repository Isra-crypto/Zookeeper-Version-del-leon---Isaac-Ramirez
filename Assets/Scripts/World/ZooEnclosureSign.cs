using UnityEngine;
using UnityEngine.SceneManagement;
using ZooGame.Core;

namespace ZooGame.World
{
    public class ZooEnclosureSign : MonoBehaviour, IInteractable
    {
        [Header("Sign Info")]
        [SerializeField] private string enclosureName = "Sabana de Elefantes 🐘";
        [TextArea(2, 4)]
        [SerializeField] private string description = "Familia de elefantes africanos.";

        [Header("Scene Transition")]
        [SerializeField] private bool loadsScene = true;
        [SerializeField] private string targetSceneName = "ElephantCareScene";

        public string TargetSceneName
        {
            get => targetSceneName;
            set => targetSceneName = value;
        }

        public bool LoadsScene
        {
            get => loadsScene;
            set => loadsScene = value;
        }

        public string GetInteractionPrompt()
        {
            if (loadsScene && !string.IsNullOrEmpty(targetSceneName))
            {
                return $"[E] Entrar a cuidar: {enclosureName}";
            }
            return $"[E] Leer cartel: {enclosureName}";
        }

        public void Interact(GameObject interactor)
        {
            if (loadsScene && !string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log($"<b>[Zoo Game]</b> Viajando a: <color=#FFA500>{targetSceneName}</color> ({enclosureName})...");
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.Log($"<b>[Cartel del Zoo]</b> <color=#FFA500>{enclosureName}</color>: {description}");
            }
        }
    }
}
