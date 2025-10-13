using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHotbarItem
{
    UnityEngine.Sprite GetIcon();        // UI 아이콘
    bool IsStackable();                  // 스택 여부
    int MaxStack();                     // 최대 스택
    // 필요하면 사용/드랍 등도 추후 확장 가능
}
