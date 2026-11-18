using UnityEngine;

namespace ZooGame.Core
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(GameObject interactor);
    }
}
