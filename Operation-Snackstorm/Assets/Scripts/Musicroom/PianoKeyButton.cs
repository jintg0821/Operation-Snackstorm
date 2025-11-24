using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PianoKeyButton : MonoBehaviour
{
    public NoteName note;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (PianoMinigameManager.Instance != null)
        {
            PianoMinigameManager.Instance.OnKeyPressed(note);
        }
    }
}
