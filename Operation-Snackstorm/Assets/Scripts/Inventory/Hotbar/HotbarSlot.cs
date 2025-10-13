using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HotbarSlot
{
    public UnityEngine.Object itemRef;
    public int amount;

    public bool IsEmpty => itemRef == null || amount <= 0;

    public IHotbarItem AsHotbarItem()
    {
        return itemRef as IHotbarItem;   // 기존 아이템이 IHotbarItem 구현하면 바로 캐스팅
    }
}
