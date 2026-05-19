using System.Collections;
using UnityEngine;

namespace Poker
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        // =========================================================
        // 🎧 MAIN SOURCES
        // =========================================================

        [Header("Drone Sources")]
        [SerializeField] private AudioSource droneA;
        [SerializeField] private AudioSource droneB;

        [Header("Dynamic Sources")]
        [SerializeField] private AudioSource textureSource;
        [SerializeField] private AudioSource hitSource;
        [SerializeField] private AudioSource noiseSource;

        [Header("Gameplay Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        // =========================================================
        // 🔊 VOLUMES
        // =========================================================

        [Header("Volumes")]

        [Range(0f, 1f)]
        [SerializeField] private float droneVolume = 0.22f;

        [Range(0f, 1f)]
        [SerializeField] private float textureVolume = 0.14f;

        [Range(0f, 1f)]
        [SerializeField] private float noiseVolume = 0.05f;

        [Range(0f, 1f)]
        [SerializeField] private float hitVolume = 0.45f;

        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.8f;

        [Range(0f, 1f)]
        [SerializeField] private float uiVolume = 0.75f;

        // =========================================================
        // ⏱ TRANSITIONS
        // =========================================================

        [Header("Transitions")]

        [SerializeField] private float droneFadeTime = 1f;

        // =========================================================
        // 🎭 EMOTION LAYERS
        // =========================================================

        [System.Serializable]
        public class EmotionLayer
        {
            [Header("MAIN DRONE")]
            public AudioClip drone;

            [Header("TEXTURES")]
            public AudioClip[] textures;

            [Header("HITS")]
            public AudioClip[] hits;

            [Header("NOISE")]
            public AudioClip[] noises;

            [Header("TEXTURE SETTINGS")]
            public float textureDelayMin = 8f;
            public float textureDelayMax = 15f;

            [Range(0f, 1f)]
            public float textureChance = 0.4f;

            [Header("NOISE SETTINGS")]
            public float noiseDelayMin = 15f;
            public float noiseDelayMax = 25f;

            [Range(0f, 1f)]
            public float noiseChance = 0.25f;
        }

        // =========================================================
        // 🎭 STATES
        // =========================================================

        public enum TensionState
        {
            Calm,
            Nervous,
            Panic,
            Focus,
            Suspicious,
            Greedy,
            Aggressive,
            BluffCalm,
            BluffNervous,
            Unhinged
        }

        // =========================================================
        // 🎭 EMOTION CONFIGS
        // =========================================================

        [Header("Emotion Layers")]

        [SerializeField] private EmotionLayer calm;
        [SerializeField] private EmotionLayer nervous;
        [SerializeField] private EmotionLayer panic;
        [SerializeField] private EmotionLayer focus;
        [SerializeField] private EmotionLayer suspicious;
        [SerializeField] private EmotionLayer greedy;
        [SerializeField] private EmotionLayer aggressive;
        [SerializeField] private EmotionLayer bluffCalm;
        [SerializeField] private EmotionLayer bluffNervous;
        [SerializeField] private EmotionLayer unhinged;

        // =========================================================
        // 🃏 CARD SFX
        // =========================================================

        [Header("Card SFX")]

        [SerializeField] private AudioClip cardDeal;
        [SerializeField] private AudioClip cardDiscard;

        // =========================================================
        // ❤️ HP
        // =========================================================

        [Header("HP Sounds")]

        [SerializeField] private AudioClip hpLose;
        [SerializeField] private AudioClip hpGain;

        // =========================================================
        // UI
        // =========================================================

        [Header("UI")]

        [SerializeField] private AudioClip buttonClick;

        // =========================================================
        // INTERNAL
        // =========================================================

        private AudioSource currentDrone;
        private AudioSource nextDrone;

        private Coroutine textureRoutine;
        private Coroutine noiseRoutine;

        private TensionState currentState;

        // =========================================================
        // INIT
        // =========================================================

        private void Awake()
        {
            Instance = this;

            currentDrone = droneA;
            nextDrone = droneB;
        }

        private void Start()
        {
            SetTensionState(TensionState.Calm);
        }
        // =========================================================
        // 🖱 HOVER
        // =========================================================

        public void PlayButtonHover()
        {
            // можно оставить пустым
        }

        // =========================================================
        // 🎭 CHANGE STATE
        // =========================================================

        public void SetTensionState(TensionState state)
        {
            if (currentState == state)
            {
                PlayEmotionHit();
                return;
            }

            currentState = state;

            EmotionLayer layer = GetLayer(state);

            if (layer == null)
                return;

            StartCoroutine(CrossfadeDrone(layer));

            if (textureRoutine != null)
                StopCoroutine(textureRoutine);

            if (noiseRoutine != null)
                StopCoroutine(noiseRoutine);

            textureRoutine =
                StartCoroutine(TextureLoop(layer));

            noiseRoutine =
                StartCoroutine(NoiseLoop(layer));

            PlayEmotionHit();
        }

        // =========================================================
        // 🎼 DRONE CROSSFADE
        // =========================================================

        private IEnumerator CrossfadeDrone(EmotionLayer layer)
        {
            if (layer.drone == null)
                yield break;

            nextDrone.clip = layer.drone;
            nextDrone.loop = true;
            nextDrone.volume = 0f;

            nextDrone.Play();

            float t = 0f;

            while (t < droneFadeTime)
            {
                t += Time.deltaTime;

                float k = t / droneFadeTime;

                currentDrone.volume =
                    Mathf.Lerp(droneVolume, 0f, k);

                nextDrone.volume =
                    Mathf.Lerp(0f, droneVolume, k);

                yield return null;
            }

            currentDrone.Stop();

            AudioSource temp = currentDrone;
            currentDrone = nextDrone;
            nextDrone = temp;
        }

        // =========================================================
        // 🌫 TEXTURES
        // =========================================================

        private IEnumerator TextureLoop(EmotionLayer layer)
        {
            while (true)
            {
                yield return new WaitForSeconds(
                    Random.Range(
                        layer.textureDelayMin,
                        layer.textureDelayMax
                    )
                );

                if (Random.value > layer.textureChance)
                    continue;

                AudioClip clip =
                    GetRandom(layer.textures);

                if (clip == null)
                    continue;

                textureSource.pitch =
                    Random.Range(0.96f, 1.04f);

                textureSource.panStereo =
                    Random.Range(-0.25f, 0.25f);

                textureSource.PlayOneShot(
                    clip,
                    textureVolume
                );
            }
        }

        // =========================================================
        // 🌫 NOISES
        // =========================================================

        private IEnumerator NoiseLoop(EmotionLayer layer)
        {
            while (true)
            {
                yield return new WaitForSeconds(
                    Random.Range(
                        layer.noiseDelayMin,
                        layer.noiseDelayMax
                    )
                );

                if (Random.value > layer.noiseChance)
                    continue;

                AudioClip clip =
                    GetRandom(layer.noises);

                if (clip == null)
                    continue;

                noiseSource.pitch =
                    Random.Range(0.97f, 1.03f);

                noiseSource.panStereo =
                    Random.Range(-0.2f, 0.2f);

                noiseSource.PlayOneShot(
                    clip,
                    noiseVolume
                );
            }
        }

        // =========================================================
        // 🔥 EMOTION HIT
        // =========================================================

        public void PlayEmotionHit()
        {
            EmotionLayer layer =
                GetLayer(currentState);

            if (layer == null)
                return;

            AudioClip clip =
                GetRandom(layer.hits);

            if (clip == null)
                return;

            hitSource.pitch =
                Random.Range(0.95f, 1.05f);

            hitSource.PlayOneShot(
                clip,
                hitVolume
            );
        }

        // =========================================================
        // 🃏 CARD DEAL
        // =========================================================

        public void PlayCardDeal()
        {
            if (cardDeal == null)
                return;

            sfxSource.pitch =
                Random.Range(0.97f, 1.03f);

            sfxSource.PlayOneShot(
                cardDeal,
                sfxVolume
            );
        }

        // =========================================================
        // 🃏 CARD DISCARD
        // =========================================================

        public void PlayCardDiscard()
        {
            if (cardDiscard == null)
                return;

            sfxSource.pitch =
                Random.Range(0.95f, 1.02f);

            sfxSource.PlayOneShot(
                cardDiscard,
                sfxVolume
            );
        }

        // =========================================================
        // ❤️ HP LOSE
        // =========================================================

        public void PlayHpLose()
        {
            if (hpLose == null)
                return;

            sfxSource.pitch =
                Random.Range(0.88f, 0.98f);

            sfxSource.PlayOneShot(
                hpLose,
                sfxVolume
            );
        }

        // =========================================================
        // 💚 HP GAIN
        // =========================================================

        public void PlayHpGain()
        {
            if (hpGain == null)
                return;

            sfxSource.pitch =
                Random.Range(1f, 1.06f);

            sfxSource.PlayOneShot(
                hpGain,
                sfxVolume
            );
        }

        // =========================================================
        // 🖱 CLICK
        // =========================================================

        public void PlayButtonClick()
        {
            if (buttonClick == null)
                return;

            uiSource.PlayOneShot(
                buttonClick,
                uiVolume
            );
        }

        // =========================================================
        // 🧠 GET EMOTION LAYER
        // =========================================================

        private EmotionLayer GetLayer(
            TensionState state)
        {
            switch (state)
            {
                case TensionState.Calm:
                    return calm;

                case TensionState.Nervous:
                    return nervous;

                case TensionState.Panic:
                    return panic;

                case TensionState.Focus:
                    return focus;

                case TensionState.Suspicious:
                    return suspicious;

                case TensionState.Greedy:
                    return greedy;

                case TensionState.Aggressive:
                    return aggressive;

                case TensionState.BluffCalm:
                    return bluffCalm;

                case TensionState.BluffNervous:
                    return bluffNervous;

                case TensionState.Unhinged:
                    return unhinged;
            }

            return calm;
        }

        // =========================================================
        // 🎲 RANDOM
        // =========================================================

        private AudioClip GetRandom(AudioClip[] array)
        {
            if (array == null || array.Length == 0)
                return null;

            return array[
                Random.Range(0, array.Length)
            ];
        }
        // =========================================================
        // ⏸ PAUSE ALL
        // =========================================================

        public void PauseAll()
        {
            if (droneA != null)
                droneA.Pause();

            if (droneB != null)
                droneB.Pause();

            if (textureSource != null)
                textureSource.Pause();

            if (noiseSource != null)
                noiseSource.Pause();

            if (hitSource != null)
                hitSource.Pause();

            if (sfxSource != null)
                sfxSource.Pause();

            if (uiSource != null)
                uiSource.Pause();
        }

        // =========================================================
        // ▶ RESUME ALL
        // =========================================================

        public void ResumeAll()
        {
            if (droneA != null)
                droneA.UnPause();

            if (droneB != null)
                droneB.UnPause();

            if (textureSource != null)
                textureSource.UnPause();

            if (noiseSource != null)
                noiseSource.UnPause();

            if (hitSource != null)
                hitSource.UnPause();

            if (sfxSource != null)
                sfxSource.UnPause();

            if (uiSource != null)
                uiSource.UnPause();
        }

        // =========================================================
        // 🛑 STOP ALL
        // =========================================================

        public void StopAll()
        {
            if (droneA != null)
                droneA.Stop();

            if (droneB != null)
                droneB.Stop();

            if (textureSource != null)
                textureSource.Stop();

            if (noiseSource != null)
                noiseSource.Stop();

            if (hitSource != null)
                hitSource.Stop();

            if (sfxSource != null)
                sfxSource.Stop();

            if (uiSource != null)
                uiSource.Stop();
        }
    }
}