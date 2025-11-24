using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum NoteType
{
    Do,
    Re,
    Mi,
    Fa,
    Sol,
    La,
    Ti   // 시
}

public class PianoGameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI noteText;   // 칠판에 계이름 보여줄 텍스트
    public Image colorImage;           // 칠판 색깔 박스

    [Header("설정")]
    public bool useNoteName = true;    // true면 "도/레/미" 읽어주기, false면 색깔만 보고 맞추기

    [Header("현재 문제 정보 (디버그용)")]
    public NoteType currentNote;

    // 색 매핑용 딕셔너리 (선택)
    Dictionary<NoteType, Color> noteColorMap;

    void Awake()
    {
        InitColorMap();
    }

    void Start()
    {
        CreateNewQuestion();
    }

    void InitColorMap()
    {
        noteColorMap = new Dictionary<NoteType, Color>();

        // 원하는 색으로 바꿔도 됨
        noteColorMap[NoteType.Do] = Color.red;                // 빨강
        noteColorMap[NoteType.Re] = new Color(1f, 0.5f, 0f);  // 주황
        noteColorMap[NoteType.Mi] = Color.yellow;
        noteColorMap[NoteType.Fa] = Color.green;
        noteColorMap[NoteType.Sol] = Color.cyan;
        noteColorMap[NoteType.La] = Color.blue;
        noteColorMap[NoteType.Ti] = new Color(0.6f, 0f, 1f);  // 보라
    }

    // 새 문제 생성
    public void CreateNewQuestion()
    {
        // 0~6 랜덤
        int randomIndex = Random.Range(0, 7);
        currentNote = (NoteType)randomIndex;

        UpdateBlackboardUI();
    }

    void UpdateBlackboardUI()
    {
        // 계이름 텍스트 갱신
        if (noteText != null)
        {
            if (useNoteName)
            {
                noteText.text = GetKoreanName(currentNote); // 도/레/미...
            }
            else
            {
                // 색깔만 보여주고 텍스트는 비우거나 "색을 맞춰보세요"
                noteText.text = "색을 맞춰보세요";
            }
        }

        // 색 박스 갱신
        if (colorImage != null && noteColorMap != null)
        {
            colorImage.color = noteColorMap[currentNote];
        }
    }

    string GetKoreanName(NoteType note)
    {
        switch (note)
        {
            case NoteType.Do: return "도";
            case NoteType.Re: return "레";
            case NoteType.Mi: return "미";
            case NoteType.Fa: return "파";
            case NoteType.Sol: return "솔";
            case NoteType.La: return "라";
            case NoteType.Ti: return "시";
        }
        return "";
    }

    // 버튼에서 호출할 함수
    public void OnKeyPressed(NoteType pressedNote)
    {
        if (pressedNote == currentNote)
        {
            Debug.Log("정답! " + GetKoreanName(pressedNote));
            // TODO: 정답 표시 UI, 효과음 등
        }
        else
        {
            Debug.Log("오답! " + GetKoreanName(pressedNote) + " 를 눌렀습니다.");
            // TODO: 오답 표시 UI
        }

        // 다음 문제
        CreateNewQuestion();
    }
}
