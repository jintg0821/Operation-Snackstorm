using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PianoKeyButton : MonoBehaviour
{
    public NoteName note;
    private Image keyImage;

    // 7음계 고정 색상
    private static readonly Color[] noteColors = new Color[]
    {
        new Color(1f, 0.2f, 0.2f),   // 도 - 빨강
        new Color(1f, 0.5f, 0f),     // 레 - 주황
        new Color(1f, 0.9f, 0.2f),   // 미 - 노랑
        new Color(0.3f, 0.9f, 0.3f), // 파 - 초록
        new Color(0.3f, 0.7f, 1f),   // 솔 - 하늘파랑
        new Color(0.1f, 0.1f, 0.9f), // 라 - 남색
        new Color(0.8f, 0.3f, 0.9f)  // 티 - 보라
    };

    void Awake()
    {
        keyImage = GetComponent<Image>();

        if (note == NoteName.Do)  
        {
            string n = gameObject.name.ToLower();
            if (n.Contains("do") || n.Contains("c")) note = NoteName.Do;
            else if (n.Contains("re") || n.Contains("d")) note = NoteName.Re;
            else if (n.Contains("mi") || n.Contains("e")) note = NoteName.Mi;
            else if (n.Contains("fa") || n.Contains("f")) note = NoteName.Fa;
            else if (n.Contains("sol") || n.Contains("g")) note = NoteName.Sol;
            else if (n.Contains("la") || n.Contains("a")) note = NoteName.La;
            else if (n.Contains("ti") || n.Contains("b")) note = NoteName.Ti;
        }

        // 색상 바로 적용
        if (keyImage != null)
            keyImage.color = noteColors[(int)note];
    }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            PianoMinigameManager.Instance?.OnKeyPressed(note);
        });
    }

    public void OnPointerDown(PointerEventData e) => keyImage.color = Color.Lerp(noteColors[(int)note], Color.white, 0.3f);
    public void OnPointerUp(PointerEventData e)   => keyImage.color = noteColors[(int)note];
}

