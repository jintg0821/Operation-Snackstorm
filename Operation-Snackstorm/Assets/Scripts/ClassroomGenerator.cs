using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassroomGenerator : MonoBehaviour
{
    public GameObject classroomPrefab;
    public List<GameObject> specialClassrooms;
    public List<Transform> classroomSlots;

    void Start()
    {
        Generate();
    }

    public void ReRoll()
    {
        Clear();
        Generate();
    }

    void Generate()
    {
        for (int i = classroomSlots.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (classroomSlots[i], classroomSlots[j]) = (classroomSlots[j], classroomSlots[i]);
        }

        // 특수 교실 배치
        int specialToPlace = Mathf.Min(specialClassrooms.Count, classroomSlots.Count);
        for (int i = 0; i < specialToPlace; i++)
            Spawn(specialClassrooms[i], classroomSlots[i]);

        // 나머지 슬롯을 일반 교실로 채움
        for (int i = specialToPlace; i < classroomSlots.Count; i++)
            Spawn(classroomPrefab, classroomSlots[i]);
    }

    void Spawn(GameObject prefab, Transform slot)
    {
        // 슬롯 아래 기존 오브젝트 제거
        for (int i = slot.childCount - 1; i >= 0; i--)
            Destroy(slot.GetChild(i).gameObject);

        Instantiate(prefab, slot.position, slot.rotation, slot);
    }

    void Clear()
    {
        foreach (Transform slot in classroomSlots)
        {
            for (int i = slot.childCount - 1; i >= 0; i--)
                Destroy(slot.GetChild(i).gameObject);
        }
    }
}
