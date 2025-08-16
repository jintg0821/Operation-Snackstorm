using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{
    public string id;
    public string name;
    public int price;
    public string category; // 매점 / 자판기 / 공통
    public int point;
    public Sprite icon;

    public GameObject prefab; // 3D 모델 Prefab 연결
}
