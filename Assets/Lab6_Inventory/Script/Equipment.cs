using System;
using System.Collections.Generic;

public enum EquipSlotType { None, Head, Weapon, Armor }

// Экипировка — отдельная модель: «что надето», а не «что в сумке».
// В каждом слоте может быть только один предмет.
public class Equipment
{
    public event Action OnEquipmentChanged;

    private readonly Dictionary<EquipSlotType, ItemData> slots = new Dictionary<EquipSlotType, ItemData>();

    public IReadOnlyDictionary<EquipSlotType, ItemData> Slots => slots;

    public ItemData Get(EquipSlotType slot) =>
        slots.TryGetValue(slot, out var item) ? item : null;

    // Надеть. В previous возвращается то, что было надето раньше (вернём в инвентарь).
    public bool Equip(ItemData item, out ItemData previous)
    {
        previous = null;
        if (item == null || !item.IsEquippable) return false;   // зелье не наденешь

        previous = Get(item.equipSlot);
        slots[item.equipSlot] = item;
        OnEquipmentChanged?.Invoke();
        return true;
    }

    // Снять. Возвращает снятый предмет или null.
    public ItemData Unequip(EquipSlotType slot)
    {
        var item = Get(slot);
        if (item == null) return null;
        slots.Remove(slot);
        OnEquipmentChanged?.Invoke();
        return item;
    }

    public void Clear()
    {
        slots.Clear();
        OnEquipmentChanged?.Invoke();
    }
}
