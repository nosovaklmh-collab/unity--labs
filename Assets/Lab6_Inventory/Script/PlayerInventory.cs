using UnityEngine;

// Мост между игроком и моделями Inventory / Equipment.
// Вешается на Player вместе с CharacterController и скриптом движения.
public class PlayerInventory : MonoBehaviour
{
    public Inventory Inventory { get; private set; } = new Inventory(20);
    public Equipment Equipment { get; private set; } = new Equipment();

    [Header("Стрельба (доп. задание 1)")]
    public ItemData arrowItem;                 // ассет Arrow
    public KeyCode shootKey = KeyCode.F;

    void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        interactable?.Interact(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(shootKey)) Shoot();
    }

    // Клик по слоту: экипируемое — надеть, лечащее — использовать
    public void UseOrEquip(int index)
    {
        if (index < 0 || index >= Inventory.SlotCount) return;
        var slot = Inventory.Slots[index];
        if (slot.IsEmpty) return;
        var item = slot.item;

        if (item.IsEquippable)
        {
            Inventory.RemoveAt(index, 1);                          // из сумки…
            Equipment.Equip(item, out ItemData previous);           // …на персонажа
            if (previous != null) Inventory.AddItem(previous, 1);   // старый — обратно в сумку
            Debug.Log($"Надето: {item.itemName}");
        }
        else if (item.healAmount > 0)
        {
            UseItem(index);
        }
    }

    // Доп. задание 1: использование. Зелье лечит, если у игрока есть PlayerHealth (из ЛР6).
    public void UseItem(int index)
    {
        var item = Inventory.Slots[index].item;
        var health = GetComponent<PlayerHealth>();

        if (health != null)
        {
            if (!health.Heal(item.healAmount)) { Debug.Log("Здоровье и так полное"); return; }
        }
        Inventory.RemoveAt(index, 1);
        Debug.Log($"Использовано: {item.itemName} (+{item.healAmount} HP)");
    }

    // Снять предмет обратно в инвентарь
    public void Unequip(EquipSlotType slotType)
    {
        var item = Equipment.Get(slotType);
        if (item == null) return;

        if (Inventory.FreeSpaceFor(item) < 1) { Debug.Log("Нет места в инвентаре"); return; }
        Equipment.Unequip(slotType);
        Inventory.AddItem(item, 1);
        Debug.Log($"Снято: {item.itemName}");
    }

    // Доп. задание 1: выстрел тратит одну стрелу (нужен надетый лук или меч — для простоты любое оружие)
    void Shoot()
    {
        if (arrowItem == null) return;
        if (Inventory.RemoveItem(arrowItem, 1))
            Debug.Log($"Выстрел! Осталось стрел: {Inventory.CountOf(arrowItem)}");
        else
            Debug.Log("Стрелы закончились");
    }
}
