using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroscopeTrigger : MonoBehaviour
{
    public GameObject playerA; // 플레이어 A 오브젝트
    public GameObject playerB; // 플레이어 B 오브젝트
    public float triggerDistance = 2f; // 트리거 거리
    private bool miniGameActive = false;

    void Update()
    {
        if (!miniGameActive && Vector3.Distance(playerA.transform.position, transform.position) < triggerDistance &&
            Vector3.Distance(playerB.transform.position, transform.position) < triggerDistance)
        {
            // 입력으로 확인
            if (Input.GetKeyDown(KeyCode.E)) 
            {
                StartMiniGame();
            }
        }
    }

    void StartMiniGame()
    {
        miniGameActive = true;
    }
}
