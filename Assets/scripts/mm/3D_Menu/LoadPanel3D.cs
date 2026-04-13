using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPanel3D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform savesListRoot;
    [SerializeField] private GameObject saveSlotTemplate;

    [Header("Visible List")]
    [SerializeField] private int visibleSlotCount = 3;
    [SerializeField] private float slotSpacingY = 1.12f;
    [SerializeField] private int scrollStep = 1;

    [Header("Slot Hover")]
    [SerializeField] private float slotHoverScale = 1.03f;
    [SerializeField] private float slotScaleSpeed = 8f;
    [SerializeField] private float slotHoverPaddingX = 0f;
    [SerializeField] private float slotHoverPaddingY = 0f;

    [Header("Button Hover")]
    [SerializeField] private float buttonHoverScale = 1.06f;
    [SerializeField] private float buttonScaleSpeed = 10f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.75f, 0.95f, 1f);
    [SerializeField] private float hoverPaddingX = 16f;
    [SerializeField] private float hoverPaddingY = 10f;

    private readonly List<SlotUI> spawnedSlots = new();
    private readonly List<SaveFileEntry> loadedSaves = new();

    private int topVisibleIndex = 0;

    private string SaveFolderPath => Path.Combine(Application.persistentDataPath, "saves");

    private class SaveFileEntry
    {
        public string jsonPath;
        public PokerUnifiedSaveData data;
        public Texture2D screenshot;
    }

    private class SlotUI
    {
        public int saveListIndex;
        public GameObject root;
        public Transform rootTransform;
        public Vector3 rootBaseScale;

        public Renderer slotBackRenderer;

        public Transform loadButtonRoot;
        public Transform deleteButtonRoot;

        public TextMeshPro loadButtonText;
        public TextMeshPro deleteButtonText;

        public Vector3 loadBaseScale;
        public Vector3 deleteBaseScale;
    }

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void OnEnable()
    {
        ReloadSaveFiles();
        RebuildList();
    }

    private void Update()
    {
        UpdateScroll();
        UpdateSlotHover();
        UpdateButtonsHoverAndClick();
    }

    public void ReloadSaveFiles()
    {
        loadedSaves.Clear();

        if (!Directory.Exists(SaveFolderPath))
            Directory.CreateDirectory(SaveFolderPath);

        string[] files = Directory.GetFiles(SaveFolderPath, "*.json");
        Array.Sort(files, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

        for (int i = 0; i < files.Length; i++)
        {
            string path = files[i];

            try
            {
                string json = File.ReadAllText(path);
                PokerUnifiedSaveData data = JsonUtility.FromJson<PokerUnifiedSaveData>(json);

                if (data == null)
                    continue;

                Texture2D screenshot = LoadScreenshot(data.screenshotFileName);

                loadedSaves.Add(new SaveFileEntry
                {
                    jsonPath = path,
                    data = data,
                    screenshot = screenshot
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LoadPanel3D] Не удалось прочитать сейв {path}: {e}");
            }
        }

        topVisibleIndex = Mathf.Clamp(topVisibleIndex, 0, GetMaxTopIndex());
    }

    public void RebuildList()
    {
        ClearSpawnedSlots();

        if (savesListRoot == null || saveSlotTemplate == null)
            return;

        saveSlotTemplate.SetActive(false);

        int countToShow = Mathf.Min(visibleSlotCount, loadedSaves.Count);

        for (int i = 0; i < countToShow; i++)
        {
            int saveListIndex = topVisibleIndex + i;
            if (saveListIndex >= loadedSaves.Count)
                break;

            SaveFileEntry entry = loadedSaves[saveListIndex];

            GameObject go = Instantiate(saveSlotTemplate, savesListRoot);
            go.name = "SaveSlot_" + saveListIndex;
            go.SetActive(true);

            go.transform.localPosition = new Vector3(0f, -i * slotSpacingY, 0f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            ApplySlotVisuals(go.transform, saveListIndex, entry);

            SlotUI slot = new SlotUI
            {
                saveListIndex = saveListIndex,
                root = go,
                rootTransform = go.transform,
                rootBaseScale = go.transform.localScale
            };

            Transform slotBack = FindDeepChild(go.transform, "Slot_Back");
            if (slotBack != null)
                slot.slotBackRenderer = slotBack.GetComponent<Renderer>();

            slot.loadButtonRoot = FindDeepChild(go.transform, "Load_Button_Group");
            slot.deleteButtonRoot = FindDeepChild(go.transform, "Delete_Button_Group");

            slot.loadButtonText = GetTMPFromGroup(slot.loadButtonRoot);
            slot.deleteButtonText = GetTMPFromGroup(slot.deleteButtonRoot);

            if (slot.loadButtonRoot != null)
                slot.loadBaseScale = slot.loadButtonRoot.localScale;

            if (slot.deleteButtonRoot != null)
                slot.deleteBaseScale = slot.deleteButtonRoot.localScale;

            spawnedSlots.Add(slot);
        }
    }

    private void ApplySlotVisuals(Transform slotRoot, int saveIndex, SaveFileEntry entry)
    {
        TextMeshPro saveIndexText =
            GetTMPFromGroup(FindDeepChild(slotRoot, "SaveIndex_Text"));

        TextMeshPro saveDateText =
            GetTMPFromGroup(FindDeepChild(slotRoot, "Save_Date_Text"));

        TextMeshPro saveSceneText =
            GetTMPFromGroup(FindDeepChild(slotRoot, "Save_Scene_Text"));

        MeshRenderer screenshotRenderer =
            FindDeepChild(slotRoot, "Screenshot_Plane")?.GetComponent<MeshRenderer>();

        // Номер сейва считаем по позиции в общем списке
        string saveTitle = $"СЕЙВ {saveIndex + 1}";

        string saveDate = entry.data != null && !string.IsNullOrWhiteSpace(entry.data.createdAt)
            ? entry.data.createdAt
            : "ПУСТО";

        string saveScene = entry.data != null && !string.IsNullOrWhiteSpace(entry.data.sceneName)
            ? entry.data.sceneName
            : "";

        if (saveIndexText != null)
            saveIndexText.text = saveTitle;

        if (saveDateText != null)
            saveDateText.text = saveDate;

        if (saveSceneText != null)
            saveSceneText.text = saveScene;

        if (screenshotRenderer != null)
        {
            Material mat = new Material(screenshotRenderer.sharedMaterial);

            if (entry.screenshot != null)
                mat.mainTexture = entry.screenshot;
            else
                mat.mainTexture = null;

            screenshotRenderer.material = mat;
        }
    }


    private void UpdateSlotHover()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            SlotUI slot = spawnedSlots[i];
            if (slot.rootTransform == null)
                continue;

            bool hovered = false;

            if (slot.slotBackRenderer != null)
            {
                hovered = IsMouseOverRenderer(
                    slot.slotBackRenderer,
                    slotHoverPaddingX,
                    slotHoverPaddingY
                );
            }

            Vector3 targetScale = hovered
                ? slot.rootBaseScale * slotHoverScale
                : slot.rootBaseScale;

            slot.rootTransform.localScale = Vector3.Lerp(
                slot.rootTransform.localScale,
                targetScale,
                Time.deltaTime * slotScaleSpeed
            );
        }
    }

    private void UpdateButtonsHoverAndClick()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            SlotUI slot = spawnedSlots[i];

            HandleButton(
                slot.saveListIndex,
                slot.loadButtonRoot,
                slot.loadButtonText,
                slot.loadBaseScale,
                true
            );

            HandleButton(
                slot.saveListIndex,
                slot.deleteButtonRoot,
                slot.deleteButtonText,
                slot.deleteBaseScale,
                false
            );
        }
    }

    private void HandleButton(
        int saveListIndex,
        Transform buttonRoot,
        TextMeshPro buttonText,
        Vector3 baseScale,
        bool isLoadButton
    )
    {
        if (buttonRoot == null || buttonText == null || targetCamera == null)
            return;

        bool hovered = IsMouseOverTMP(buttonText, hoverPaddingX, hoverPaddingY);

        Vector3 targetScale = hovered ? baseScale * buttonHoverScale : baseScale;

        buttonRoot.localScale = Vector3.Lerp(
            buttonRoot.localScale,
            targetScale,
            Time.deltaTime * buttonScaleSpeed
        );

        Color targetColor = hovered ? hoverColor : normalColor;

        buttonText.color = Color.Lerp(
            buttonText.color,
            targetColor,
            Time.deltaTime * 12f
        );

        if (hovered && Input.GetMouseButtonDown(0))
        {
            if (saveListIndex < 0 || saveListIndex >= loadedSaves.Count)
                return;

            if (isLoadButton)
            {
                LoadSaveFromEntry(loadedSaves[saveListIndex]);
            }
            else
            {
                DeleteSaveEntry(loadedSaves[saveListIndex]);
            }
        }
    }

    private void LoadSaveFromEntry(SaveFileEntry entry)
    {
        if (entry == null || entry.data == null || string.IsNullOrEmpty(entry.jsonPath))
            return;

        MainMenuLoadController.PendingLoadSavePath = entry.jsonPath;
        SceneManager.LoadScene(entry.data.sceneName);
    }

    private void DeleteSaveEntry(SaveFileEntry entry)
    {
        if (entry == null)
            return;

        try
        {
            if (!string.IsNullOrEmpty(entry.jsonPath) && File.Exists(entry.jsonPath))
                File.Delete(entry.jsonPath);

            if (entry.data != null && !string.IsNullOrEmpty(entry.data.screenshotFileName))
            {
                string screenshotPath = Path.Combine(SaveFolderPath, entry.data.screenshotFileName);
                if (File.Exists(screenshotPath))
                    File.Delete(screenshotPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LoadPanel3D] Delete error: {e}");
        }

        ReloadSaveFiles();
        RebuildList();
    }

    private Texture2D LoadScreenshot(string screenshotFileName)
    {
        if (string.IsNullOrEmpty(screenshotFileName))
            return null;

        string path = Path.Combine(SaveFolderPath, screenshotFileName);
        if (!File.Exists(path))
            return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(bytes);
            return texture;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LoadPanel3D] Screenshot load error: {e}");
            return null;
        }
    }

    private void UpdateScroll()
    {
        if (!gameObject.activeInHierarchy)
            return;

        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) < 0.01f)
            return;

        if (wheel < 0f)
            topVisibleIndex += scrollStep;
        else if (wheel > 0f)
            topVisibleIndex -= scrollStep;

        topVisibleIndex = Mathf.Clamp(topVisibleIndex, 0, GetMaxTopIndex());
        RebuildList();
    }

    private int GetMaxTopIndex()
    {
        if (loadedSaves.Count <= visibleSlotCount)
            return 0;

        return loadedSaves.Count - visibleSlotCount;
    }

    private bool IsMouseOverTMP(TextMeshPro tmp, float paddingX, float paddingY)
    {
        if (tmp == null || tmp.renderer == null || targetCamera == null)
            return false;

        return IsMouseOverBounds(tmp.renderer.bounds, paddingX, paddingY);
    }

    private bool IsMouseOverRenderer(Renderer rendererRef, float paddingX, float paddingY)
    {
        if (rendererRef == null || targetCamera == null)
            return false;

        return IsMouseOverBounds(rendererRef.bounds, paddingX, paddingY);
    }

    private bool IsMouseOverBounds(Bounds b, float paddingX, float paddingY)
    {
        Vector3[] corners = new Vector3[8];

        Vector3 min = b.min;
        Vector3 max = b.max;

        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(max.x, min.y, min.z);
        corners[2] = new Vector3(min.x, max.y, min.z);
        corners[3] = new Vector3(max.x, max.y, min.z);
        corners[4] = new Vector3(min.x, min.y, max.z);
        corners[5] = new Vector3(max.x, min.y, max.z);
        corners[6] = new Vector3(min.x, max.y, max.z);
        corners[7] = new Vector3(max.x, max.y, max.z);

        bool anyVisible = false;

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 screen = targetCamera.WorldToScreenPoint(corners[i]);

            if (screen.z > 0f)
            {
                anyVisible = true;

                minX = Mathf.Min(minX, screen.x);
                minY = Mathf.Min(minY, screen.y);
                maxX = Mathf.Max(maxX, screen.x);
                maxY = Mathf.Max(maxY, screen.y);
            }
        }

        if (!anyVisible)
            return false;

        minX -= paddingX;
        maxX += paddingX;
        minY -= paddingY;
        maxY += paddingY;

        Vector3 mouse = Input.mousePosition;

        return mouse.x >= minX && mouse.x <= maxX &&
               mouse.y >= minY && mouse.y <= maxY;
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == childName)
                return child;

            Transform found = FindDeepChild(child, childName);
            if (found != null)
                return found;
        }

        return null;
    }

    private TextMeshPro GetTMPFromGroup(Transform group)
    {
        if (group == null)
            return null;

        return group.GetComponentInChildren<TextMeshPro>(true);
    }

    private void ClearSpawnedSlots()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null && spawnedSlots[i].root != null)
                Destroy(spawnedSlots[i].root);
        }

        spawnedSlots.Clear();
    }
}
