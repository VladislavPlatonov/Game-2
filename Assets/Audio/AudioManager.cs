using System.Collections;
using UnityEngine;

namespace Poker
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        // =========================================================
        // 🔊 AUDIO SOURCES
        // =========================================================
        [Header("Audio Sources")]

        [SerializeField] private AudioSource droneSource;
        [SerializeField] private AudioSource textureSource;
        [SerializeField] private AudioSource hitSource;
        [SerializeField] private AudioSource noiseSource;

        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        // =========================================================
        // 🔊 VOLUME
        // =========================================================
        [Header("Layer Volumes")]

        [Range(0f, 1f)]
        [SerializeField] private float droneVolume = 0.18f;

        [Range(0f, 1f)]
        [SerializeField] private float textureVolume = 0.12f;

        [Range(0f, 1f)]
        [SerializeField] private float hitVolume = 0.35f;

        [Range(0f, 1f)]
        [SerializeField] private float noiseVolume = 0.08f;

        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.8f;

        [Range(0f, 1f)]
        [SerializeField] private float uiVolume = 0.7f;

        // =========================================================
        // 🎭 EMOTION LAYERS
        // =========================================================

        [System.Serializable]
        public class EmotionLayer
        {
            [Header("Loop Drones")]
            public AudioClip[] drones;

            [Header("Textures")]
            public AudioClip[] textures;

            [Header("Hits")]
            public AudioClip[] hits;

            [Header("Noises")]
            public AudioClip[] noises;
        }

        [Header("Emotion Layers")]

        [SerializeField] private EmotionLayer calmLayer;
        [SerializeField] private EmotionLayer nervousLayer;
        [SerializeField] private EmotionLayer panicLayer;
        [SerializeField] private EmotionLayer focusLayer;
        [SerializeField] private EmotionLayer suspiciousLayer;
        [SerializeField] private EmotionLayer greedyLayer;
        [SerializeField] private EmotionLayer aggressiveLayer;
        [SerializeField] private EmotionLayer bluffCalmLayer;
        [SerializeField] private EmotionLayer bluffNervousLayer;
        [SerializeField] private EmotionLayer unhingedLayer;

        // =========================================================
        // 🃏 SFX
        // =========================================================
        [Header("Card Sounds")]

        [SerializeField] private AudioClip cardDeal;
        [SerializeField] private AudioClip cardDiscard;

        [Header("HP Sounds")]

        [SerializeField] private AudioClip hpLose;
        [SerializeField] private AudioClip hpGain;

        // =========================================================
        // 🖱 UI
        // =========================================================
        [Header("UI")]

        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip buttonHover;

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

        private TensionState currentState;

        // =========================================================
        // INIT
        // =========================================================
        private void Awake()
        {
            Instance = this;
        }

        // =========================================================
        // 🎭 CHANGE STATE
        // =========================================================
        public void SetTensionState(TensionState newState)
        {
            currentState = newState;

            StopAllCoroutines();

            StopEmotionAudio();

            EmotionLayer layer = GetLayerByState(newState);

            if (layer == null)
                return;

            // DRONE LOOP
            StartLayerLoop(
                droneSource,
                layer.drones,
                droneVolume,
                true
            );

            // RANDOM TEXTURES
            StartCoroutine(RandomLoop(
                textureSource,
                layer.textures,
                textureVolume,
                5f,
                12f
            ));

            // RANDOM HITS
            StartCoroutine(RandomLoop(
                hitSource,
                layer.hits,
                hitVolume,
                8f,
                18f
            ));

            // RANDOM NOISES
            StartCoroutine(RandomLoop(
                noiseSource,
                layer.noises,
                noiseVolume,
                10f,
                25f
            ));
        }

        // =========================================================
        // 🎧 START LOOP
        // =========================================================
        private void StartLayerLoop(
            AudioSource source,
            AudioClip[] clips,
            float volume,
            bool loop)
        {
            if (source == null)
                return;

            if (clips == null || clips.Length == 0)
                return;

            source.Stop();

            source.clip = GetRandom(clips);

            source.loop = loop;

            source.volume = volume;

            source.pitch = 1f;

            source.panStereo = 0f;

            source.Play();
        }

        // =========================================================
        // 🔀 RANDOM LOOP
        // =========================================================
        private IEnumerator RandomLoop(
            AudioSource source,
            AudioClip[] clips,
            float volume,
            float minDelay,
            float maxDelay)
        {
            if (source == null)
                yield break;

            if (clips == null || clips.Length == 0)
                yield break;

            while (true)
            {
                yield return new WaitForSeconds(
                    Random.Range(minDelay, maxDelay)
                );

                AudioClip clip = GetRandom(clips);

                if (clip == null)
                    continue;

                source.volume = volume;

                source.pitch = Random.Range(0.96f, 1.04f);

                source.panStereo = Random.Range(-0.35f, 0.35f);

                source.PlayOneShot(clip);
            }
        }

        // =========================================================
        // 🧠 GET LAYER
        // =========================================================
        private EmotionLayer GetLayerByState(TensionState state)
        {
            switch (state)
            {
                case TensionState.Calm:
                    return calmLayer;

                case TensionState.Nervous:
                    return nervousLayer;

                case TensionState.Panic:
                    return panicLayer;

                case TensionState.Focus:
                    return focusLayer;

                case TensionState.Suspicious:
                    return suspiciousLayer;

                case TensionState.Greedy:
                    return greedyLayer;

                case TensionState.Aggressive:
                    return aggressiveLayer;

                case TensionState.BluffCalm:
                    return bluffCalmLayer;

                case TensionState.BluffNervous:
                    return bluffNervousLayer;

                case TensionState.Unhinged:
                    return unhingedLayer;
            }

            return null;
        }

        // =========================================================
        // 🃏 CARD DEAL
        // =========================================================
        public void PlayCardDeal()
        {
            if (cardDeal == null)
                return;

            sfxSource.pitch = Random.Range(0.95f, 1.05f);

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

            sfxSource.pitch = Random.Range(0.94f, 1.02f);

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

            sfxSource.pitch = Random.Range(0.92f, 1.02f);

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

            sfxSource.pitch = Random.Range(0.98f, 1.08f);

            sfxSource.PlayOneShot(
                hpGain,
                sfxVolume
            );
        }

        // =========================================================
        // 🖱 BUTTON CLICK
        // =========================================================
        public void PlayButtonClick()
        {
            if (buttonClick == null)
                return;

            uiSource.pitch = Random.Range(0.98f, 1.02f);

            uiSource.PlayOneShot(
                buttonClick,
                uiVolume
            );
        }

        // =========================================================
        // 🖱 BUTTON HOVER
        // =========================================================
        public void PlayButtonHover()
        {
            if (buttonHover == null)
                return;

            uiSource.pitch = Random.Range(0.98f, 1.02f);

            uiSource.PlayOneShot(
                buttonHover,
                uiVolume
            );
        }

        // =========================================================
        // ⏸️ PAUSE
        // =========================================================
        public void PauseAll()
        {
            if (droneSource != null)
                droneSource.Pause();

            if (textureSource != null)
                textureSource.Pause();

            if (hitSource != null)
                hitSource.Pause();

            if (noiseSource != null)
                noiseSource.Pause();
        }

        // =========================================================
        // ▶ RESUME
        // =========================================================
        public void ResumeAll()
        {
            if (droneSource != null)
                droneSource.UnPause();

            if (textureSource != null)
                textureSource.UnPause();

            if (hitSource != null)
                hitSource.UnPause();

            if (noiseSource != null)
                noiseSource.UnPause();
        }

        // =========================================================
        // 🛑 STOP ALL
        // =========================================================
        public void StopAll()
        {
            StopAllCoroutines();

            StopEmotionAudio();

            if (sfxSource != null)
                sfxSource.Stop();
        }

        // =========================================================
        // 🛑 STOP EMOTION AUDIO
        // =========================================================
        private void StopEmotionAudio()
        {
            if (droneSource != null)
                droneSource.Stop();

            if (textureSource != null)
                textureSource.Stop();

            if (hitSource != null)
                hitSource.Stop();

            if (noiseSource != null)
                noiseSource.Stop();
        }

        // =========================================================
        // 🎲 RANDOM
        // =========================================================
        private AudioClip GetRandom(AudioClip[] array)
        {
            if (array == null)
                return null;

            if (array.Length == 0)
                return null;

            return array[
                Random.Range(0, array.Length)
            ];
        }
    }
}