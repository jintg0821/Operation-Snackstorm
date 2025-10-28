using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class MicroscopeMiniGame : MonoBehaviour
{
    [Header("=== 초점 설정 ===")]
    public Transform focusTarget;           // 목표 위치 (숨겨진 간식)
    public Vector2 currentFocus = Vector2.zero;
    public float moveSpeed = 5f;
    public float focusThreshold = 0.15f;
    public float timeLimit = 30f;

    [Header("=== UI ===")]
    public GameObject uiRoot;               // MicroscopeUI 전체
    public RectTransform crosshair;         // FocusCrosshair
    public Image resultImage;               // 엉뚱한 사진
    public TextMeshProUGUI timerText;
    public Sprite[] funnySprites;           // 개구리 발, 고양이 등

    [Header("=== 입력 ===")]
    public InputActionAsset actions;
    private InputAction horizontalA, verticalB;

    [Header("=== 외부 매니저 ===")]
    public GameManager gameManager;
    

    // 플레이어 컨트롤러 비활성화용
    private MonoBehaviour playerCtrlA, playerCtrlB;

    private void OnEnable()
    {
        var map = actions.FindActionMap("MiniGame");
        horizontalA = map.FindAction("Horizontal");
        verticalB = map.FindAction("Vertical");
        horizontalA.Enable();
        verticalB.Enable();

        // UI 초기화
        uiRoot.SetActive(true);
        resultImage.gameObject.SetActive(false);
        currentFocus = Vector2.zero;
        UpdateCrosshair();
        StartCoroutine(TimerRoutine());
    }

    private void OnDisable()
    {
        horizontalA.Disable();
        verticalB.Disable();
    }

    private void Update()
    {
        float h = horizontalA.ReadValue<float>();
        float v = verticalB.ReadValue<float>();
        currentFocus += new Vector2(h, v) * moveSpeed * Time.deltaTime;

        UpdateCrosshair();

        // 성공 체크
        Vector2 target2D = new Vector2(focusTarget.localPosition.x, focusTarget.localPosition.y);
        if (Vector2.Distance(currentFocus, target2D) < focusThreshold)
        {
            Success();
        }
    }

    void UpdateCrosshair()
    {
        // 화면 중앙 기준으로 이동 (-250 ~ 250 정도)
        crosshair.anchoredPosition = currentFocus * 300f;
    }

    IEnumerator TimerRoutine()
    {
        float t = timeLimit;
        while (t > 0)
        {
            t -= Time.deltaTime;
            timerText.text = $"시간: {t:00.0}초";
            yield return null;
        }
        Failure();
    }

    private void Success()
    {
        StopAllCoroutines();
        timerText.text = "성공! 간식 발견!";
        StartCoroutine(EndGameDelay(true, 2f));
    }

    private void Failure()
    {
        StopAllCoroutines();
        timerText.text = "실패!";
        ShowFunnyImage();
        StartCoroutine(EndGameDelay(false, 5f));
    }

    void ShowFunnyImage()
    {
        if (funnySprites.Length > 0)
        {
            int idx = Random.Range(0, funnySprites.Length);
            resultImage.sprite = funnySprites[idx];
            resultImage.gameObject.SetActive(true);
        }
    }

    IEnumerator EndGameDelay(bool success, float delay)
    {
        yield return new WaitForSeconds(delay);

        // UI 끄기
        uiRoot.SetActive(false);

        // 플레이어 이동 복구
        if (playerCtrlA) playerCtrlA.enabled = true;
        if (playerCtrlB) playerCtrlB.enabled = true;

        // 미니게임 종료
        this.enabled = false;
    }

    // 트리거 스크립트에서 호출
    public void StartGame(MonoBehaviour ctrlA, MonoBehaviour ctrlB)
    {
        playerCtrlA = ctrlA;
        playerCtrlB = ctrlB;
        playerCtrlA.enabled = false;
        playerCtrlB.enabled = false;
        this.enabled = true;
    }
}