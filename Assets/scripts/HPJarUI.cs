using TMPro;
using UnityEngine;
using Poker;

public class HPJarUI : MonoBehaviour
{
    [Header("3D TMP Text")]
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text aiHpText;

    [Header("Labels")]
    [SerializeField] private string playerPrefix = "HP: ";
    [SerializeField] private string aiPrefix = "HP: ";

    private SoulManager souls;

    private int lastPlayerHp = -9999;
    private int lastAiHp = -9999;

    private void Awake()
    {
        souls = SoulManager.Instance;
        if (souls == null)
            souls = FindFirstObjectByType<SoulManager>();
    }

    private void Start()
    {
        RefreshNow();
    }

    private void Update()
    {
        if (souls == null)
        {
            souls = SoulManager.Instance;
            if (souls == null)
                souls = FindFirstObjectByType<SoulManager>();
        }

        if (souls == null) return;

        int currentPlayerHp = souls.GetPlayerSouls();
        int currentAiHp = souls.GetAISouls();

        if (currentPlayerHp != lastPlayerHp)
        {
            lastPlayerHp = currentPlayerHp;
            if (playerHpText != null)
                playerHpText.text = playerPrefix + currentPlayerHp;
        }

        if (currentAiHp != lastAiHp)
        {
            lastAiHp = currentAiHp;
            if (aiHpText != null)
                aiHpText.text = aiPrefix + currentAiHp;
        }
    }

    public void RefreshNow()
    {
        if (souls == null) return;

        lastPlayerHp = souls.GetPlayerSouls();
        lastAiHp = souls.GetAISouls();

        if (playerHpText != null)
            playerHpText.text = playerPrefix + lastPlayerHp;

        if (aiHpText != null)
            aiHpText.text = aiPrefix + lastAiHp;
    }
}
