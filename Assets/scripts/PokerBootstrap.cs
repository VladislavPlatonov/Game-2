using UnityEngine;

namespace Poker
{
    public class PokerBootstrap : MonoBehaviour
    {
        public static PokerBootstrap Instance { get; private set; }

        [Header("Cursor while playing")]
        [SerializeField] private bool lockCursorWhenPlaying = false; // поставь true если хочешь скрывать/локать курсор в игре

        private bool paused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // стартуем "в игре" (не в паузе)
            SetPaused(false);
        }

        /// <summary>
        /// Вызывать из PauseMenuController при открытии/закрытии паузы.
        /// </summary>
        public void SetPaused(bool isPaused)
        {
            paused = isPaused;

            Time.timeScale = paused ? 0f : 1f;

            ApplyCursorState();
        }

        private void ApplyCursorState()
        {
            if (paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (lockCursorWhenPlaying)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // когда кликаешь в окно игры / теряешь фокус — курсор часто меняется,
            // поэтому восстанавливаем правильное состояние
            ApplyCursorState();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            ApplyCursorState();
        }
    }
}