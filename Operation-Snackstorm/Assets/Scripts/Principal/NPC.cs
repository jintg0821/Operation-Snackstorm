using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public string displayName;
    private Coroutine aiRoutine;
    private bool isPaused = false;

    public void PauseAI(bool pause)
    {
        if (isPaused == pause) return;
        isPaused = pause;

        if (pause)
        {
            if (aiRoutine != null) StopCoroutine(aiRoutine);
            GetComponent<Animator>()?.CrossFade("Idle", 0.1f);
        }
        else
        {
            aiRoutine = StartCoroutine(AIRoutine());
        }
    }

    private IEnumerator AIRoutine()
    {
        // 아무것도 안 함 (테스트용)
        yield return null;
    }
}
