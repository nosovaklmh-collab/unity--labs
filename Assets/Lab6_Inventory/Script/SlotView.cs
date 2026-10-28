using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Одна ячейка инвентаря на экране. ЛКМ — использовать/надеть, ПКМ — разделить стак.
public class SlotView : MonoBehaviour, IPointerClickHandler
{
    public Image background;
    public Image icon;
    public Text countText;

    private InventoryUI owner;
    private int index;

    public void Setup(InventoryUI owner, int index, InventorySlot slot)
    {
        this.owner = owner;
        this.index = index;

        if (slot.IsEmpty)
        {
            icon.enabled = false;
            countText.text = "";
        }
        else
        {
            icon.enabled = true;
            icon.sprite = slot.item.icon;
            countText.text = slot.count > 1 ? slot.count.ToString() : "";
        }
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (owner == null) return;
        if (e.button == PointerEventData.InputButton.Left) owner.OnSlotLeftClick(index);
        if (e.button == PointerEventData.InputButton.Right) owner.OnSlotRightClick(index);
    }
}
