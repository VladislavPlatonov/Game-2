using UnityEngine;
using Unity.InferenceEngine; // библиотека для ONNX

namespace Poker
{
    public class NPCHybridONNXRunner : MonoBehaviour
    {
        // ================================
        // НАСТРОЙКИ В ИНСПЕКТОРЕ
        // ================================

        [Header("Model")]
        [SerializeField] private ModelAsset modelAsset;
        [SerializeField] private BackendType backendType = BackendType.CPU;

        // ================================
        // ВНУТРЕННИЕ ПЕРЕМЕННЫЕ
        // ================================

        private Worker worker;
        private Model runtimeModel;

        // 🔥 ВАЖНО: теперь 6 (как в train_model.py)
        private const int FeatureCount = 7;

        // ================================
        // ИНИЦИАЛИЗАЦИЯ
        // ================================

        private void Awake()
        {
            if (modelAsset == null)
            {
                Debug.LogError("[ONNX Runner] ❌ ModelAsset не назначен!");
                return;
            }

            runtimeModel = ModelLoader.Load(modelAsset);
            worker = new Worker(runtimeModel, backendType);

            Debug.Log("[ONNX Runner] ✅ Модель загружена");
        }

        // ================================
        // ОСНОВНОЙ МЕТОД
        // ================================

        public NPCHybridBrainResult Predict(NPCHybridBrainInput input)
        {
            if (worker == null)
            {
                Debug.LogWarning("[ONNX Runner] Worker не создан!");

                return new NPCHybridBrainResult
                {
                    action = PlayerActionType.Check,
                    emotion = EnemyHeadCalmController.EmotionState.Calm,
                    dialogueIntent = NPCDialogueIntent.None
                };
            }

            // ============================
            // СОБИРАЕМ ФИЧИ
            // ============================

            float[] features = BuildFeatures(input);

            Debug.Log("FEATURE COUNT: " + features.Length);

            // ============================
            // СОЗДАЁМ TENSOR
            // ============================

            using Tensor<float> inputTensor =
                new Tensor<float>(new TensorShape(1, FeatureCount), features);

            // ============================
            // ЗАПУСК МОДЕЛИ
            // ============================

            worker.Schedule(inputTensor);

            // 🔥 берём именно эмоции из модели
            Tensor<float> emotionTensor = worker.PeekOutput("emotion_logits") as Tensor<float>;

            // читаем данные (обязательно!)
            var readable = emotionTensor.ReadbackAndClone();

            // создаём массив
            float[] output = new float[readable.shape.length];

            // копируем значения
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = readable[i];
            }

            // DEBUG
            Debug.Log("OUTPUT SIZE: " + output.Length);

            // ============================
            // DEBUG ВЫХОДА
            // ============================

            string debug = "MODEL OUTPUT: ";

            for (int i = 0; i < output.Length; i++)
            {
                debug += $"[{i}:{output[i]:F2}] ";
            }

            Debug.Log(debug);

            // ============================
            // ВЫБОР ЭМОЦИИ
            // ============================

            int emotionIndex = SampleFromSoftmax(output);

            // ======================================
            // 🧠 КОРРЕКЦИЯ ЭМОЦИЙ (ЛОГИКА ПОВЕРХ ИИ)
            // ======================================

            // 🔹 Если рука сильная и риск низкий — НЕ может быть Nervous
            if (input.aiHandStrength > 0.65f && input.riskLevel < 0.3f)
            {
                if (emotionIndex == (int)EnemyHeadCalmController.EmotionState.Nervous)
                {
                    emotionIndex = (int)EnemyHeadCalmController.EmotionState.Focus;
                    Debug.Log("🛠 Nervous → Focus (сильная рука, низкий риск)");
                }
            }

            // 🔹 Если рука слабая и риск высокий — НЕ может быть Calm
            if (input.aiHandStrength < 0.35f && input.riskLevel > 0.7f)
            {
                if (emotionIndex == (int)EnemyHeadCalmController.EmotionState.Calm)
                {
                    emotionIndex = (int)EnemyHeadCalmController.EmotionState.Panic;
                    Debug.Log("🛠 Calm → Panic (слабая рука, высокий риск)");
                }
            }

