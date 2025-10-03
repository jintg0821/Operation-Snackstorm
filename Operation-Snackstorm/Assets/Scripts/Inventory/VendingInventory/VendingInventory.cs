using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VendingItem
{
    public Item prefab;      // ScriptableObject 아이템
}

public class VendingInventory : MonoBehaviour
{
    [Header("자판기에서 판매할 아이템들")]
    public VendingItem[] items;
}
