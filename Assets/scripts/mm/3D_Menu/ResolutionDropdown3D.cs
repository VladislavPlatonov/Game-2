using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionDropdown3D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private GameObject dropdownPanel;
    [SerializeField] private Renderer dropdownBackRenderer;
    [SerializeField] private TextMeshPro currentResolutionText;
    [SerializeField] private Transform optionsRoot;
    [SerializeField] private GameObject optionTemplate;

    [Header("Visible List")]
    [SerializeField] private int visibleOptionCount = 5;
    [SerializeField] private float optionSpacingY = 0.28f;
    [SerializeField] private int scrollStep = 1;

    [Header("Hover Area Padding")]
    [SerializeField] private float panelHoverPaddingX = 20f;
    [SerializeField] private float panelHoverPaddingY = 14f;

    private readonly List<Resolution> uniqueResolutions = new();
    private readonly List<GameObject> spawnedOptions = new();

    private int currentResolutionIndex = -1;
    private int topVisibleIndex = 0;
    private bool isOpen;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        BuildResolutionList();
        ClampTopIndexToCurrent();
        BuildVisibleOptions();
        RefreshCurrentResolutionText();

        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen)
            return;

        HandleMouseWheelScroll();
    }

    public void ToggleDropdown()
    {
        isOpen = !isOpen;

        if (dropdownPanel != null)
            dropdownPanel.SetActive(isOpen);

        if (isOpen)
        {
            ClampTopIndexToCurrent();
            BuildVisibleOptions();
        }
    }

    public void CloseDropdown()
    {
        isOpen = false;

        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);
    }

    public void SelectResolution(int realIndex)
    {
        if (realIndex < 0 || realIndex >= uniqueResolutions.Count)
            return;

        Resolution r = uniqueResolutions[realIndex];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);

        currentResolutionIndex = realIndex;
        RefreshCurrentResolutionText();
        CloseDropdown();
    }

    private void BuildResolutionList()
    {
        uniqueResolutions.Clear();

        Resolution[] all = Screen.resolutions;
        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < all.Length; i++)
        {
            string key = all[i].width + "x" + all[i].height;
            if (seen.Contains(key))
                continue;

            seen.Add(key);
            uniqueResolutions.Add(all[i]);
        }

        int currentW = Screen.currentResolution.width;
        int currentH = Screen.currentResolution.height;

        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            if (uniqueResolutions[i].width == currentW &&
                uniqueResolutions[i].height == currentH)
            {
                currentResolutionIndex = i;
                break;
            }
        }

        if (currentResolutionIndex < 0 && uniqueResolutions.Count > 0)
            currentResolutionIndex = uniqueResolutions.Count - 1;
    }

    private void ClampTopIndexToCurrent()
    {
        if (uniqueResolutions.Count == 0)
            return;

        topVisibleIndex = Mathf.Clamp(
            currentResolutionIndex - 1,
            0,
            Mathf.Max(0, uniqueResolutions.Count - visibleOptionCount)
        );
    }

    private void BuildVisibleOptions()
    {
        if (optionsRoot == null || optionTemplate == null)
            return;

        for (int i = 0; i < spawnedOptions.Count; i++)
        {
            if (spawnedOptions[i] != null)
                Destroy(spawnedOptions[i]);
        }

        spawnedOptions.Clear();

        optionTemplate.SetActive(false);

        int count = Mathf.Min(visibleOptionCount, uniqueResolutions.Count);

        for (int i = 0; i < count; i++)
        {
            int realIndex = topVisibleIndex + i;
            if (realIndex >= uniqueResolutions.Count)
                break;

            GameObject option = Instantiate(optionTemplate, optionsRoot);
            option.name = "Resolution_Option_" + realIndex;
            option.SetActive(true);

            option.transform.localPosition = new Vector3(0f, -i * optionSpacingY, 0f);
            option.transform.localRotation = Quaternion.identity;
            option.transform.localScale = Vector3.one;

            ResolutionOption3DNoCollider optionScript = option.GetComponent<ResolutionOption3DNoCollider>();
            if (optionScript != null)
                optionScript.Setup(this, targetCamera, realIndex);

            TextMeshPro tmp = option.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
                tmp.text = uniqueResolutions[realIndex].width + "x" + uniqueResolutions[realIndex].height;

            spawnedOptions.Add(option);
        }
    }

    private void RefreshCurrentResolutionText()
    {
        if (currentResolutionText == null)
            return;

        if (currentResolutionIndex < 0 || currentResolutionIndex >= uniqueResolutions.Count)
        {
            currentResolutionText.text = "N/A";
            return;
        }

        Resolution r = uniqueResolutions[currentResolutionIndex];
        currentResolutionText.text = r.width + "x" + r.height;
    }

    private void HandleMouseWheelScroll()
    {
        if (!IsMouseOverDropdown())
            return;

        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) < 0.01f)
            return;

        if (wheel > 0f)
            topVisibleIndex -= scrollStep;
        else if (wheel < 0f)
            topVisibleIndex += scrollStep;

        topVisibleIndex = Mathf.Clamp(
            topVisibleIndex,
            0,
            Mathf.Max(0, uniqueResolutions.Count - visibleOptionCount)
        );

        BuildVisibleOptions();
    }

    private bool IsMouseOverDropdown()
    {
        if (targetCamera == null || dropdownBackRenderer == null)
            return false;

        Bounds b = dropdownBackRenderer.bounds;

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

        minX -= panelHoverPaddingX;
        maxX += panelHoverPaddingX;
        minY -= panelHoverPaddingY;
        maxY += panelHoverPaddingY;

        Vector3 mouse = Input.mousePosition;

        return mouse.x >= minX && mouse.x <= maxX &&
               mouse.y >= minY && mouse.y <= maxY;
    }
}