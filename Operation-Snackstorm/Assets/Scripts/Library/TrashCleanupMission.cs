using UnityEngine;
using Photon.Pun;
using TMPro;

public class TrashCleanupMission : MonoBehaviour
{
    public static TrashCleanupMission Instance;

    [Header("UI")]
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("설정")]
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private int requiredTrashCount = 5;
    [SerializeField] private string trashPrefabPath = "Trash";

    [Header("스폰 구역")]
    [SerializeField] private BoxCollider spawnArea;

    public bool isMissionActive = false;
    private float timer;
    private int collectedTrashCount;
    private PlayerController punishedPlayer;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isMissionActive) return;

        timer -= Time.deltaTime;
        timerText.text = $"남은 시간: {timer:F1}";

        if (timer <= 0)
        {
            EndMission(false);
        }
    }

    public void StartMission(PlayerController player)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CleanupAllTrash();
        }

        punishedPlayer = player;
        isMissionActive = true;
        timer = timeLimit;
        collectedTrashCount = 0;

        missionPanel.SetActive(true);
        UpdateObjectiveText();

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < requiredTrashCount; i++)
            {
                Vector3 randomPos = GetRandomPositionInBounds(spawnArea.bounds);
                PhotonNetwork.Instantiate(trashPrefabPath, randomPos, Quaternion.identity);
            }
        }
    }

    public void OnTrashCollected()
    {
        if (!isMissionActive) return;

        collectedTrashCount++;
        UpdateObjectiveText();

        if (collectedTrashCount >= requiredTrashCount)
        {
            EndMission(true);
        }
    }

    private void EndMission(bool success)
    {
        isMissionActive = false;
        missionPanel.SetActive(false);

        if (punishedPlayer != null)
        {
            punishedPlayer.GrantPunishmentImmunity(5f);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            CleanupAllTrash();
        }

        if (success)
        {
            Debug.Log("쓰레기 줍기 성공");
        }
        else
        {
            Debug.Log("쓰레기 줍기 실패 (시간 초과)");
        }
    }

    private void CleanupAllTrash()
    {
        TrashObject[] allTrash = FindObjectsOfType<TrashObject>();
        foreach (TrashObject trash in allTrash)
        {
            if (trash.photonView != null)
            {
                trash.photonView.RPC("DestroyTrash", RpcTarget.All);
            }
        }
    }

    private void UpdateObjectiveText()
    {
        objectiveText.text = $"쓰레기 줍기 ({collectedTrashCount} / {requiredTrashCount})";
    }

    private Vector3 GetRandomPositionInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}