using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string id;
    public string name;
    public int price;
    public string category; // ���� / ���Ǳ� / ����
    public int point;
    public Sprite icon;

    public GameObject prefab; // 3D �� Prefab ����
}
