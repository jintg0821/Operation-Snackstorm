using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();
    public List<ItemData> storeItems = new List<ItemData>();
    public List<ItemData> vendingItems = new List<ItemData>();
    public List<ItemData> commonItems = new List<ItemData>();

    private void Awake()
    {
        Instance = this;
        LoadCSV();
    }

    void LoadCSV()
    {
        TextAsset csvData = Resources.Load<TextAsset>("ItemData"); // Resources/ItemData.csv
        string[] lines = csvData.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 헤더 제외
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Split(',');

            ItemData item = new ItemData
            {
                id = row[0],               
                name = row[1],
                price = int.Parse(row[2]),
                category = row[3],
                point = int.Parse(row[4])
            };

            items[item.id] = item;

            switch (item.category)
            {
                case "매점": storeItems.Add(item); break;
                case "자판기": vendingItems.Add(item); break;
                case "공통": commonItems.Add(item); break;
            }
        }

        Debug.Log($"아이템 {items.Count}개 로드됨 (매점 {storeItems.Count}, 자판기 {vendingItems.Count}, 공통 {commonItems.Count})");
    }

    public ItemData GetItem(string id)
    {
        return items.ContainsKey(id) ? items[id] : null;
    }
}
