using UnityEngine;

// UI инвентаря. Перерисовывается ТОЛЬКО по событию OnInventoryChanged, а не в Update.
public class InventoryUI : MonoBehaviour
{
    public PlayerInventory player;
    public Transform slotsParent;     // объект с Grid Layout Group
    public GameObject slotPrefab;     // префаб ячейки со SlotView

    [Header("Показать/скрыть")]
    public GameObject[] panels;       // панели инвентаря и экипировки
    public KeyCode toggleKey = KeyCode.I;

    void Start()
    {
        // Подписка в Start: к этому моменту инвентарь игрока уже создан
        player.Inventory.OnInventoryChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (player != null) player.Inventory.OnInventoryChanged -= Refresh;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            foreach (var p in panels) if (p != null) p.SetActive(!p.activeSelf);
    }

    void Refresh()
    {
        foreach (Transform child in slotsParent)
            Destroy(child.gameObject);

        for (int i = 0; i < player.Inventory.SlotCount; i++)
        {
            var go = Instantiate(slotPrefab, slotsParent);
            go.GetComponent<SlotView>().Setup(this, i, player.Inventory.Slots[i]);
        }
    }

    public void OnSlotLeftClick(int index)  => player.UseOrEquip(index);
    public void OnSlotRightClick(int index) => player.Inventory.SplitStack(index);
}
