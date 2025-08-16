using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InventorySlotUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;   // TMP Text
    public Transform modelParent; // 3D 모델이 들어갈 슬롯 위치
    private GameObject currentModel;

    // itemPrefab은 Inspector에서 직접 연결
    public void Setup(ItemData item)
    {
        nameText.text = item.name;

        if (currentModel != null) Destroy(currentModel);

        if (item.prefab != null)
        {
            currentModel = Instantiate(item.prefab, modelParent);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
            currentModel.transform.localScale = Vector3.one;
        }
    }

    public void Clear()
    {
        nameText.text = "";
        if (currentModel != null) Destroy(currentModel);
    }
}
