using TMPro;
using UnityEngine;

namespace Poker
{
    public class NPCDialogueController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("Timing")]
        [SerializeField] private float showDuration = 2.5f;

        private float hideTime;

        private void Update()
        {
            if (dialoguePanel != null && dialoguePanel.activeSelf && Time.time >= hideTime)
                dialoguePanel.SetActive(false);
        }

        public void TryShow(NPCDialogueIntent intent, float chance)
        {
            if (intent == NPCDialogueIntent.None)
                return;

            if (Random.value > chance)
                return;

            Show(GetLine(intent));
        }

        private void Show(string text)
        {
            if (dialoguePanel == null || dialogueText == null)
                return;

            dialogueText.text = text;
            dialoguePanel.SetActive(true);
            hideTime = Time.time + showDuration;
        }

        private string GetLine(NPCDialogueIntent intent)
        {
            switch (intent)
            {
                case NPCDialogueIntent.CalmComment:
                    return Random.value < 0.5f ? "Посмотрим." : "Спокойный ход.";

                case NPCDialogueIntent.NervousLie:
                    return Random.value < 0.5f ? "Я не уверен..." : "Наверное, рискну.";

                case NPCDialogueIntent.ConfidentTaunt:
                    return Random.value < 0.5f ? "Ты уверен?" : "Интересный выбор.";

                case NPCDialogueIntent.SuspiciousQuestion:
                    return Random.value < 0.5f ? "Ты что-то задумал?" : "Слишком уверенно.";

                case NPCDialogueIntent.AggressiveThreat:
                    return Random.value < 0.5f ? "Давай выше." : "Не отступай.";

                case NPCDialogueIntent.KnifeRemark:
                    return Random.value < 0.5f ? "Не отвлекайся от карт." : "Иногда ставка — не самое опасное.";

                default:
                    return "";
            }
        }
    }
}