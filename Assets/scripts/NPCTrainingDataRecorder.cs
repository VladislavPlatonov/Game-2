using System.Globalization; // нужен для корректной записи float с точкой
using System.IO;            // работа с файлами
using UnityEngine;

namespace Poker
{
    public static class NPCTrainingDataRecorder
    {
        // Путь к файлу (в папке AppData игры)
        private static readonly string FilePath =
            Path.Combine(Application.persistentDataPath, "npc_training_data.csv");

        public static void SaveExample(NPCHybridBrainInput input, NPCHybridBrainResult result)
        {
            // Проверяем, нужно ли записывать заголовок (если файл создаётся впервые)
            bool writeHeader = !File.Exists(FilePath);

            using StreamWriter writer = new StreamWriter(FilePath, true);

            // Заголовки колонок
            if (writeHeader)
            {
                writer.WriteLine(
                    "aiHandStrength,riskLevel,potPressure,callPressure," +
                    "aiHpNormalized,playerHpNormalized,playerAggression,playerSuspicion," +
                    "isPreflop,isFlop,isTurn,isRiver," +
                    "canCheck,canCall,canRaise,canFold," +
                    "knifeAvailable,randomMood," +
                    "action,emotion,dialogue"
                );
            }

            // Запись одной строки данных
            writer.WriteLine(string.Join(",",
                F(input.aiHandStrength),      // сила руки
                F(input.riskLevel),           // риск
                F(input.potPressure),         // давление банка
                F(input.callPressure),        // давление ставки

                F(input.aiHpNormalized),      // HP NPC
                F(input.playerHpNormalized),  // HP игрока
                F(input.playerAggression),    // агрессия игрока
                F(input.playerSuspicion),     // подозрительность

                F(input.isPreflop),
                F(input.isFlop),
                F(input.isTurn),
                F(input.isRiver),

                F(input.canCheck),
                F(input.canCall),
                F(input.canRaise),
                F(input.canFold),

                F(input.knifeAvailable),
                F(input.randomMood),

                ((int)result.action).ToString(),        // действие
                ((int)result.emotion).ToString(),       // эмоция
                ((int)result.dialogueIntent).ToString() // реплика
            ));
        }

        // Преобразование float → строка с точкой (важно для Python)
        private static string F(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string GetFilePath()
        {
            return FilePath;
        }
    }
}