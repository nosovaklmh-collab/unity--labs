using System;

// Один слот инвентаря: какой предмет и сколько штук
[Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    public bool IsEmpty => item == null;

    // Можно ли доложить такой же предмет в этот (непустой) слот
    public bool CanStack(ItemData newItem)
    {
        return item == newItem          // сравнение по ссылке на ассет, а не по имени
            && item.isStackable
            && count < item.maxStack;
    }

    public void Clear()
    {
        item = null;
        count = 0;
    }
}
