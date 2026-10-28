using UnityEngine;

// UI экипировки: три слота (голова, оружие, броня) с подсветкой надетого.
public class EquipmentUI : MonoBehaviour
{
    public PlayerInventory player;
    public EquipSlotView[] slotViews;
    public Color emptyColor = new Color(0.2f, 0.2f, 0.22f, 0.9f);
    public Color equippedColor = new Color(0.85f, 0.65f, 0.15f, 0.95f);   // золотая подсветка

    void Start()
    {
        foreach (var v in slotViews) v.player = player;
        player.Equipment.OnEquipmentChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (player != null) player.Equipment.OnEquipmentChanged -= Refresh;
    }

    void Refresh()
    {
        foreach (var v in slotViews)
            v.Show(player.Equipment.Get(v.slotType), emptyColor, equippedColor);
    }
}
