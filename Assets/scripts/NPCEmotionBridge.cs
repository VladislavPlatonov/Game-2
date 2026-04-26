//Этот скрипт отвечает за плавное и безопасное переключение эмоций.

using UnityEngine;

namespace Poker
{
    public class NPCEmotionBridge : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private EnemyHeadCalmController headController;
        [SerializeField] private AIHandEmotionController handController;

        [Header("Switching")]
        [SerializeField] private float minEmotionDuration = 1.5f;

        private EnemyHeadCalmController.EmotionState currentEmotion;
        private float lastEmotionSwitchTime = -999f;

        private void Awake()
        {
            if (headController == null)
                headController = FindFirstObjectByType<EnemyHeadCalmController>();

            if (handController == null)
                handController = FindFirstObjectByType<AIHandEmotionController>();

            currentEmotion = EnemyHeadCalmController.EmotionState.Calm;
        }

        public void SetEmotion(EnemyHeadCalmController.EmotionState emotion, bool force = false)
        {
            if (!force)
            {
                if (emotion == currentEmotion)
                    return;

                if (Time.time - lastEmotionSwitchTime < minEmotionDuration)
                    return;
            }

            currentEmotion = emotion;
            lastEmotionSwitchTime = Time.time;

            if (headController != null)
                headController.SetEmotion(emotion);

            if (handController == null)
                return;

            switch (emotion)
            {
                case EnemyHeadCalmController.EmotionState.Calm:
                    handController.SetStateCalm();
                    break;

                case EnemyHeadCalmController.EmotionState.Nervous:
                    handController.SetStateNervous();
                    break;

                case EnemyHeadCalmController.EmotionState.Panic:
                    handController.SetStatePanic();
                    break;

                case EnemyHeadCalmController.EmotionState.Focus:
                    handController.SetStateFocus();
                    break;

                case EnemyHeadCalmController.EmotionState.Suspicious:
                    handController.SetStateSuspicious();
                    break;

                case EnemyHeadCalmController.EmotionState.Greedy:
                    handController.SetStateGreedy();
                    break;

                case EnemyHeadCalmController.EmotionState.Aggressive:
                    handController.SetStateAggressive();
                    break;

                case EnemyHeadCalmController.EmotionState.BluffCalm:
                    handController.SetStateBluffCalm();
                    break;

                case EnemyHeadCalmController.EmotionState.BluffNervous:
                    handController.SetStateBluffNervous();
                    break;

                case EnemyHeadCalmController.EmotionState.Unhinged:
                    handController.SetStateUnhinged();
                    break;
            }
        }
    }
}