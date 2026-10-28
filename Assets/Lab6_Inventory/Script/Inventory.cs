using System;
using System.Collections.Generic;
using UnityEngine;

// Инвентарь — чистая логика, не MonoBehaviour: можно тестировать без сцены.
// Об изменениях сообщает событием OnInventoryChanged — UI на него подписан.
public class Inventory
{
    public event Action OnInventoryChanged;

    private readonly List<InventorySlot> slots;

    public Inventory(int capacity = 20)
    {
        slots = new List<InventorySlot>(capacity);
        for (int i = 0; i < capacity; i++)
            slots.Add(new InventorySlot());
    }

    public int SlotCount => slots.Count;
    public IReadOnlyList<InventorySlot> Slots => slots;

    private void NotifyChanged() => OnInventoryChanged?.Invoke();

    // Сколько всего штук этого предмета в инвентаре
    public int CountOf(ItemData item)
    {
        int total = 0;
        foreach (var s in slots) if (s.item == item) total += s.count;
        return total;
    }

    // Сколько штук этого предмета ещё влезет
    public int FreeSpaceFor(ItemData item)
    {
        int space = 0;
        foreach (var s in slots)
        {
            if (s.IsEmpty) space += item.maxStack;
            else if (s.CanStack(item)) space += item.maxStack - s.count;
        }
        return space;
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        // Не влезает целиком — ничего не добавляем (предмет остаётся на полу)
        if (FreeSpaceFor(item) < amount) return false;

        // 1. Сначала докладываем в уже начатые стаки этого предмета
        foreach (var slot in slots)
        {
            if (slot.IsEmpty || !slot.CanStack(item)) continue;

            int toAdd = Mathf.Min(item.maxStack - slot.count, amount);
            slot.count += toAdd;
            amount -= toAdd;
            if (amount <= 0) break;
        }

        // 2. Остаток — в пустые слоты
        if (amount > 0)
        {
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty) continue;

                int toAdd = Mathf.Min(item.maxStack, amount);
                slot.item = item;
                slot.count = toAdd;
                amount -= toAdd;
                if (amount <= 0) break;
            }
        }

        NotifyChanged();
        return true;
    }

    public bool RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;
        if (CountOf(item) < amount) return false;   // столько нет — ничего не удаляем

        foreach (var slot in slots)
        {
            if (slot.item != item) continue;

            int toRemove = Mathf.Min(slot.count, amount);
            slot.count -= toRemove;
            amount -= toRemove;

            if (slot.count == 0) slot.Clear();
            if (amount <= 0) break;
        }

        NotifyChanged();
        return true;
    }

    // Удалить из конкретного слота
    public bool RemoveAt(int index, int amount = 1)
    {
        if (index < 0 || index >= slots.Count) return false;
        var slot = slots[index];
        if (slot.IsEmpty || slot.count < amount) return false;

        slot.count -= amount;
        if (slot.count == 0) slot.Clear();
        NotifyChanged();
        return true;
    }

    // Разделить стак: половину (или amount штук) перенести в первый пустой слот
    public bool SplitStack(int index, int amount = -1)
    {
        if (index < 0 || index >= slots.Count) return false;
        var source = slots[index];
        if (source.IsEmpty || source.count < 2) return false;

        if (amount <= 0) amount = source.count / 2;
        if (amount >= source.count) return false;

        var target = slots.Find(s => s.IsEmpty);
        if (target == null) return false;   // нет свободного места

        target.item = source.item;
        target.count = amount;
        source.count -= amount;
        NotifyChanged();
        return true;
    }

    // ---- для сохранения/загрузки (доп. задание 3) ----
    public void Clear()
    {
        foreach (var s in slots) s.Clear();
        NotifyChanged();
    }

    public void SetSlot(int index, ItemData item, int count)
    {
        if (index < 0 || index >= slots.Count) return;
        slots[index].item = item;
        slots[index].count = item == null ? 0 : count;
    }

    public void ForceRefresh() => NotifyChanged();
}
