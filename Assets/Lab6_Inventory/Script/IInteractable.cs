using UnityEngine;

// Всё, с чем можно взаимодействовать: предметы, двери, рычаги...
public interface IInteractable
{
    void Interact(GameObject actor);
}