            // 🔹 Если сильная рука — не должно быть Panic
            if (input.aiHandStrength > 0.7f)
            {
                if (emotionIndex == (int)EnemyHeadCalmController.EmotionState.Panic)
                {
                    emotionIndex = (int)EnemyHeadCalmController.EmotionState.Greedy;
                    Debug.Log("🛠 Panic → Greedy (сильная рука)");
                }
            }

            // 🔹 Greedy нельзя при слабой руке
            if (input.aiHandStrength < 0.6f)
            {
                if (emotionIndex == (int)EnemyHeadCalmController.EmotionState.Greedy)
                {
                    emotionIndex = (int)EnemyHeadCalmController.EmotionState.Suspicious;
                    Debug.Log("🛠 Greedy → Suspicious (слабая рука)");
                }
            }

            // 🔹 Если высокий bluff — усиливаем шанс блефа
            if (input.bluffFactor > 0.7f && input.aiHandStrength < 0.5f)
            {
                if (UnityEngine.Random.value < 0.5f)
                {
                    emotionIndex = (int)EnemyHeadCalmController.EmotionState.BluffCalm;
                    Debug.Log("🎭 Принудительный BluffCalm");
                }
            }

            Debug.Log("🎯 Emotion index: " + emotionIndex);
            Debug.Log("🧠 Emotion: " + ((EnemyHeadCalmController.EmotionState)emotionIndex));

            // ============================
            // РЕЗУЛЬТАТ
            // ============================

            return new NPCHybridBrainResult
            {
                action = PlayerActionType.Call,

                emotion = (EnemyHeadCalmController.EmotionState)emotionIndex,

                dialogueIntent = MapEmotionToDialogue(emotionIndex),

                confidence = 0.8f,
                dialogueChance = 0.5f
            };
        }

        // ================================
        // ФИЧИ (САМОЕ ВАЖНОЕ)
        // ================================

        private float[] BuildFeatures(NPCHybridBrainInput i)
        {
            // 🔥 ДОЛЖНО СОВПАДАТЬ С train_model.py

            return new float[]
            {
                i.aiHandStrength,       // 1
                i.riskLevel,            // 2
                i.potPressure,          // 3
                i.playerAggression,     // 4
                i.aiHpNormalized,       // 5 🔥
                i.playerHpNormalized,    // 6 🔥
                i.bluffFactor // 🔥 ОБЯЗАТЕЛЬНО
            };
        }

        // ================================
        // ARGMAX
        // ================================

        private int ArgMax(float[] values)
        {
            int index = 0;
            float best = values[0];

            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > best)
                {
                    best = values[i];
                    index = i;
                }
            }

            return index;
        }

        private int SampleFromSoftmax(float[] logits)
        {
            // 🔥 находим максимум (для стабильности)
            float max = logits[0];
            for (int i = 1; i < logits.Length; i++)
                if (logits[i] > max)
                    max = logits[i];

            // 🔥 считаем softmax
            float sum = 0f;
            float[] probs = new float[logits.Length];

            for (int i = 0; i < logits.Length; i++)
            {
                float temperature = 0.4f; // 🔥 ключ
                probs[i] = Mathf.Exp((logits[i] - max) / temperature);
                sum += probs[i];
            }

            // нормализация
            for (int i = 0; i < probs.Length; i++)
                probs[i] /= sum;

            // 🔥 случайный выбор
            float r = UnityEngine.Random.value;
            float cumulative = 0f;

            for (int i = 0; i < probs.Length; i++)
            {
                cumulative += probs[i];
                if (r <= cumulative)
                    return i;
            }

            return probs.Length - 1;
        }

        // ================================
        // ЭМОЦИЯ → ДИАЛОГ
        // ================================

        private NPCDialogueIntent MapEmotionToDialogue(int emotion)
        {
            switch (emotion)
            {
                case 1: return NPCDialogueIntent.NervousLie;
                case 2: return NPCDialogueIntent.NervousLie;

                case 3: return NPCDialogueIntent.CalmComment;

                case 4: return NPCDialogueIntent.SuspiciousQuestion;

                case 5: return NPCDialogueIntent.ConfidentTaunt;
                case 6: return NPCDialogueIntent.AggressiveThreat;

                case 7: return NPCDialogueIntent.CalmComment;
                case 8: return NPCDialogueIntent.NervousLie;

                case 9: return NPCDialogueIntent.KnifeRemark;

                default: return NPCDialogueIntent.CalmComment;
            }
        }

        private void OnDestroy()
        {
            worker?.Dispose();
        }
    }
}