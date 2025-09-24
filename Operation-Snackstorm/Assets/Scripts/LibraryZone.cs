using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryZone : MonoBehaviour
{
    // 플레이어가 도서관 영역에 들어왔을 때 호출
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 오브젝트가 Player 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // PlayerController에게 도서관에 들어왔다고 알림
                player.EnterLibraryZone(true);
            }
        }
    }

    // 플레이어가 도서관 영역에서 나갔을 때 호출
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // PlayerController에게 도서관에서 나갔다고 알려줌
                player.EnterLibraryZone(false);
            }
        }
    }
}