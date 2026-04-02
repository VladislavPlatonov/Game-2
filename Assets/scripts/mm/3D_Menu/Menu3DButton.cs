using UnityEngine;
using TMPro;

public class Menu3DButton : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private TextMeshPro frontText;
    [SerializeField] private Transform visualRoot;

    [Header("Action")]
    [SerializeField] private Menu3DActionType actionType = Menu3DActionType.None;

    [Header("Hover")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float scaleSpeed = 10f;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.65f, 0.9f, 1f);

    [Header("One-shot hover animation")]
    [SerializeField] private float hoverAnimDuration = 1f;
    [SerializeField] private float shakeAmountX = 0.004f;
    [SerializeField] private float shakeAmountY = 0.008f;
    [SerializeField] private float shakeSpeed = 12f;
    [SerializeField] private float punchAmount = 0.05f;

    [Header("Screen hover padding")]
    [SerializeField] private float screenPaddingX = 20f;
    [SerializeField] private float screenPaddingY = 12f;

    private bool isHovered;
    private bool wasHoveredLastFrame;
    private float hoverAnimTimer;

    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale;
    private MainMenu3DManager menuManager;
    private Renderer cachedRenderer;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (frontText == null)
            frontText = GetComponentInChildren<TextMeshPro>();

        if (visualRoot == null)
            visualRoot = transform;

        menuManager = FindFirstObjectByType<MainMenu3DManager>();

        baseLocalPosition = visualRoot.localPosition;
        baseLocalScale = visualRoot.localScale;

        if (frontText != null)
        {
            frontText.color = normalColor;
            cachedRenderer = frontText.renderer;
        }
    }

    private void Update()
    {
        UpdateHoverState();
        UpdateHoverEnter();
        UpdateVisuals();
        UpdateClick();
    }

    private void UpdateHoverState()
    {
        if (targetCamera == null || cachedRenderer == null)
        {
            isHovered = false;
            return;
        }

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
        {
            isHovered = false;
            return;
        }

        minX -= screenPaddingX;
        maxX += screenPaddingX;
        minY -= screenPaddingY;
        maxY += screenPaddingY;

        Vector3 mouse = Input.mousePosition;

        isHovered =
            mouse.x >= minX && mouse.x <= maxX &&
            mouse.y >= minY && mouse.y <= maxY;
    }

    private void UpdateHoverEnter()
    {
        if (isHovered && !wasHoveredLastFrame)
        {
            hoverAnimTimer = hoverAnimDuration;
        }

        wasHoveredLastFrame = isHovered;
    }

    private void UpdateVisuals()
    {
        Vector3 targetPos = baseLocalPosition;
        Vector3 targetScale = baseLocalScale * (isHovered ? hoverScale : normalScale);

        if (hoverAnimTimer > 0f)
        {
            hoverAnimTimer -= Time.deltaTime;

            float normalized = 1f - Mathf.Clamp01(hoverAnimTimer / hoverAnimDuration);
            float t = Time.time * shakeSpeed;

            float jitterX = (Mathf.PerlinNoise(t * 1.9f, 0.13f) - 0.5f) * 2f * shakeAmountX;
            float jitterY = (Mathf.PerlinNoise(0.71f, t * 2.3f) - 0.5f) * 2f * shakeAmountY;

            targetPos += new Vector3(jitterX, jitterY, 0f);

            float fadeOut = 1f - normalized;
            float punch = Mathf.Abs(Mathf.Sin(t * 4.2f)) * punchAmount * fadeOut;

            targetScale *= (1f + punch);
        }

        visualRoot.localPosition = Vector3.Lerp(
            visualRoot.localPosition,
            targetPos,
            Time.deltaTime * 20f
        );

        visualRoot.localScale = Vector3.Lerp(
            visualRoot.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );

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
        if (menuManager == null) return;

        menuManager.ExecuteAction(actionType);
    }
}