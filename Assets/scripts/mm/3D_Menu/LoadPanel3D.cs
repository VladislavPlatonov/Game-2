using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadPanel3D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform savesListRoot;
    [SerializeField] private GameObject saveSlotTemplate;

    [Header("Data")]
    [SerializeField] private int maxSlots = 10;

    [Header("Visible List")]
    [SerializeField] private int visibleSlotCount = 3;
    [SerializeField] private float slotSpacingY = 1.12f;
    [SerializeField] private int scrollStep = 1;

    [Header("Slot Hover")]
    [SerializeField] private float slotHoverScale = 1.03f;
    [SerializeField] private float slotScaleSpeed = 8f;
    [SerializeField] private float slotHoverPaddingX = 16f;
    [SerializeField] private float slotHoverPaddingY = 12f;

    [Header("Button Hover")]
    [SerializeField] private float buttonHoverScale = 1.06f;
    [SerializeField] private float buttonScaleSpeed = 10f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.75f, 0.95f, 1f);
    [SerializeField] private float hoverPaddingX = 16f;
    [SerializeField] private float hoverPaddingY = 10f;

    private readonly List<SlotUI> spawnedSlots = new List<SlotUI>();
    private int topVisibleSlotIndex = 1;

    private class SlotUI
    {
        public int slotIndex;
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
        topVisibleSlotIndex = Mathf.Clamp(
            topVisibleSlotIndex,
            1,
            GetMaxTopIndex()
        );

        RebuildList();
    }

    private void Update()
    {
        UpdateScroll();
        UpdateSlotHover();
        UpdateButtonsHoverAndClick();
    }

    public void RebuildList()
    {
        ClearSpawnedSlots();

        if (savesListRoot == null ||
            saveSlotTemplate == null ||
            LoadMenuSaveSystem.Instance == null)
            return;

        saveSlotTemplate.SetActive(false);

        int countToShow = Mathf.Min(visibleSlotCount, maxSlots);

        for (int i = 0; i < countToShow; i++)
        {
            int slotIndex = topVisibleSlotIndex + i;

            if (slotIndex > maxSlots)
                break;

            GameObject go = Instantiate(saveSlotTemplate, savesListRoot);
            go.name = "SaveSlot_" + slotIndex;
            go.SetActive(true);

            go.transform.localPosition = new Vector3(
                0f,
                -i * slotSpacingY,
                0f
            );

            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            SaveMetadata meta =
                LoadMenuSaveSystem.Instance.LoadMetadata(slotIndex);

            Texture2D screenshot = null;

            if (meta != null)
                screenshot = LoadMenuSaveSystem.Instance
                    .LoadScreenshot(meta.screenshotFileName);

            ApplySlotVisuals(
                go.transform,
                slotIndex,
                meta,
                screenshot
            );

            SlotUI slot = new SlotUI();

            slot.slotIndex = slotIndex;
            slot.root = go;
            slot.rootTransform = go.transform;
            slot.rootBaseScale = go.transform.localScale;

            Transform slotBack = FindDeepChild(go.transform, "Slot_Back");

            if (slotBack != null)
                slot.slotBackRenderer = slotBack.GetComponent<Renderer>();

            slot.loadButtonRoot =
                FindDeepChild(go.transform, "Load_Button_Group");

            slot.deleteButtonRoot =
                FindDeepChild(go.transform, "Delete_Button_Group");

            slot.loadButtonText = GetTMPFromGroup(slot.loadButtonRoot);
            slot.deleteButtonText = GetTMPFromGroup(slot.deleteButtonRoot);

            if (slot.loadButtonRoot != null)
                slot.loadBaseScale = slot.loadButtonRoot.localScale;

            if (slot.deleteButtonRoot != null)
                slot.deleteBaseScale = slot.deleteButtonRoot.localScale;

            spawnedSlots.Add(slot);
        }
    }

    private void ApplySlotVisuals(
        Transform slotRoot,
        int slotIndex,
        SaveMetadata meta,
        Texture2D screenshot
    )
    {
        TextMeshPro saveIndexText =
            GetTMPFromGroup(FindDeepChild(slotRoot, "SaveIndex_Text"));

        TextMeshPro saveDateText =
            GetTMPFromGroup(FindDeepChild(slotRoot, "Save_Date_Text"));

        MeshRenderer screenshotRenderer =
            FindDeepChild(slotRoot, "Screenshot_Plane")
            ?.GetComponent<MeshRenderer>();

        if (saveIndexText != null)
            saveIndexText.text = meta != null
                ? meta.saveTitle
                : "яеиб " + slotIndex;

        if (saveDateText != null)
            saveDateText.text = meta != null
                ? meta.saveDate
                : "осярн";

        if (screenshotRenderer != null)
        {
            Material mat =
                new Material(screenshotRenderer.sharedMaterial);

            if (screenshot != null)
                mat.mainTexture = screenshot;
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
                slot.slotIndex,
                slot.loadButtonRoot,
                slot.loadButtonText,
                slot.loadBaseScale,
                true
            );

            HandleButton(
                slot.slotIndex,
                slot.deleteButtonRoot,
                slot.deleteButtonText,
                slot.deleteBaseScale,
                false
            );
        }
    }

    private void HandleButton(
        int slotIndex,
        Transform buttonRoot,
        TextMeshPro buttonText,
        Vector3 baseScale,
        bool isLoadButton
    )
    {
        if (buttonRoot == null ||
            buttonText == null ||
            targetCamera == null)
            return;

        bool hovered = IsMouseOverTMP(
            buttonText,
            hoverPaddingX,
            hoverPaddingY
        );

        Vector3 targetScale = hovered
            ? baseScale * buttonHoverScale
            : baseScale;

        buttonRoot.localScale = Vector3.Lerp(
            buttonRoot.localScale,
            targetScale,
            Time.deltaTime * buttonScaleSpeed
        );

        Color targetColor = hovered
            ? hoverColor
            : normalColor;

        buttonText.color = Color.Lerp(
            buttonText.color,
            targetColor,
            Time.deltaTime * 12f
        );

        if (hovered && Input.GetMouseButtonDown(0))
        {
            if (isLoadButton)
            {
                LoadMenuSaveSystem.Instance.LoadSlot(slotIndex);
            }
            else
            {
                LoadMenuSaveSystem.Instance.DeleteSave(slotIndex);
                RebuildList();
            }
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
            topVisibleSlotIndex += scrollStep;
        else if (wheel > 0f)
            topVisibleSlotIndex -= scrollStep;

        topVisibleSlotIndex = Mathf.Clamp(
            topVisibleSlotIndex,
            1,
            GetMaxTopIndex()
        );

        RebuildList();
    }

    private int GetMaxTopIndex()
    {
        return Mathf.Max(
            1,
            maxSlots - visibleSlotCount + 1
        );
    }

    private bool IsMouseOverTMP(
        TextMeshPro tmp,
        float paddingX,
        float paddingY
    )
    {
        if (tmp == null ||
            tmp.renderer == null ||
            targetCamera == null)
            return false;

        return IsMouseOverBounds(
            tmp.renderer.bounds,
            paddingX,
            paddingY
        );
    }

    private bool IsMouseOverRenderer(
        Renderer rendererRef,
        float paddingX,
        float paddingY
    )
    {
        if (rendererRef == null || targetCamera == null)
            return false;

        return IsMouseOverBounds(
            rendererRef.bounds,
            paddingX,
            paddingY
        );
    }

    private bool IsMouseOverBounds(
        Bounds b,
        float paddingX,
        float paddingY
    )
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
            Vector3 screen =
                targetCamera.WorldToScreenPoint(corners[i]);

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
            if (spawnedSlots[i] != null &&
                spawnedSlots[i].root != null)
                Destroy(spawnedSlots[i].root);
        }

        spawnedSlots.Clear();
    }
}