using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class VhsPixelEffect : MonoBehaviour
{
    [Header("Pixelation")]
    [Range(0.1f, 1f)]
    public float resolutionScale = 0.7f;

    [Header("VHS Jitter")]
    public float jitterStrength = 0.002f;
    public float jitterSpeed = 10f;

    private Camera cam;
    private Vector3 originalPos;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        originalPos = transform.localPosition;
    }

    private void Update()
    {
        // 📺 VHS тряска
        float jitterX = Mathf.Sin(Time.time * jitterSpeed) * jitterStrength;
        float jitterY = Mathf.Cos(Time.time * jitterSpeed * 1.3f) * jitterStrength;

        transform.localPosition = originalPos + new Vector3(jitterX, jitterY, 0);
    }

    private void OnPreCull()
    {
        if (cam == null) return;

        // 🧊 Псевдо пиксели (снижение разрешения)
        cam.pixelRect = new Rect(
            0,
            0,
            Screen.width * resolutionScale,
            Screen.height * resolutionScale
        );
    }

    private void OnPostRender()
    {
        if (cam == null) return;

        // возвращаем нормальный размер
        cam.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
    }
}
