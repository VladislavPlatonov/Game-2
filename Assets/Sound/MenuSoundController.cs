using UnityEngine;

public class MenuSoundController : MonoBehaviour
{
    public static MenuSoundController Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource normalSource;
    [SerializeField] private AudioSource reverseSource;

    [Header("Clips")]
    [SerializeField] private AudioClip normalClick;
    [SerializeField] private AudioClip reverseClick;

    [Header("Volume")]
    [SerializeField] private float normalVolume = 1f;
    [SerializeField] private float reverseVolume = 1f;

    [Header("Pitch Random")]
    [SerializeField] private float normalPitchMin = 0.92f;
    [SerializeField] private float normalPitchMax = 1.03f;
    [SerializeField] private float reversePitchMin = 0.92f;
    [SerializeField] private float reversePitchMax = 1.03f;

    [Header("ESC")]
    [SerializeField] private bool playReverseOnEscape = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (normalSource != null)
        {
            normalSource.playOnAwake = false;
            normalSource.loop = false;
        }

        if (reverseSource != null)
        {
            reverseSource.playOnAwake = false;
            reverseSource.loop = false;
        }
    }

    private void Update()
    {
        if (playReverseOnEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            PlayReverse();
        }
    }

    public void PlayNormal()
    {
        if (normalSource == null || normalClick == null)
            return;

        normalSource.pitch = Random.Range(normalPitchMin, normalPitchMax);
        normalSource.volume = normalVolume;
        normalSource.PlayOneShot(normalClick);
    }

    public void PlayReverse()
    {
        if (reverseSource == null || reverseClick == null)
            return;

        reverseSource.pitch = Random.Range(reversePitchMin, reversePitchMax);
        reverseSource.volume = reverseVolume;
        reverseSource.PlayOneShot(reverseClick);
    }

    public void PlayClick(bool isBack)
    {
        if (isBack)
            PlayReverse();
        else
            PlayNormal();
    }
}
