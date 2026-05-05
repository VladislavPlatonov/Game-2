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
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource tensionSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;

        // =========================================================
        // 🔊 VOLUME SETTINGS
        // =========================================================
        [Header("Volume")]
        [Range(0f, 1f)] public float ambientVolume = 0.4f;
        [Range(0f, 1f)] public float tensionVolume = 0.6f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float uiVolume = 0.7f;

        // =========================================================
        // 🌫 AMBIENT
        // =========================================================
        [Header("Ambient")]
        [SerializeField] private AudioClip[] ambientLoops;

        // =========================================================
        // 🎭 ВСЕ ЭМОЦИИ
        // =========================================================
        [Header("Tension States")]

        [SerializeField] private AudioClip[] calmSounds;
        [SerializeField] private AudioClip[] nervousSounds;
        [SerializeField] private AudioClip[] panicSounds;
        [SerializeField] private AudioClip[] focusSounds;
        [SerializeField] private AudioClip[] suspiciousSounds;
        [SerializeField] private AudioClip[] greedySounds;
        [SerializeField] private AudioClip[] aggressiveSounds;
        [SerializeField] private AudioClip[] bluffCalmSounds;
        [SerializeField] private AudioClip[] bluffNervousSounds;
        [SerializeField] private AudioClip[] unhingedSounds;

        // =========================================================
        // 🃏 SFX
        // =========================================================
        [Header("SFX")]

        [SerializeField] private AudioClip cardDeal;
        [SerializeField] private AudioClip cardDiscard;

        // =========================================================
        // UI
        // =========================================================
        [Header("UI")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip buttonHover;

        // =========================================================
        // 🎭 STATE
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
        private Coroutine tensionRoutine;

        // =========================================================
        // INIT
        // =========================================================
        private void Awake()
        {
            Instance = this;
            PlayAmbient();
        }

        // =========================================================
        // 🌫 AMBIENT
        // =========================================================
        public void PlayAmbient()
        {
            if (ambientLoops.Length == 0) return;

            ambientSource.clip = GetRandom(ambientLoops);
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
            ambientSource.Play();
        }// =========================================================
        // 🎭 СМЕНА СОСТОЯНИЯ
        // =========================================================
        public void SetTensionState(TensionState newState)
        {
            if (currentState == newState)
                return;

            currentState = newState;

            if (tensionRoutine != null)
                StopCoroutine(tensionRoutine);

            tensionRoutine = StartCoroutine(TensionLoop());
        }

        // =========================================================
        // 🎧 LOOP С ПЛАВНЫМ ПЕРЕХОДОМ
        // =========================================================
        private IEnumerator TensionLoop()
        {
            while (true)
            {
                AudioClip clip = GetClipByState(currentState);

                if (clip == null)
                    yield break;

                yield return StartCoroutine(FadeOut(tensionSource, 0.3f));

                tensionSource.clip = clip;
                tensionSource.volume = 0f;
                tensionSource.Play();

                yield return StartCoroutine(FadeIn(tensionSource, 0.4f));

                yield return new WaitForSeconds(clip.length - 0.3f);
            }
        }

        // =========================================================
        // 🎯 ВЫБОР КЛИПА
        // =========================================================
        private AudioClip GetClipByState(TensionState state)
        {
            switch (state)
            {
                case TensionState.Calm: return GetRandom(calmSounds);
                case TensionState.Nervous: return GetRandom(nervousSounds);
                case TensionState.Panic: return GetRandom(panicSounds);
                case TensionState.Focus: return GetRandom(focusSounds);
                case TensionState.Suspicious: return GetRandom(suspiciousSounds);
                case TensionState.Greedy: return GetRandom(greedySounds);
                case TensionState.Aggressive: return GetRandom(aggressiveSounds);
                case TensionState.BluffCalm: return GetRandom(bluffCalmSounds);
                case TensionState.BluffNervous: return GetRandom(bluffNervousSounds);
                case TensionState.Unhinged: return GetRandom(unhingedSounds);
            }

            return null;
        }

        // =========================================================
        // 🎚 FADE
        // =========================================================
        private IEnumerator FadeOut(AudioSource source, float time)
        {
            float start = source.volume;

            for (float t = 0; t < time; t += Time.deltaTime)
            {
                source.volume = Mathf.Lerp(start, 0f, t / time);
                yield return null;
            }

            source.volume = 0f;
        }

        private IEnumerator FadeIn(AudioSource source, float time)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                source.volume = Mathf.Lerp(0f, tensionVolume, t / time);
                yield return null;
            }

            source.volume = tensionVolume;
        }

        // =========================================================
        // 🃏 SFX
        // =========================================================
        public void PlayCardDeal()
        {
            if (cardDeal == null) return;

            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(cardDeal, sfxVolume);
        }

        public void PlayCardDiscard()
        {
            if (cardDiscard == null) return;

            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(cardDiscard, sfxVolume);
        }// =========================================================
        // 🖱 UI
        // =========================================================
        public void PlayButtonClick()
        {
            if (buttonClick != null)
                uiSource.PlayOneShot(buttonClick, uiVolume);
        }

        public void PlayButtonHover()
        {
            if (buttonHover != null)
                uiSource.PlayOneShot(buttonHover, uiVolume);
        }

        // =========================================================
        // 🎲 RANDOM
        // =========================================================
        private AudioClip GetRandom(AudioClip[] array)
        {
            if (array == null || array.Length == 0)
                return null;

            return array[Random.Range(0, array.Length)];
        }
        // =========================================================
        // ⏸️ PAUSE / RESUME
        // =========================================================
        public void PauseAll()
        {
            if (ambientSource != null) ambientSource.Pause();
            if (tensionSource != null) tensionSource.Pause();
        }

        public void ResumeAll()
        {
            if (ambientSource != null) ambientSource.UnPause();
            if (tensionSource != null) tensionSource.UnPause();
        }

        // =========================================================
        // 🛑 STOP ALL (КОНЕЦ ИГРЫ)
        // =========================================================
        public void StopAll()
        {
            if (ambientSource != null)
                ambientSource.Stop();

            if (tensionSource != null)
                tensionSource.Stop();

            if (sfxSource != null)
                sfxSource.Stop();
        }
    }
}