using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Слот экипировки на экране. Клик — снять предмет обратно в инвентарь.
public class EquipSlotView : MonoBehaviour, IPointerClickHandler
{
    public EquipSlotType slotType;
    public Image background;
    public Image icon;

    [HideInInspector] public PlayerInventory player;

    public void Show(ItemData item, Color emptyColor, Color equippedColor)
    {
        icon.enabled = item != null;
        if (item != null) icon.sprite = item.icon;
        background.color = item != null ? equippedColor : emptyColor;   // подсветка «надето»
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (player != null) player.Unequip(slotType);
    }
}
