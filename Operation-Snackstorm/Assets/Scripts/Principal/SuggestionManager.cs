using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuggestionManager : MonoBehaviour
{
    public static SuggestionManager Instance;
    private List<Suggestion> logs = new List<Suggestion>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Submit(NPC target, string content)
    {
        logs.Add(new Suggestion
        {
            target = target,
            content = content,
            timestamp = Time.time
        });

        // 5분 후 NPC 복구
        StartCoroutine(AutoResumeAfterDelay(target, 300f));
    }

    private IEnumerator AutoResumeAfterDelay(NPC npc, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (npc != null) npc.PauseAI(false);
    }
}

[System.Serializable]
public class Suggestion
{
    public NPC target;
    public string content;
    public float timestamp;
}
