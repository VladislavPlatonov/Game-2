using UnityEngine;
using Unity.InferenceEngine; // пакет для запуска нейросети

namespace Poker
{
    public class NPCHybridONNXRunner : MonoBehaviour
    {
        [Header("Model")]
        [SerializeField] private ModelAsset modelAsset; // сюда перетаскиваем .onnx
        [SerializeField] private BackendType backendType = BackendType.CPU; // CPU или GPU

        private Worker worker; // исполнитель модели
        private Model runtimeModel; // загруженная модель

        private const int FeatureCount = 18; // столько входных параметров

        private void Awake()
        {
            // Проверяем, что модель назначена
            if (modelAsset == null)
            {
                Debug.LogError("[ONNX Runner] ModelAsset не назначен.");
                return;
            }

            // Загружаем модель из .onnx
            runtimeModel = ModelLoader.Load(modelAsset);

            // Создаём worker (движок выполнения)
            worker = new Worker(runtimeModel, backendType);

            Debug.Log("[ONNX Runner] Модель загружена.");
        }

        public NPCHybridBrainResult Predict(NPCHybridBrainInput input)
        {
            // Если модель не загрузилась — возвращаем fallback
            if (worker == null)
            {
                Debug.LogWarning("[ONNX Runner] Worker не создан.");

                return new NPCHybridBrainResult
                {
                    action = PlayerActionType.Check,
                    emotion = EnemyHeadCalmController.EmotionState.Calm,
                    dialogueIntent = NPCDialogueIntent.None
                };
            }

            // Превращаем входные данные в массив float
            float[] features = BuildFeatures(input);

            // Создаём тензор (формат для нейросети)
            using Tensor<float> inputTensor =
                new Tensor<float>(new TensorShape(1, FeatureCount), features);

            // Запускаем модель
            worker.Schedule(inputTensor);

            // Получаем выходы
            Tensor<float> actionTensor = worker.PeekOutput("action_logits") as Tensor<float>;
            Tensor<float> emotionTensor = worker.PeekOutput("emotion_logits") as Tensor<float>;
            Tensor<float> dialogueTensor = worker.PeekOutput("dialogue_logits") as Tensor<float>;

            // Переводим в массивы
            float[] action = actionTensor.DownloadToArray();
            float[] emotion = emotionTensor.DownloadToArray();
            float[] dialogue = dialogueTensor.DownloadToArray();

            // Берём максимальные значения (самый вероятный класс)
            int actionIndex = ArgMax(action);
            int emotionIndex = ArgMax(emotion);
            int dialogueIndex = ArgMax(dialogue);

            return new NPCHybridBrainResult
            {
                action = (PlayerActionType)actionIndex,
                emotion = (EnemyHeadCalmController.EmotionState)emotionIndex,
                dialogueIntent = (NPCDialogueIntent)dialogueIndex,
                confidence = SoftmaxConfidence(action, actionIndex),
                dialogueChance = 0.35f
            };
        }

        // Превращаем input → float[]
        private float[] BuildFeatures(NPCHybridBrainInput i)
        {
            return new float[]
            {
                i.aiHandStrength,
                i.riskLevel,
                i.potPressure,
                i.callPressure,
                i.aiHpNormalized,
                i.playerHpNormalized,
                i.playerAggression,
                i.playerSuspicion,
                i.isPreflop,
                i.isFlop,
                i.isTurn,
                i.isRiver,
                i.canCheck,
                i.canCall,
                i.canRaise,
                i.canFold,
                i.knifeAvailable,
                i.randomMood
            };
        }

        // Поиск максимального значения
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

        // Уверенность (softmax)
        private float SoftmaxConfidence(float[] logits, int index)
        {
            float max = logits[0];

            for (int i = 1; i < logits.Length; i++)
                if (logits[i] > max)
                    max = logits[i];

            float sum = 0f;

            for (int i = 0; i < logits.Length; i++)
                sum += Mathf.Exp(logits[i] - max);

            return Mathf.Exp(logits[index] - max) / sum;
        }

        private void OnDestroy()
        {
            worker?.Dispose(); // освобождаем память
        }
    }
}