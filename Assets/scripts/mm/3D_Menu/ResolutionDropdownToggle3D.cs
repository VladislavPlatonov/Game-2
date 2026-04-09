using TMPro;
using UnityEngine;

public class ResolutionDropdownToggle3D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private ResolutionDropdown3D dropdown;
    [SerializeField] private TextMeshPro frontText;
    [SerializeField] private Transform visualRoot;

    [Header("Hover")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float scaleSpeed = 10f;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.75f, 0.95f, 1f);

    [Header("Hover Padding")]
    [SerializeField] private float screenPaddingX = 18f;
    [SerializeField] private float screenPaddingY = 10f;

    private bool isHovered;
    private Vector3 baseLocalScale;
    private Renderer cachedRenderer;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (visualRoot == null)
            visualRoot = transform;

        if (frontText == null)
            frontText = GetComponentInChildren<TextMeshPro>();

        if (frontText != null)
            cachedRenderer = frontText.renderer;

        baseLocalScale = visualRoot.localScale;
    }

    private void Update()
    {
        UpdateHoverState();
        UpdateVisuals();
        UpdateClick();
    }

    private void UpdateHoverState()
    {
        isHovered = false;

        if (targetCamera == null || cachedRenderer == null)
            return;

        Bounds b = cachedRenderer.bounds;

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
            return;

        minX -= screenPaddingX;
        maxX += screenPaddingX;
        minY -= screenPaddingY;
        maxY += screenPaddingY;

        Vector3 mouse = Input.mousePosition;

        isHovered =
            mouse.x >= minX && mouse.x <= maxX &&
            mouse.y >= minY && mouse.y <= maxY;
    }

    private void UpdateVisuals()
    {
        if (visualRoot != null)
        {
            float targetScale = isHovered ? hoverScale : normalScale;
            visualRoot.localScale = Vector3.Lerp(
                visualRoot.localScale,
                baseLocalScale * targetScale,
                Time.deltaTime * scaleSpeed
            );
        }

        if (frontText != null)
        {
            Color targetColor = isHovered ? hoverColor : normalColor;
            frontText.color = Color.Lerp(frontText.color, targetColor, Time.deltaTime * 12f);
        }
    }

    private void UpdateClick()
    {
        if (!isHovered) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (dropdown == null) return;

        dropdown.ToggleDropdown();
    }
}
