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

        // Ư�� ���� ��ġ
        int specialToPlace = Mathf.Min(specialClassrooms.Count, classroomSlots.Count);
        for (int i = 0; i < specialToPlace; i++)
            Spawn(specialClassrooms[i], classroomSlots[i]);

        // ������ ������ �Ϲ� ���Ƿ� ä��
        for (int i = specialToPlace; i < classroomSlots.Count; i++)
            Spawn(classroomPrefab, classroomSlots[i]);
    }

    void Spawn(GameObject prefab, Transform slot)
    {
        // ���� �Ʒ� ���� ������Ʈ ����
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
