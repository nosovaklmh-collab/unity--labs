using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Доп. задание 3: сохранение инвентаря и экипировки в JSON.
// Сохраняются не ссылки на ассеты, а их ИМЕНА; при загрузке ассет ищется в списке allItems.
// F5 — сохранить, F9 — загрузить.
public class InventorySaver : MonoBehaviour
{
    public PlayerInventory player;
    public List<ItemData> allItems;          // все ассеты предметов (перетащить сюда)

    [Serializable] class SlotSave { public string item; public int count; }
    [Serializable] class SaveData
    {
        public List<SlotSave> slots = new List<SlotSave>();
        public List<string> equipped = new List<string>();
    }

    string PathToFile => Path.Combine(Application.persistentDataPath, "inventory.json");

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)) Save();
        if (Input.GetKeyDown(KeyCode.F9)) Load();
    }

    public void Save()
    {
        var data = new SaveData();
        foreach (var s in player.Inventory.Slots)
            data.slots.Add(new SlotSave { item = s.IsEmpty ? "" : s.item.name, count = s.count });
        foreach (var kv in player.Equipment.Slots)
            data.equipped.Add(kv.Value.name);

        File.WriteAllText(PathToFile, JsonUtility.ToJson(data, true));
        Debug.Log("Инвентарь сохранён: " + PathToFile);
    }

    public void Load()
    {
        if (!File.Exists(PathToFile)) { Debug.Log("Сохранения нет"); return; }
        var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathToFile));

        for (int i = 0; i < player.Inventory.SlotCount; i++)
        {
            var s = i < data.slots.Count ? data.slots[i] : null;
            player.Inventory.SetSlot(i, s == null ? null : Find(s.item), s == null ? 0 : s.count);
        }
        player.Inventory.ForceRefresh();

        player.Equipment.Clear();
        foreach (var name in data.equipped)
        {
            var item = Find(name);
            if (item != null) player.Equipment.Equip(item, out _);
        }
        Debug.Log("Инвентарь загружен");
    }

    ItemData Find(string assetName)
    {
        if (string.IsNullOrEmpty(assetName)) return null;
        return allItems.Find(i => i != null && i.name == assetName);
    }
}
