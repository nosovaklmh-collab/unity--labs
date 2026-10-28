using UnityEngine;

// Предмет на полу. При касании игроком пытается лечь в его инвентарь.
public class PickupItem : MonoBehaviour, IInteractable
{
    public ItemData item;
    public int amount = 1;
    public float rotateSpeed = 60f;   // медленно крутится, чтобы было заметно

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
    }

    public void Interact(GameObject actor)
    {
        var inventory = actor.GetComponent<PlayerInventory>();
        if (inventory == null || item == null) return;

        if (inventory.Inventory.AddItem(item, amount))
        {
            Debug.Log($"Подобрано: {item.itemName} x{amount}");
            Destroy(gameObject);    // уничтожаем только после успешного добавления
        }
        else
        {
            Debug.Log($"Не хватает места для: {item.itemName} x{amount}");
        }
    }
}
