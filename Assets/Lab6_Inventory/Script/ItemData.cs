using UnityEngine;

// Данные предмета (data-driven): каждый предмет — отдельный ассет.
// Создание: Assets → Create → Data → Item
[CreateAssetMenu(menuName = "Data/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable;
    public int maxStack = 1;
    public EquipSlotType equipSlot = EquipSlotType.None;   // None — не экипируется

    [Header("Использование (доп. задание 1)")]
    public int healAmount = 0;                              // > 0 — предмет лечит (зелье)

    public bool IsEquippable => equipSlot != EquipSlotType.None;
}
