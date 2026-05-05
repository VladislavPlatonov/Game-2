using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    public static MenuAudioManager Instance;

    [Header("Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource uiSource;

    [Header("Ambient")]
    [SerializeField] private AudioClip menuAmbient;
    [Range(0f, 1f)] public float ambientVolume = 0.4f;

    [Header("UI")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip buttonHover;
    [Range(0f, 1f)] public float uiVolume = 0.7f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PlayAmbient();
    }

    // =========================
    // 🌫 AMBIENT
    // =========================
    public void PlayAmbient()
    {
        if (ambientSource == null || menuAmbient == null)
            return;

        ambientSource.clip = menuAmbient;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.Play();
    }

    // =========================
    // 🖱 UI
    // =========================
    public void PlayClick()
    {
        if (uiSource == null || buttonClick == null)
            return;

        uiSource.PlayOneShot(buttonClick, uiVolume);
    }

    public void PlayHover()
    {
        if (uiSource == null || buttonHover == null)
            return;

        uiSource.PlayOneShot(buttonHover, uiVolume);
    }
}
