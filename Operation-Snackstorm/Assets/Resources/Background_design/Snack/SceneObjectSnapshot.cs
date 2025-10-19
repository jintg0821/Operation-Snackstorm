using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SceneObjectSnapshot : MonoBehaviour
{
    [Header("Target Objects")]
    [Tooltip("스냅샷 찍을 오브젝트들을 여기에 드래그")]
    public List<GameObject> objectsToSnapshot = new List<GameObject>();

    [Header("Camera Settings")]
    [SerializeField] private int imageWidth = 512;
    [SerializeField] private int imageHeight = 512;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0);
    [SerializeField] private float cameraDistance = 1.8f;
    [SerializeField] private float orthographicSize = 0.45f;

    [Header("Light Settings")]
    [SerializeField] private float lightIntensity = 4.5f;
    [SerializeField] private float ambientIntensity = 1.8f;

    [Header("Snapshot Settings")]
    [SerializeField] private float delayBetweenSnapshots = 0.3f;
    [SerializeField] private bool hideOtherObjects = true;

    private string savePath;
    private Camera snapshotCamera;
    private GameObject cameraObject;
    private Light[] snapshotLights;
    private GameObject[] lightObjects;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(TakeAllSnapshots());
        }
    }

    public void StartSnapshot()
    {
        StartCoroutine(TakeAllSnapshots());
    }

    private IEnumerator TakeAllSnapshots()
    {
        savePath = Application.dataPath + "/MobileGallery/SnapShots";

        if (objectsToSnapshot == null || objectsToSnapshot.Count == 0)
        {
            Debug.LogError("스냅샷 찍을 오브젝트가 없습니다!");
            yield break;
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"폴더 생성: {savePath}");
        }

        SetupCamera();
        SetupLights();

        Debug.Log($"<color=cyan>========================================</color>");
        Debug.Log($"<color=cyan>스냅샷 시작: 총 {objectsToSnapshot.Count}개</color>");
        Debug.Log($"<color=cyan>========================================</color>");

        List<GameObject> hiddenObjects = new List<GameObject>();
        if (hideOtherObjects)
        {
            hiddenObjects = HideOtherObjects();
        }

        int successCount = 0;
        int failCount = 0;

        for (int i = 0; i < objectsToSnapshot.Count; i++)
        {
            GameObject obj = objectsToSnapshot[i];

            if (obj == null)
            {
                Debug.LogWarning($"[{i + 1}] Null 오브젝트 건너뛰기");
                failCount++;
                continue;
            }

            yield return new WaitForEndOfFrame();

            bool wasActive = obj.activeSelf;
            obj.SetActive(true);

            yield return null;
            yield return new WaitForEndOfFrame();

            Texture2D snapshot = TakeSnapshot(obj);

            if (snapshot != null)
            {
                string fileName = obj.name + "Icon";
                bool saved = SavePNG(snapshot, fileName);

                if (saved)
                {
                    successCount++;
                    Debug.Log($"<color=green>✓ [{successCount}/{objectsToSnapshot.Count}] {fileName}</color>");
                }
                else
                {
                    failCount++;
                    Debug.LogWarning($"<color=yellow>✗ {fileName} 저장 실패</color>");
                }

                Destroy(snapshot);
            }
            else
            {
                failCount++;
                Debug.LogWarning($"<color=yellow>✗ [{i + 1}] {obj.name} 촬영 실패</color>");
            }

            obj.SetActive(wasActive);

            yield return new WaitForSeconds(delayBetweenSnapshots);
        }

        if (hideOtherObjects)
        {
            ShowObjects(hiddenObjects);
        }

        Debug.Log($"<color=cyan>========================================</color>");
        Debug.Log($"<color=green>스냅샷 완료!</color>");
        Debug.Log($"<color=green>성공: {successCount}개</color>");
        if (failCount > 0)
        {
            Debug.Log($"<color=yellow>실패: {failCount}개</color>");
        }
        Debug.Log($"<color=cyan>저장 위치: {savePath}</color>");
        Debug.Log($"<color=cyan>========================================</color>");

        CleanupCamera();
    }

    private List<GameObject> HideOtherObjects()
    {
        List<GameObject> hiddenObjects = new List<GameObject>();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (!objectsToSnapshot.Contains(obj) && obj.activeSelf)
            {
                if (obj.GetComponent<Camera>() == null &&
                    obj.GetComponent<Light>() == null &&
                    obj != this.gameObject)
                {
                    obj.SetActive(false);
                    hiddenObjects.Add(obj);
                }
            }
        }

        return hiddenObjects;
    }

    private void ShowObjects(List<GameObject> objects)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    private void SetupCamera()
    {
        cameraObject = new GameObject("SnapshotCamera");
        cameraObject.transform.position = Vector3.zero;

        snapshotCamera = cameraObject.AddComponent<Camera>();
        snapshotCamera.backgroundColor = backgroundColor;
        snapshotCamera.clearFlags = CameraClearFlags.SolidColor;
        snapshotCamera.orthographic = true;
        snapshotCamera.orthographicSize = orthographicSize;
        snapshotCamera.nearClipPlane = 0.1f;
        snapshotCamera.farClipPlane = 100f;
        snapshotCamera.enabled = false;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.white;
        RenderSettings.ambientIntensity = ambientIntensity;
    }

    private void SetupLights()
    {
        lightObjects = new GameObject[3];
        snapshotLights = new Light[3];

        lightObjects[0] = new GameObject("FrontLight");
        snapshotLights[0] = lightObjects[0].AddComponent<Light>();
        snapshotLights[0].type = LightType.Directional;
        snapshotLights[0].intensity = lightIntensity;
        snapshotLights[0].color = Color.white;

        lightObjects[1] = new GameObject("TopLight");
        snapshotLights[1] = lightObjects[1].AddComponent<Light>();
        snapshotLights[1].type = LightType.Directional;
        snapshotLights[1].intensity = lightIntensity * 0.6f;
        snapshotLights[1].color = Color.white;

        lightObjects[2] = new GameObject("SideLight");
        snapshotLights[2] = lightObjects[2].AddComponent<Light>();
        snapshotLights[2].type = LightType.Directional;
        snapshotLights[2].intensity = lightIntensity * 0.4f;
        snapshotLights[2].color = Color.white;
    }

    private Texture2D TakeSnapshot(GameObject obj)
    {
        if (obj == null || snapshotCamera == null)
            return null;

        Vector3 objPos = obj.transform.position;

        snapshotCamera.transform.position = objPos + new Vector3(0, 0, -cameraDistance);
        snapshotCamera.transform.LookAt(objPos);

        lightObjects[0].transform.position = objPos + new Vector3(0, 0, -cameraDistance + 0.5f);
        lightObjects[0].transform.LookAt(objPos);

        lightObjects[1].transform.position = objPos + new Vector3(0, 5, 0);
        lightObjects[1].transform.LookAt(objPos);

        lightObjects[2].transform.position = objPos + new Vector3(3, 0, -cameraDistance);
        lightObjects[2].transform.LookAt(objPos);

        RenderTexture rt = RenderTexture.GetTemporary(imageWidth, imageHeight, 24, RenderTextureFormat.ARGB32);
        snapshotCamera.targetTexture = rt;
        RenderTexture.active = rt;

        snapshotCamera.Render();

        Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        texture.Apply();

        snapshotCamera.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return texture;
    }

    private bool SavePNG(Texture2D texture, string fileName)
    {
        try
        {
            byte[] bytes = texture.EncodeToPNG();
            if (bytes == null || bytes.Length == 0)
                return false;

            string fullPath = Path.Combine(savePath, fileName + ".png");
            File.WriteAllBytes(fullPath, bytes);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SavePNG 에러: {e.Message}");
            return false;
        }
    }

    private void CleanupCamera()
    {
        if (cameraObject != null)
        {
            Destroy(cameraObject);
            cameraObject = null;
            snapshotCamera = null;
        }

        if (lightObjects != null)
        {
            foreach (GameObject lightObj in lightObjects)
            {
                if (lightObj != null)
                {
                    Destroy(lightObj);
                }
            }
            lightObjects = null;
            snapshotLights = null;
        }
    }

    private void OnDestroy()
    {
        CleanupCamera();
    }
}
