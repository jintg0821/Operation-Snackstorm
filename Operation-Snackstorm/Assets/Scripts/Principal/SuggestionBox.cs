using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuggestionBox : MonoBehaviour, IInteractable
{
    [SerializeField] private List<NPC> targetNPCs; // Inspector에서 할당 (선생님 1명 + 선배 18명)

    public void Interact()
    {
        SuggestionUI.Instance.Open(this, targetNPCs);
    }
}
