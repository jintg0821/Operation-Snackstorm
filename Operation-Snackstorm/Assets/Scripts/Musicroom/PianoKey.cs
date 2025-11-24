using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PianoKey : MonoBehaviour
{
    public NoteType noteType;
    public PianoGameManager gameManager;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClickKey);
    }

    void OnClickKey()
    {
        if (gameManager != null)
        {
            gameManager.OnKeyPressed(noteType);
        }
        else
        {
            Debug.LogWarning("PianoKey에 gameManager가 할당 안됨");
        }
    }
}
